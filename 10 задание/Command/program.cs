using System;
using System.Collections.Generic;

namespace ShoppingCartCommandPattern
{
    class Program
    {
        static void Main()
        {
            var cart = new ShoppingCart(); // Получатель (Receiver) 
            var invoker = new CartInvoker(); // Invoker, который будет управлять командами и их историей

            var laptop = new CartItem { Id = "1", Name = "Ноут", Price = 50000, Quantity = 1 };
            var mouse = new CartItem { Id = "2", Name = "Мышка", Price = 1500, Quantity = 2 };

            // Выполняем команды через Invoker, который управляет историей для Undo/Redo.
            invoker.Execute(new AddToCartCommand(cart, laptop));
            invoker.Execute(new AddToCartCommand(cart, mouse));
            cart.Show();

            // Изменяем количество товара и применяем скидку через отдельные команды
            invoker.Execute(new ChangeQuantityCommand(cart, "2", 3));
            cart.Show();

            invoker.Execute(new ApplyDiscountCommand(cart, "1", 10)); // скидка 10% на ноутбук
            cart.Show();

            // Отменяем последнее действие
            invoker.Undo();
            cart.Show();

            // объединяем несколько команд в одно 
            var promoCommands = new List<ICommand>
            {
                new ApplyDiscountCommand(cart, "1", 15),
                new ApplyDiscountCommand(cart, "2", 20)
            };
            invoker.Execute(new ApplyPromoCodeCommand("Распродажа", promoCommands));
            cart.Show();

            // Отмена макрокоманды: Undo отменяет всю группу команд, выполненных как одну операцию
            invoker.Undo();
            cart.Show();

            // Повтор последней отмененной команды / макрокоманды
            invoker.Redo();
            cart.Show();

            invoker.ShowAnalytics();
        }
    }
}