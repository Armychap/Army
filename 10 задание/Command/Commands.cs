using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingCartCommandPattern
{
    // Интерфейс команды: все команды должны выполнять действие и отменять его
    public interface ICommand
    {
        void Execute();
        void Undo();
        string Description { get; }
    }

    // добавление товара в корзину
    public class AddToCartCommand : ICommand
    {
        private ShoppingCart cart;
        private Product product;
        private int quantity;
        private string? createdCartItemId;
        private bool wasExisting;
        private int oldQuantity;
        
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

    // удаление товара из корзины
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

    // изменение количества товара
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

    // применение скидки к товару - скидки применяются последовательно к текущей цене
    public class ApplyDiscountCommand : ICommand
    {
        private ShoppingCart cart;
        private string cartItemId;
        private decimal discountPercent;
        private decimal oldCurrentPrice;
        private decimal oldDiscountPercent;
        
        public string Description => $"Скидка {discountPercent}% на позицию {cartItemId}";

        public ApplyDiscountCommand(ShoppingCart cart, string cartItemId, decimal discountPercent)
        {
            this.cart = cart;
            this.cartItemId = cartItemId;
            this.discountPercent = discountPercent;
        }

        public void Execute()
        {
            var item = cart.GetCartItem(cartItemId);
            if (item != null)
            {
                oldCurrentPrice = item.CurrentPrice;
                oldDiscountPercent = item.DiscountPercent;
                cart.ApplyDiscount(cartItemId, discountPercent);
            }
        }
        
        public void Undo()
        {
            var item = cart.GetCartItem(cartItemId);
            if (item != null)
            {
                item.CurrentPrice = oldCurrentPrice;
                item.DiscountPercent = oldDiscountPercent;
                Console.WriteLine($"Восстановлена цена {item.Name}: {item.CurrentPrice:C} (скидка {item.DiscountPercent}%)");
            }
        }
    }

    // компонуем несколько команд в одну логическую операцию
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