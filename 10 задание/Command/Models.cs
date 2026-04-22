using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoppingCartCommandPattern
{
    // Модель товара в магазине
    public class Product
    {
        public string ProductId { get; set; } = string.Empty; // ID товара в магазине
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    // Модель позиции в корзине
    public class CartItem
    {
        public string CartItemId { get; set; } = string.Empty; // Уникальный ID позиции в корзине
        public string ProductId { get; set; } = string.Empty; // Ссылка на ID товара в магазине
        public string Name { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; } // исходная цена
        public decimal CurrentPrice { get; set; }  // текущая цена (с учетом всех скидок)
        public int Quantity { get; set; }
        public decimal DiscountPercent { get; set; } // общий процент скидки (для отображения)
        public decimal Total => CurrentPrice * Quantity;

        public CartItem Clone()
        {
            return new CartItem
            {
                CartItemId = this.CartItemId,
                ProductId = this.ProductId,
                Name = this.Name,
                OriginalPrice = this.OriginalPrice,
                CurrentPrice = this.CurrentPrice,
                Quantity = this.Quantity,
                DiscountPercent = this.DiscountPercent
            };
        }
    }

    // Получатель (Receiver) - бизнес-логика работы с корзиной
    public class ShoppingCart
    {
        private List<CartItem> items = new List<CartItem>();
        private int nextCartItemId = 1; // Генератор ID для позиций в корзине

        private string GenerateCartItemId() => (nextCartItemId++).ToString();

        public bool HasProduct(string productId) => items.Any(i => i.ProductId == productId);

        public CartItem? GetCartItem(string cartItemId) => items.FirstOrDefault(i => i.CartItemId == cartItemId);

        public CartItem? GetCartItemByProductId(string productId) => items.FirstOrDefault(i => i.ProductId == productId);

        public void AddItem(Product product, int quantity = 1)
        {
            // Проверяем, есть ли уже такой товар в корзине
            var existing = items.FirstOrDefault(i => i.ProductId == product.ProductId);

            if (existing != null)
            {
                existing.Quantity += quantity;
                Console.WriteLine($"Добавлен: {product.Name} x{quantity}");
            }
            else
            {
                var newItem = new CartItem
                {
                    CartItemId = GenerateCartItemId(),
                    ProductId = product.ProductId,
                    Name = product.Name,
                    OriginalPrice = product.Price,
                    CurrentPrice = product.Price,
                    Quantity = quantity,
                    DiscountPercent = 0
                };
                items.Add(newItem);
                Console.WriteLine($"Добавлен: {product.Name} x{quantity}");
            }
        }

        public void RemoveItem(string cartItemId)
        {
            var item = items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item != null)
            {
                items.Remove(item);
                Console.WriteLine($"Удален: {item.Name}");
            }
        }

        public void ChangeQuantity(string cartItemId, int newQty)
        {
            var item = items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item != null && newQty > 0)
            {
                Console.WriteLine($"{item.Name}: {item.Quantity} -> {newQty}");
                item.Quantity = newQty;
            }
            else if (item != null && newQty <= 0)
            {
                RemoveItem(cartItemId);
            }
        }

        public void ApplyDiscount(string cartItemId, decimal percent)
        {
            var item = items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item != null)
            {
                // применяем скидку к текущей цене
                item.CurrentPrice = item.CurrentPrice * (1 - percent / 100);

                // пересчитываем общий процент скидки от исходной цены
                decimal totalDiscountPercent = (1 - item.CurrentPrice / item.OriginalPrice) * 100;
                item.DiscountPercent = Math.Round(totalDiscountPercent, 2);

                Console.WriteLine($"Скидка {percent}% на {item.Name} → цена: {item.CurrentPrice:C} (общая скидка {item.DiscountPercent}%)");
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
                    Console.WriteLine($"{i.Name} | {i.Quantity} шт | {i.OriginalPrice:C} | скидка {i.DiscountPercent}% | цена: {i.CurrentPrice:C} | итог: {i.Total:C}");
                Console.WriteLine($"Общая сумма: {items.Sum(i => i.Total):C}");
            }
            Console.WriteLine();
        }
    }
}