using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingCartCommandPattern
{
    public interface ICommand
    {
        void Execute(); // выполнить действие
        void Undo();    // отменить действие
        string Description { get; } // описание команды для логирования
    }

    // Команда для добавления товара в корзину
    public class AddToCartCommand : ICommand
    {
        private ShoppingCart cart;      // ссылка на корзину (получатель)
        private Product product;        // добавляемый товар
        private int quantity;           // количество
        private string? createdCartItemId; // ID созданной позиции (если товар добавлялся впервые)
        private bool wasExisting;       // был ли товар уже в корзине до выполнения
        private int oldQuantity;        // старое количество (если товар уже был)
        
        public string Description => $"Добавить {product.Name} x{quantity}";

        public AddToCartCommand(ShoppingCart cart, Product product, int quantity = 1)
        {
            this.cart = cart;
            this.product = product;
            this.quantity = quantity;
        }

        // Выполняет добавление товара в корзину
        public void Execute()
        {
            // Запоминаем состояние до выполнения
            wasExisting = cart.HasProduct(product.ProductId);
            
            if (wasExisting)
            {
                // Если товар уже был - запоминаем его ID и текущее количество
                var existingItem = cart.GetCartItemByProductId(product.ProductId);
                if (existingItem != null)
                {
                    createdCartItemId = existingItem.CartItemId;
                    oldQuantity = existingItem.Quantity;
                }
            }
            
            // Выполняем основное действие
            cart.AddItem(product, quantity);
        }
        
        // Отменяет добавление товара
        public void Undo()
        {
            if (wasExisting && createdCartItemId != null)
            {
                // Если товар уже был - возвращаем старое количество
                cart.ChangeQuantity(createdCartItemId, oldQuantity);
            }
            else if (!wasExisting)
            {
                // Если товар был добавлен впервые - полностью удаляем позицию
                var addedItem = cart.GetCartItemByProductId(product.ProductId);
                if (addedItem != null)
                {
                    cart.RemoveItem(addedItem.CartItemId);
                }
            }
        }
    }

    // Команда для удаления товара из корзины
    public class RemoveFromCartCommand : ICommand
    {
        private ShoppingCart cart;      // ссылка на корзину
        private string cartItemId;      // ID позиции для удаления
        private CartItem? removedItem;  // сохраненная копия удаляемой позиции (для отмены)
        public string Description => $"Удалить позицию {cartItemId}";

        public RemoveFromCartCommand(ShoppingCart cart, string cartItemId)
        {
            this.cart = cart;
            this.cartItemId = cartItemId;
        }

        // Выполняет удаление (предварительно сохраняя копию удаляемого объекта)
        public void Execute()
        {
            removedItem = cart.GetCartItem(cartItemId)?.Clone(); // сохраняем копию
            if (removedItem != null)
                cart.RemoveItem(cartItemId);
        }

        // Отменяет удаление - восстанавливает позицию с сохраненными параметрами
        public void Undo()
        {
            if (removedItem != null)
            {
                // Создаем товар из сохраненных данных
                var product = new Product
                {
                    ProductId = removedItem.ProductId,
                    Name = removedItem.Name,
                    Price = removedItem.OriginalPrice
                };
                cart.AddItem(product, removedItem.Quantity);
                
                // Восстанавливаем цену и скидку, если они были
                var restoredItem = cart.GetCartItemByProductId(removedItem.ProductId);
                if (restoredItem != null && removedItem.DiscountPercent > 0)
                {
                    restoredItem.CurrentPrice = removedItem.CurrentPrice;
                    restoredItem.DiscountPercent = removedItem.DiscountPercent;
                }
            }
        }
    }

    // Команда для изменения количества товара
    public class ChangeQuantityCommand : ICommand
    {
        private ShoppingCart cart;      // ссылка на корзину
        private string cartItemId;      // ID позиции
        private int newQuantity;        // новое количество
        private int oldQuantity;        // старое количество (сохраняется при выполнении)
        
        public string Description => $"Изменить кол-во позиции {cartItemId} с {oldQuantity} на {newQuantity}";

        public ChangeQuantityCommand(ShoppingCart cart, string cartItemId, int newQuantity)
        {
            this.cart = cart;
            this.cartItemId = cartItemId;
            this.newQuantity = newQuantity;
        }

        // Выполняет изменение количества (запоминает старое значение)
        public void Execute()
        {
            var item = cart.GetCartItem(cartItemId);
            if (item != null)
            {
                oldQuantity = item.Quantity; // сохраняем старое количество
                cart.ChangeQuantity(cartItemId, newQuantity);
            }
        }
        
        // Отменяет изменение - возвращает старое количество
        public void Undo()
        {
            cart.ChangeQuantity(cartItemId, oldQuantity);
        }
    }

    // Команда для применения скидки к товару
    // Скидки применяются последовательно к текущей цене
    public class ApplyDiscountCommand : ICommand
    {
        private ShoppingCart cart;          // ссылка на корзину
        private string cartItemId;          // ID позиции
        private decimal discountPercent;    // процент скидки для применения
        private decimal oldCurrentPrice;    // сохраненная цена до применения скидки
        private decimal oldDiscountPercent; // сохраненный процент скидки до применения
        
        public string Description => $"Скидка {discountPercent}% на позицию {cartItemId}";

        public ApplyDiscountCommand(ShoppingCart cart, string cartItemId, decimal discountPercent)
        {
            this.cart = cart;
            this.cartItemId = cartItemId;
            this.discountPercent = discountPercent;
        }

        // Выполняет применение скидки (сохраняет состояние до изменения)
        public void Execute()
        {
            var item = cart.GetCartItem(cartItemId);
            if (item != null)
            {
                // Сохраняем текущее состояние перед применением скидки
                oldCurrentPrice = item.CurrentPrice;
                oldDiscountPercent = item.DiscountPercent;
                cart.ApplyDiscount(cartItemId, discountPercent);
            }
        }
        
        // Отменяет скидку - полностью восстанавливает цену и процент скидки
        public void Undo()
        {
            var item = cart.GetCartItem(cartItemId);
            if (item != null)
            {
                // Возвращаем сохраненные цену и процент скидки
                item.CurrentPrice = oldCurrentPrice;
                item.DiscountPercent = oldDiscountPercent;
                Console.WriteLine($"Восстановлена цена {item.Name}: {item.CurrentPrice:C} (скидка {item.DiscountPercent}%)");
            }
        }
    }

    // Составная команда (макрокоманда) - объединяет несколько команд в одну
    // Позволяет применять промокод, который включает несколько скидок
    public class ApplyPromoCodeCommand : ICommand
    {
        private List<ICommand> commands = new List<ICommand>(); // список вложенных команд
        private string promoCode;                               // название промокода
        public string Description => $"Промокод {promoCode} ({commands.Count} скидок)";

        public ApplyPromoCodeCommand(string promoCode, List<ICommand> commands)
        {
            this.promoCode = promoCode;
            this.commands = commands;
        }

        // Выполняет все вложенные команды последовательно
        public void Execute()
        {
            Console.WriteLine($"Применяем промокод: {promoCode}");
            foreach (var cmd in commands)
                cmd.Execute(); // каждая команда сама заботится о сохранении своего состояния
        }

        // Отменяет все вложенные команды в обратном порядке
        public void Undo()
        {
            Console.WriteLine($"Отменяем промокод: {promoCode}");
            foreach (var cmd in commands)
                cmd.Undo(); // отменяем каждую команду
        }
    }
}