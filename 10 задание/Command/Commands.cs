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

        public void Execute()
        {
            wasExisting = cart.HasProduct(product.ProductId);
            
            if (wasExisting)
            {
                var existingItem = cart.GetCartItemByProductId(product.ProductId);
                if (existingItem != null)
                {
                    createdCartItemId = existingItem.CartItemId;
                    oldQuantity = existingItem.Quantity;
                }
            }
            
            cart.AddItem(product, quantity);
        }
        
        public void Undo()
        {
            if (wasExisting && createdCartItemId != null)
            {
                cart.ChangeQuantity(createdCartItemId, oldQuantity);
            }
            else if (!wasExisting)
            {
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
        private ShoppingCart cart;
        private string cartItemId;
        private CartItem? removedItem;
        public string Description => $"Удалить позицию {cartItemId}";

        public RemoveFromCartCommand(ShoppingCart cart, string cartItemId)
        {
            this.cart = cart;
            this.cartItemId = cartItemId;
        }

        public void Execute()
        {
            removedItem = cart.GetCartItem(cartItemId)?.Clone();
            if (removedItem != null)
                cart.RemoveItem(cartItemId);
        }

        public void Undo()
        {
            if (removedItem != null)
            {
                var product = new Product
                {
                    ProductId = removedItem.ProductId,
                    Name = removedItem.Name,
                    Price = removedItem.OriginalPrice
                };
                cart.AddItem(product, removedItem.Quantity);
                
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
        private ShoppingCart cart;
        private string cartItemId;
        private int newQuantity;
        private int oldQuantity;
        
        public string Description => $"Изменить кол-во позиции {cartItemId} с {oldQuantity} на {newQuantity}";

        public ChangeQuantityCommand(ShoppingCart cart, string cartItemId, int newQuantity)
        {
            this.cart = cart;
            this.cartItemId = cartItemId;
            this.newQuantity = newQuantity;
        }

        public void Execute()
        {
            var item = cart.GetCartItem(cartItemId);
            if (item != null)
            {
                oldQuantity = item.Quantity;
                cart.ChangeQuantity(cartItemId, newQuantity);
            }
        }
        
        public void Undo()
        {
            cart.ChangeQuantity(cartItemId, oldQuantity);
        }
    }

    // Команда для применения скидки к товару (упрощённая - без хранения состояния)
    public class ApplyDiscountCommand : ICommand
    {
        private ShoppingCart cart;
        private string cartItemId;
        private decimal discountPercent;
        
        public string Description => $"Скидка {discountPercent}% на позицию {cartItemId}";

        public ApplyDiscountCommand(ShoppingCart cart, string cartItemId, decimal discountPercent)
        {
            this.cart = cart;
            this.cartItemId = cartItemId;
            this.discountPercent = discountPercent;
        }

        public void Execute()
        {
            cart.ApplyDiscount(cartItemId, discountPercent);
        }
        
        public void Undo()
        {
            cart.UndoLastDiscount(cartItemId);
        }
    }

    // Составная команда (макрокоманда) - объединяет несколько команд в одну
    public class ApplyPromoCodeCommand : ICommand
    {
        private List<ICommand> commands = new List<ICommand>();
        private string promoCode;
        
        public string Description => $"Промокод {promoCode} ({commands.Count} скидок)";

        public ApplyPromoCodeCommand(string promoCode, List<ICommand> commands)
        {
            this.promoCode = promoCode;
            this.commands = commands;
        }

        public void Execute()
        {
            Console.WriteLine($"Применяем промокод: {promoCode}");
            foreach (var cmd in commands)
                cmd.Execute();
        }

        public void Undo()
        {
            Console.WriteLine($"Отменяем промокод: {promoCode}");
            foreach (var cmd in commands)
                cmd.Undo();
        }
    }
}