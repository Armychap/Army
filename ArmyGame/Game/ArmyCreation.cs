using System;
using ArmyBattle.Models;
using ArmyBattle.Services;
using ArmyBattle.UI;

namespace ArmyBattle
{
    /// <summary>
    /// Класс для создания и настройки армий
    /// </summary>
    static class ArmyCreation
    {
        /// <summary>
        /// Спрашивает у пользователя способ создания армии: автоматический или ручной.
        /// Возвращает true для автоматического создания, false для ручного.
        /// </summary>
        public static bool AskCreationMethod()
        {
            while (true)
            {
                Console.WriteLine("\nВыберите способ создания армии:");
                Console.WriteLine("1. Создать автоматически");
                Console.WriteLine("2. Создать вручную");
                Console.Write("Выбор (1 или 2): ");

                string? choice = Console.ReadLine();

                if (choice == "1")
                {
                    return true;
                }
                else if (choice == "2")
                {
                    return false;
                }
                else
                {
                    Console.WriteLine("Неверный выбор! Введите 1 или 2.");
                }
            }
        }

        /// <summary>
        /// Создает две армии с автоматической генерацией и немедленно начинает битву.
        /// Главные армии генерируются случайно в зависимости от установленного бюджета.
        /// </summary>
        public static (IArmy army1, IArmy army2) CreateRandomBattle()
        {
            // Очищаем экран перед началом
            ConsoleMenu.ClearConsole();

            // Получаем названия обеих армий от пользователя
            var (name1, name2) = GetArmyNames();
            
            // Получаем общий бюджет для обеих армий
            int budget = GetCommonBudget(200);

            // Создаем первую армию с красным цветом
            var army1 = new Army(name1, ConsoleColor.Red);
            
            // Создаем вторую армию с синим цветом
            var army2 = new Army(name2, ConsoleColor.Blue);

            // Генерируем случайные юниты для первой армии в рамках бюджета
            army1.GenerateArmyWithBudget(budget);
            
            // Генерируем случайные юниты для второй армии в рамках бюджета
            army2.GenerateArmyWithBudget(budget);

            return (army1, army2);
        }

        /// <summary>
        /// Позволяет пользователю вручную создать две армии через интерактивное меню.
        /// Для каждой армии пользователь выбирает тип и количество юнитов.
        /// </summary>
        public static (IArmy army1, IArmy army2) CreateManualArmies()
        {
            // Очищаем экран перед началом
            ConsoleMenu.ClearConsole();

            // Получаем названия обеих армий от пользователя
            var (name1, name2) = GetArmyNames();
            
            // Получаем общий бюджет для обеих армий
            int budget = GetCommonBudget(200);

            // Создаем первую армию с красным цветом
            var army1 = new Army(name1, ConsoleColor.Red);
            
            // Создаем вторую армию с синим цветом
            var army2 = new Army(name2, ConsoleColor.Blue);

            // Выводим меню настройки для первой армии
            ConsoleMenu.PrintHeader("НАСТРОЙКА ПЕРВОЙ АРМИИ");
            
            // Запускаем интерактивное меню для добавления юнитов в первую армию
            SetupArmyManually(army1, budget);

            // Выводим меню настройки для второй армии
            ConsoleMenu.PrintHeader("НАСТРОЙКА ВТОРОЙ АРМИИ");
            
            // Запускаем интерактивное меню для добавления юнитов во вторую армию
            SetupArmyManually(army2, budget);

            // Очищаем экран перед окончательным отображением
            ConsoleMenu.ClearConsole();
            
            // Выводим финальный состав первой армии
            ConsoleMenu.PrintHeader("ИТОГОВЫЙ СОСТАВ АРМИЙ");
            army1.DisplayArmyInfo(true);
            Console.WriteLine();
            
            // Выводим финальный состав второй армии
            army2.DisplayArmyInfo(true);

            Console.WriteLine("\nНажмите Enter для начала битвы");
            
            // Читаем нажатую клавишу
            var key = Console.ReadKey();

            // Если нажата клавиша Enter - начинаем битву
            if (key.Key == ConsoleKey.Enter)
            {
                // Запускаем битву между вручную созданными армиями
                _ = BattleMenu.StartBattle(army1, army2);
            }

            return (army1, army2);
        }

