using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingCartCommandPattern
{
    // Модель товара в корзине.
    public class CartItem
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal Total => Price * Quantity * (1 - DiscountPercent / 100);
    }

    // Получатель (Receiver) в паттерне Command
    // Содержит реальную бизнес-логику работы с корзиной
    public class ShoppingCart
    {
        private List<CartItem> items = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            var existing = items.FirstOrDefault(i => i.Id == item.Id);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                items.Add(item);
            Console.WriteLine($"Добавлен: {item.Name} x{item.Quantity}");
        }

        public void RemoveItem(string id)
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                items.Remove(item);
                Console.WriteLine($"Удален: {item.Name}");
            }
        }

        public void ChangeQuantity(string id, int newQty)
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                Console.WriteLine($"{item.Name}: {item.Quantity} -> {newQty}");
                item.Quantity = newQty;
            }
        }

        public void ApplyDiscount(string id, decimal percent)
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                Console.WriteLine($"Скидка {percent}% на {item.Name}");
                item.DiscountPercent = percent;
            }
        }

        public void Show()
        {
            Console.WriteLine("\nКорзина:");
            if (!items.Any())
                Console.WriteLine("Корзина пуста");
            else
            {
                foreach (var i in items)
                    Console.WriteLine($"{i.Name} | {i.Quantity} шт | {i.Price:C} | скидка {i.DiscountPercent}% | итог: {i.Total:C}");
                Console.WriteLine($"Общая сумма: {items.Sum(i => i.Total):C}");
            }
            Console.WriteLine();
        }
    }
}