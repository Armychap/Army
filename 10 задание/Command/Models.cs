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
        public string CartItemId { get; set; } = string.Empty; // id позиции в корзине
        public string ProductId { get; set; } = string.Empty; // Ссылка на id товара в магазине
        public string Name { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; } // исходная цена
        public decimal CurrentPrice { get; set; }  // текущая цена (с учетом всех скидок)
        public int Quantity { get; set; } // количество товара в позиции
        public decimal DiscountPercent { get; set; } // общий процент скидки (для отображения)
        public decimal Total => CurrentPrice * Quantity; // итоговая стоимость позиции с учетом количества и скидок

        // Создает глубокую копию текущего объекта CartItem
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
        private List<CartItem> items = new List<CartItem>(); // список позиций в корзине
        private int nextCartItemId = 1; // Генератор ID для позиций в корзине

        // Генерация уникального ID для каждой позиции в корзине
        private string GenerateCartItemId() => (nextCartItemId++).ToString();

        // проверка наличия товара в корзине по ID товара
        public bool HasProduct(string productId) => items.Any(i => i.ProductId == productId);

        // получение позиции в корзине по ID позиции
        public CartItem? GetCartItem(string cartItemId) => items.FirstOrDefault(i => i.CartItemId == cartItemId);

        // получение позиции в корзине по ID товара
        public CartItem? GetCartItemByProductId(string productId) => items.FirstOrDefault(i => i.ProductId == productId);

        // Добавляет товар в корзину или увеличивает количество, если уже есть
        public void AddItem(Product product, int quantity = 1)
        {
            // Проверяем, есть ли уже такой товар в корзине
            var existing = items.FirstOrDefault(i => i.ProductId == product.ProductId);

            if (existing != null)
            {
                // Если товар уже есть - увеличиваем количество
                existing.Quantity += quantity;
                Console.WriteLine($"Добавлен: {product.Name} x{quantity}");
            }
            else
            {
                // Если товара нет - создаем новую позицию
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

        // Удаляет позицию из корзины по ID позиции
        public void RemoveItem(string cartItemId)
        {
            var item = items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item != null)
            {
                items.Remove(item);
                Console.WriteLine($"Удален: {item.Name}");
            }
        }

        // Изменяет количество товара в указанной позиции
        // Если новое количество <= 0 - удаляет позицию
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

        // Применяет скидку к указанной позиции
        // Скидка применяется к текущей цене (цепочка скидок)
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

        // Отображает текущее содержимое корзины и общую сумму
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