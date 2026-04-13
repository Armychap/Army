using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingCartWithUndo
{
    // Класс товара - отвечает за представление товара
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public Product(string id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }

        public override string ToString()
        {
            return $"{Name} (${Price})";
        }
    }

    // Класс элемента корзины - отвечает за элемент в корзине
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; } // Скидка на единицу товара

        public decimal TotalPrice => (Product.Price - Discount) * Quantity;

        public CartItem(Product product, int quantity, decimal discount = 0)
        {
            Product = product;
            Quantity = quantity;
            Discount = discount;
        }

        public override string ToString()
        {
            return $"{Product.Name} x{Quantity} = ${TotalPrice} (скидка: ${Discount}/шт)";
        }
    }

    // Класс корзины - отвечает за управление товарами в корзине
    public class ShoppingCart
    {
        private List<CartItem> _items = new List<CartItem>();

        public IReadOnlyList<CartItem> Items => _items;

        public void AddItem(Product product, int quantity)
        {
            var existing = _items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _items.Add(new CartItem(product, quantity));
            }
        }

        public void RemoveItem(string productId)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        public void ChangeQuantity(string productId, int newQuantity)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                if (newQuantity <= 0)
                {
                    _items.Remove(item);
                }
                else
                {
                    item.Quantity = newQuantity;
                }
            }
        }

        public void ApplyDiscount(string productId, decimal discount)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                item.Discount = discount;
            }
        }

        public decimal GetTotal()
        {
            return _items.Sum(i => i.TotalPrice);
        }

        public void Display()
        {
            Console.WriteLine("\n=== Корзина ===");
            if (!_items.Any())
            {
                Console.WriteLine("Корзина пуста");
            }
            else
            {
                foreach (var item in _items)
                {
                    Console.WriteLine($"  {item}");
                }
                Console.WriteLine($"Итого: ${GetTotal()}");
            }
            Console.WriteLine("===============\n");
        }
    }
}