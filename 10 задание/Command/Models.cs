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
        private List<CartItem> items = new List<CartItem>();
        private int nextCartItemId = 1;

        // История изменений цен для каждой позиции
        private Dictionary<string, Stack<PriceSnapshot>> priceHistory = new Dictionary<string, Stack<PriceSnapshot>>();

        // Класс для хранения снимка цены
        private class PriceSnapshot
        {
            public decimal CurrentPrice { get; set; }
            public decimal DiscountPercent { get; set; }
        }

        private string GenerateCartItemId() => (nextCartItemId++).ToString();

        public bool HasProduct(string productId) => items.Any(i => i.ProductId == productId);
        public CartItem? GetCartItem(string cartItemId) => items.FirstOrDefault(i => i.CartItemId == cartItemId);
        public CartItem? GetCartItemByProductId(string productId) => items.FirstOrDefault(i => i.ProductId == productId);

        public void AddItem(Product product, int quantity = 1)
        {
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

                // Инициализируем историю для новой позиции
                priceHistory[newItem.CartItemId] = new Stack<PriceSnapshot>();

                Console.WriteLine($"Добавлен: {product.Name} x{quantity}");
            }
        }

        public void RemoveItem(string cartItemId)
        {
            var item = items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item != null)
            {
                items.Remove(item);
                // НОВОЕ: Очищаем историю при удалении
                priceHistory.Remove(cartItemId);
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

        // Сохраняет состояние в историю перед применением скидки
        public void ApplyDiscount(string cartItemId, decimal percent)
        {
            var item = items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (item != null)
            {
                // Сохраняем текущее состояние в историю ПЕРЕД изменением
                if (!priceHistory.ContainsKey(cartItemId))
                    priceHistory[cartItemId] = new Stack<PriceSnapshot>();

                priceHistory[cartItemId].Push(new PriceSnapshot
                {
                    CurrentPrice = item.CurrentPrice,
                    DiscountPercent = item.DiscountPercent
                });

                // Применяем новую скидку
                item.CurrentPrice = item.CurrentPrice * (1 - percent / 100);
                decimal totalDiscountPercent = (1 - item.CurrentPrice / item.OriginalPrice) * 100;
                item.DiscountPercent = Math.Round(totalDiscountPercent, 2);

                Console.WriteLine($"Скидка {percent}% на {item.Name} → цена: {item.CurrentPrice:C} (общая скидка {item.DiscountPercent}%)");
            }
        }

        // Восстанавливает предыдущую цену из истории
        public void UndoLastDiscount(string cartItemId)
        {
            if (priceHistory.TryGetValue(cartItemId, out var history) && history.Count > 0)
            {
                var previous = history.Pop();
                var item = GetCartItem(cartItemId);
                if (item != null)
                {
                    item.CurrentPrice = previous.CurrentPrice;
                    item.DiscountPercent = previous.DiscountPercent;
                    Console.WriteLine($"Восстановлена цена {item.Name}: {item.CurrentPrice:C} (скидка {item.DiscountPercent}%)");
                }
            }
            else
            {
                Console.WriteLine($"Нет истории скидок для позиции {cartItemId}");
            }
        }

        // Проверяет, можно ли отменить скидку
        public bool CanUndoDiscount(string cartItemId)
        {
            return priceHistory.TryGetValue(cartItemId, out var history) && history.Count > 0;
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