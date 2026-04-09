using System;
using System.Collections.Generic;

namespace CompositeOrderSystem
{
    // Композитный элемент (Composite)
    class OrderGroup : OrderComponent
    {
        private List<OrderComponent> _components = new List<OrderComponent>(); // Содержит другие компоненты
        private string _groupCategory;

        public OrderGroup(string groupName, string groupCategory = "Группа")
            : base(groupName)
        {
            _groupCategory = groupCategory;
        }

        // Добавляет компонент (товар или группу) в состав группы
        public override void Add(OrderComponent component)
        {
            _components.Add(component);
        }

        // Удаляет компонент из группы
        public override void Remove(OrderComponent component)
        {
            _components.Remove(component);
        }

        // Получает количество вложенных компонентов
        public int GetComponentCount()
        {
            return _components.Count;
        }

        // Суммирует стоимость всех вложенных компонентов
        public override decimal GetTotalPrice()
        {
            decimal total = 0;
            foreach (var component in _components)
            {
                total += component.GetTotalPrice();
            }
            return total;
        }

        // Считает общее количество товаров во вложенных компонентах
        public override int GetTotalItems()
        {
            int total = 0;
            foreach (var component in _components)
            {
                total += component.GetTotalItems();
            }
            return total;
        }

        // Вычисляет среднюю цену по группе компонентов
        public override decimal GetAveragePrice()
        {
            if (_components.Count == 0) return 0;
            decimal total = 0;
            foreach (var component in _components)
            {
                total += component.GetAveragePrice();
            }
            return total / _components.Count;
        }

        // Собирает имена всех товаров рекурсивно
        public override List<string> GetAllNames()
        {
            var allNames = new List<string>();
            foreach (var component in _components)
            {
                allNames.AddRange(component.GetAllNames());
            }
            return allNames;
        }

        // Собирает статистику по категориям товаров
        public Dictionary<string, decimal> GetCategoryStats()
        {
            var stats = new Dictionary<string, decimal>();
            foreach (var component in _components)
            {
                if (component is OrderItem item)
                {
                    if (stats.ContainsKey(item.Category))
                        stats[item.Category] += item.GetTotalPrice();
                    else
                        stats[item.Category] = item.GetTotalPrice();
                }
                else if (component is OrderGroup group)
                {
                    var groupStats = group.GetCategoryStats();
                    foreach (var kvp in groupStats)
                    {
                        if (stats.ContainsKey(kvp.Key))
                            stats[kvp.Key] += kvp.Value;
                        else
                            stats[kvp.Key] = kvp.Value;
                    }
                }
            }
            return stats;
        }

        // Печатает информацию о группе и её вложенных компонентах
        public override void PrintOrder(int depth = 0)
        {
            Console.WriteLine($"{new string(' ', depth * 2)} {Name} [{_groupCategory}] | Товаров: {GetTotalItems()} | Сумма: {GetTotalPrice():C} | Сред.цена: {GetAveragePrice():C}");
            foreach (var component in _components)
            {
                component.PrintOrder(depth + 1);
            }
        }
    }
}
