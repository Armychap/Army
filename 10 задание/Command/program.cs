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

            // Товары из магазина (ProductId - ID в магазине)
            var laptop = new Product { ProductId = "P100", Name = "Ноут", Price = 50000 };
            var mouse = new Product { ProductId = "P200", Name = "Мышка", Price = 1500 };
            var keyboard = new Product { ProductId = "P300", Name = "Клавиатура", Price = 3000 };

            // Выполняем команды через Invoker, который управляет историей для Undo/Redo.
            invoker.Execute(new AddToCartCommand(cart, laptop, 1));
            invoker.Execute(new AddToCartCommand(cart, mouse, 2));
            cart.Show();

            // Получаем CartItemId для мышки (ID позиции в корзине)
            var mouseCartItem = cart.GetCartItemByProductId("P200");
            // Изменяем количество товара через команду
            invoker.Execute(new ChangeQuantityCommand(cart, mouseCartItem.CartItemId, 3));
            cart.Show();

            // Получаем CartItemId для ноутбука
            var laptopCartItem = cart.GetCartItemByProductId("P100");
            // Применяем скидку 10%
            invoker.Execute(new ApplyDiscountCommand(cart, laptopCartItem.CartItemId, 10));
            cart.Show();

            // Применяем скидку 20% поверх существующей (считается от текущей цены 45000)
            invoker.Execute(new ApplyDiscountCommand(cart, laptopCartItem.CartItemId, 20));
            cart.Show();

            // Отменяем последнее действие (скидку 20%)
            invoker.Undo();
            cart.Show();

            // Отменяем еще раз (скидку 10%)
            invoker.Undo();
            cart.Show();

            // объединяем несколько команд в одно 
            var promoCommands = new List<ICommand>
            {
                new ApplyDiscountCommand(cart, laptopCartItem.CartItemId, 15),
                new ApplyDiscountCommand(cart, mouseCartItem.CartItemId, 20)
            };
            invoker.Execute(new ApplyPromoCodeCommand("Распродажа", promoCommands));
            cart.Show();

            // Отмена макрокоманды: Undo отменяет всю группу команд, выполненных как одну операцию
            invoker.Undo();
            cart.Show();

            // Повтор последней отмененной команды / макрокоманды
            invoker.Redo();
            cart.Show();

            // Добавляем клавиатуру
            invoker.Execute(new AddToCartCommand(cart, keyboard, 1));
            cart.Show();

            // Добавляем еще один ноутбук
            invoker.Execute(new AddToCartCommand(cart, laptop, 1));
            cart.Show();

            invoker.ShowAnalytics();
        }
    }
}