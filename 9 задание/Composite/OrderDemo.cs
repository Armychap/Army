using System;
using System.Collections.Generic;

namespace CompositeOrderSystem
{
    class OrderDemo
    {
        public void Run()
        {
            var (mainOrder, electronics, groceries, books, accessories, monitor) = BuildOrder();

            PrintFullOrder(mainOrder);
            PrintOverallInfo(mainOrder);
            PrintCategoryStats(mainOrder);
            PrintAllNames(mainOrder);
            PrintGroupInfo(electronics, groceries, books, accessories);
            DemonstrateDynamicChange(electronics, monitor);
            WaitForExit();
        }

        private (OrderGroup MainOrder, OrderGroup Electronics, OrderGroup Groceries, OrderGroup Books, OrderGroup Accessories, OrderItem Monitor) BuildOrder()
        {
            OrderItem laptop = new OrderItem("Ноутбук Dell", 65000, 1, "Электроника");
            OrderItem mouse = new OrderItem("Мышь Logitech", 1500, 2, "Электроника");
            OrderItem keyboard = new OrderItem("Клавиатура", 3500, 1, "Электроника");
            OrderItem monitor = new OrderItem("Монитор 27\"", 22000, 1, "Электроника");

            OrderItem bread = new OrderItem("Хлеб", 60, 3, "Продукты");
            OrderItem milk = new OrderItem("Молоко", 90, 2, "Продукты");
            OrderItem cheese = new OrderItem("Сыр", 350, 1, "Продукты");

            OrderItem book1 = new OrderItem("C# Programming", 1200, 1, "Книги");
            OrderItem book2 = new OrderItem("Design Patterns", 1500, 2, "Книги");

            OrderGroup electronics = new OrderGroup("Электроника", "Техника");
            electronics.Add(laptop);
            electronics.Add(mouse);
            electronics.Add(keyboard);
            electronics.Add(monitor);

            OrderGroup accessories = new OrderGroup("Аксессуары", "Дополнительно");
            accessories.Add(new OrderItem("Коврик для мыши", 500, 1, "Аксессуары"));
            accessories.Add(new OrderItem("USB-Hub", 800, 2, "Аксессуары"));
            electronics.Add(accessories);

            OrderGroup groceries = new OrderGroup("Продукты", "Питание");
            groceries.Add(bread);
            groceries.Add(milk);
            groceries.Add(cheese);

            OrderGroup books = new OrderGroup("Книги", "Обучение");
            books.Add(book1);
            books.Add(book2);

            OrderGroup mainOrder = new OrderGroup("Главный заказ", "Общий");
            mainOrder.Add(electronics);
            mainOrder.Add(groceries);
            mainOrder.Add(books);
            mainOrder.Add(new OrderItem("Батарейки AA", 200, 4, "Прочее"));

            return (mainOrder, electronics, groceries, books, accessories, monitor);
        }

        private void PrintFullOrder(OrderGroup mainOrder)
        {
            Console.WriteLine("ПОЛНАЯ СТРУКТУРА ЗАКАЗА\n");
            mainOrder.PrintOrder();
        }

        private void PrintOverallInfo(OrderGroup mainOrder)
        {
            Console.WriteLine("\nОБЩАЯ ИНФОРМАЦИЯ");
            Console.WriteLine($"Общая стоимость: {mainOrder.GetTotalPrice():C}");
            Console.WriteLine($"Общее количество товаров: {mainOrder.GetTotalItems()} шт");
            Console.WriteLine($"Средняя цена товара: {mainOrder.GetAveragePrice():C}");
        }

        private void PrintCategoryStats(OrderGroup mainOrder)
        {
            Console.WriteLine("\nСТАТИСТИКА ПО КАТЕГОРИЯМ");
            var categoryStats = mainOrder.GetCategoryStats();
            foreach (var category in categoryStats)
            {
                Console.WriteLine($"{category.Key}: {category.Value:C}");
            }
        }

        private void PrintAllNames(OrderGroup mainOrder)
        {
            Console.WriteLine("\nВСЕ ТОВАРЫ В ЗАКАЗЕ");
            var allNames = mainOrder.GetAllNames();
            for (int i = 0; i < allNames.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {allNames[i]}");
            }
        }

        private void PrintGroupInfo(OrderGroup electronics, OrderGroup groceries, OrderGroup books, OrderGroup accessories)
        {
            Console.WriteLine("\nИНФОРМАЦИЯ ПО ГРУППАМ");
            Console.WriteLine($"В группе 'Электроника': {electronics.GetTotalItems()} товаров на сумму {electronics.GetTotalPrice():C}");
            Console.WriteLine($"В группе 'Продукты': {groceries.GetTotalItems()} товаров на сумму {groceries.GetTotalPrice():C}");
            Console.WriteLine($"В группе 'Книги': {books.GetTotalItems()} товаров на сумму {books.GetTotalPrice():C}");
            Console.WriteLine($"В группе 'Аксессуары': {accessories.GetTotalItems()} товаров на сумму {accessories.GetTotalPrice():C}");
        }

        private void DemonstrateDynamicChange(OrderGroup electronics, OrderItem monitor)
        {
            Console.WriteLine("\nДИНАМИЧЕСКОЕ ИЗМЕНЕНИЕ");
            Console.WriteLine($"До удаления: {electronics.GetComponentCount()} компонентов в Электронике");
            electronics.Remove(monitor);
            Console.WriteLine($"После удаления монитора: {electronics.GetComponentCount()} компонентов");
            Console.WriteLine($"Новая стоимость электроники: {electronics.GetTotalPrice():C}");
        }

        private void WaitForExit()
        {
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
