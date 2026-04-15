using System;
using System.Collections.Generic;

namespace ShoppingCartCommandPattern
{
    class Program
    {
        static void Main()
        {
            var cart = new ShoppingCart();
            var invoker = new CartInvoker();

            var laptop = new CartItem { Id = "1", Name = "Ноутбук", Price = 50000, Quantity = 1 };
            var mouse = new CartItem { Id = "2", Name = "Мышь", Price = 1500, Quantity = 2 };

            // Исполняем конкретные команды через Invoker. Каждая команда знает, что делать с корзиной.
            invoker.Execute(new AddToCartCommand(cart, laptop));
            invoker.Execute(new AddToCartCommand(cart, mouse));
            cart.Show();

            // Изменяем количество товара и применяем скидку через отдельные команды.
            invoker.Execute(new ChangeQuantityCommand(cart, "2", 3));
            cart.Show();

            invoker.Execute(new ApplyDiscountCommand(cart, "1", 10));
            cart.Show();

            // Отменяем последнее действие через Invoker: паттерн Command поддерживает Undo.
            invoker.Undo();
            cart.Show();

            // Макрокоманда: объединяем несколько команд в одно логическое действие.
            var promoCommands = new List<ICommand>
            {
                new ApplyDiscountCommand(cart, "1", 15),
                new ApplyDiscountCommand(cart, "2", 20)
            };
            invoker.Execute(new ApplyPromoCodeCommand("BLACKFRIDAY", promoCommands));
            cart.Show();

            // Отмена макрокоманды: Undo отменяет всю группу команд, выполненных как одну операцию.
            invoker.Undo();
            cart.Show();

            // Повтор последней отмененной команды / макрокоманды.
            invoker.Redo();
            cart.Show();

            invoker.ShowAnalytics();
        }
    }
}