        /// <summary>
        /// Интерактивное меню для ручного добавления и удаления юнитов в армию.
        /// </summary>
        public static void SetupArmyManually(IArmy army, int maxBudget)
        {
            // Счетчик для нумерации юнитов
            int fighterNumber = 1;
            
            // Текущее потраченное бюджета на юнитов
            int totalCost = 0;

            while (true)
            {
                // Очищаем экран перед выводом меню настройки
                ConsoleMenu.ClearConsole();
                
                Console.WriteLine($"Текущий состав {army.Name}:");
                Console.WriteLine($"Всего бойцов: {army.Units.Count}");
                Console.WriteLine($"Потрачено: {totalCost}/{maxBudget}");
                Console.WriteLine();

                // Если в армии уже есть юниты - выводим их список
                if (army.Units.Count > 0)
                {
                    // Итерируемся по каждому юниту и выводим его информацию
                    foreach (var unit in army.Units)
                    {
                        Console.WriteLine($"  {unit.GetDisplayName("")} - {unit.Name} (Стоимость: {unit.Cost})");
                    }
                }

                // Выводим варианты действий
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1 - Добавить слабого бойца (ATK 10, DEF 8, HP 25) - 15");
                Console.WriteLine("2 - Добавить лучника (ATK 5, DEF 3, HP 18) - 25");
                Console.WriteLine("3 - Добавить мага (клонирует союзников) - 30");
                Console.WriteLine("4 - Добавить сильного бойца (ATK 20, DEF 15, HP 60) - 40");
                Console.WriteLine("5 - Добавить Гуляй город (ATK 0, DEF 50, HP 70) - 55");
                Console.WriteLine("6 - Удалить последнего бойца");
                Console.WriteLine("7 - Завершить настройку");
                Console.Write("Выбор: ");

                // Читаем выбор пользователя
                string? choice = Console.ReadLine();

                // Обрабатываем выбор
                switch (choice)
                {
                    //  Добавить слабого бойца
                    case "1":
                        // Проверяем хватит ли бюджета для добавления слабого бойца
                        if (totalCost + 15 <= maxBudget)
                        {
                            // Создаем нового слабого бойца с текущим номером
                            IUnit fighter = new WeakFighter(fighterNumber++);
                            
                            // Добавляем бойца в армию
                            army.AddUnit(fighter);
                            
                            // Увеличиваем потраченный бюджет
                            totalCost += 15;
                            
                            // Выводим сообщение об успехе
                            Console.WriteLine("Слабый боец добавлен!");
                        }
                        else
                            // Если бюджета недостаточно - выводим ошибку
                            Console.WriteLine("Недостаточно бюджета!");
                        break;

                    // Добавить лучника
                    case "2":
                        // Проверяем хватит ли бюджета для добавления лучника
                        if (totalCost + 25 <= maxBudget)
                        {
                            // Создаем нового лучника с текущим номером
                            IUnit fighter = new Archer(fighterNumber++);
                            
                            // Добавляем лучника в армию
                            army.AddUnit(fighter);
                            
                            // Увеличиваем потраченный бюджет
                            totalCost += 25;
                            
                            // Выводим сообщение об успехе
                            Console.WriteLine("Лучник добавлен!");
                        }
                        else
                            // Если бюджета недостаточно - выводим ошибку
                            Console.WriteLine("Недостаточно бюджета!");
                        break;

                // Добавить мага
                case "3":
                    // Проверяем хватит ли бюджета для добавления мага
                    if (totalCost + 30 <= maxBudget)
                    {
                        IUnit fighter = new Wizard(fighterNumber++);
                        army.AddUnit(fighter);
                        totalCost += 30;
                        Console.WriteLine("Маг добавлен!");
                    }
                    else
                        Console.WriteLine("Недостаточно бюджета!");
                    break;

                case "4":
                    if (totalCost + 40 <= maxBudget)
                    {
                        IUnit fighter = new StrongFighter(fighterNumber++);
                        army.AddUnit(fighter);
                        totalCost += 40;
                        Console.WriteLine("Сильный боец добавлен!");
                    }
                    else
                        Console.WriteLine("Недостаточно бюджета!");
                    break;

                case "5":
                    // Добавить Гуляй город (стена щитов)
                    if (totalCost + 55 <= maxBudget)
                    {
                        IUnit fighter = new ShieldWall(fighterNumber++);
                        army.AddUnit(fighter);
                        totalCost += 55;
                        Console.WriteLine("Гуляй город добавлен!");
                    }
                    else
                        Console.WriteLine("Недостаточно бюджета!");
                    break;

                case "6":
                    if (army.Units.Count > 0)
                    {
                        var removed = army.Units[army.Units.Count - 1];
                        totalCost -= removed.Cost;
                        army.Units.RemoveAt(army.Units.Count - 1);
                        fighterNumber--;
                        Console.WriteLine($"Боец удален! Возвращено {removed.Cost} монет.");
                    }
                    else
                        Console.WriteLine("Нет бойцов для удаления!");
                    break;

                case "7":
                    Console.WriteLine("Настройка завершена!");
                    Console.WriteLine("Нажмите любую клавишу, чтобы продолжить...");
                    Console.ReadKey(true);
                    return;
            }
        }
    }

    /// <summary>
    /// Получает названия двух армий от пользователя.
    /// </summary>
    public static (string name1, string name2) GetArmyNames()
    {
        string name1 = ConsoleMenu.GetInput("Введите название первой армии: ");
        if (string.IsNullOrWhiteSpace(name1))
        {
            name1 = "Красная Армия";
            Console.WriteLine($"Использовано название по умолчанию: {name1}");
        }

        string name2 = ConsoleMenu.GetInput("Введите название второй армии: ");
        if (string.IsNullOrWhiteSpace(name2))
        {
            name2 = "Синяя Армия";
            Console.WriteLine($"Использовано название по умолчанию: {name2}");
        }

        return (name1, name2);
    }

    // Получает единый бюджет для обеих армий от пользователя
        public static int GetCommonBudget(int defaultBudget)
        {
            int budget = ConsoleMenu.GetIntInput($"\nВведите бюджет для обеих армий: ");
            if (budget <= 0)
                budget = defaultBudget;
            return budget;
        }
    }
}