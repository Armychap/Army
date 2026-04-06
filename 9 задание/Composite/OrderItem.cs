using System;
using System.Collections.Generic;

namespace CompositeOrderSystem
{
    // Листовой элемент (Leaf) в паттерне Composite.
    class OrderItem : OrderComponent
    {
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }

        public OrderItem(string name, decimal price, int quantity, string category = "Общее")
            : base(name)
        {
            Price = price;
            Quantity = quantity;
            Category = category;
        }

        // Общая стоимость отдельного товара
        public override decimal GetTotalPrice()
        {
            return Price * Quantity;
        }

        // Количество штук товара
        public override int GetTotalItems()
        {
            return Quantity;
        }

        // Цена товара как средняя цена для единицы
        public override decimal GetAveragePrice()
        {
            return Price;
        }

        // Возвращает собственное название товара
        public override List<string> GetAllNames()
        {
            return new List<string> { $"{Name} (x{Quantity})" };
        }

        // Печатает информацию о товаре с отступом
        public override void PrintOrder(int depth = 0)
        {
            Console.WriteLine($"{new string(' ', depth * 2)}{Name} | Кат: {Category} | {Quantity} шт | {GetTotalPrice():C} (сред: {Price:C})");
        }

        public override void Add(OrderComponent component) { }
        public override void Remove(OrderComponent component) { }
    }
}
