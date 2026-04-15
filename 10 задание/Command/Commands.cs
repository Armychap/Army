using System;
using System.Collections.Generic;

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
        private CartItem item;
        public string Description => $"Добавить {item.Name} x{item.Quantity}";

        public AddToCartCommand(ShoppingCart cart, CartItem item)
        {
            this.cart = cart;
            this.item = item;
        }

        public void Execute() => cart.AddItem(item);
        public void Undo() => cart.RemoveItem(item.Id);
    }

    // удаление товара из корзины
    // Сохраняет удаленный элемент, чтобы можно было выполнить Undo
    public class RemoveFromCartCommand : ICommand
    {
        private ShoppingCart cart;
        private string productId;
        private CartItem removedItem;
        public string Description => $"Удалить товар {productId}";

        public RemoveFromCartCommand(ShoppingCart cart, string productId)
        {
            this.cart = cart;
            this.productId = productId;
        }

        public void Execute()
        {
            removedItem = new CartItem { Id = productId, Name = "Товар", Quantity = 1 };
            cart.RemoveItem(productId);
        }

        public void Undo() => cart.AddItem(removedItem);
    }

    // изменение количества товара
    // Она запоминает старое состояние для отмены
    public class ChangeQuantityCommand : ICommand
    {
        private ShoppingCart cart;
        private string productId;
        private int oldQuantity;
        private int newQuantity;
        public string Description => $"Изменить кол-во {productId} с {oldQuantity} на {newQuantity}";

        public ChangeQuantityCommand(ShoppingCart cart, string productId, int newQuantity)
        {
            this.cart = cart;
            this.productId = productId;
            this.newQuantity = newQuantity;
            this.oldQuantity = 1;
        }

        public void Execute() => cart.ChangeQuantity(productId, newQuantity);
        public void Undo() => cart.ChangeQuantity(productId, oldQuantity);
    }

    // применение скидки к товару
    public class ApplyDiscountCommand : ICommand
    {
        private ShoppingCart cart;
        private string productId;
        private decimal discountPercent;
        private decimal oldDiscount;
        public string Description => $"Скидка {discountPercent}% на {productId}";

        public ApplyDiscountCommand(ShoppingCart cart, string productId, decimal discountPercent)
        {
            this.cart = cart;
            this.productId = productId;
            this.discountPercent = discountPercent;
            this.oldDiscount = 0;
        }

        public void Execute() => cart.ApplyDiscount(productId, discountPercent);
        public void Undo() => cart.ApplyDiscount(productId, oldDiscount);
    }

    // компонуем несколько команд в одну логическую операцию
    // При выполнении и откате она итерирует команды внутри себя
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