using System;
using System.Collections.Generic;
using System.IO;
using ArmyBattle.Models;
using ArmyBattle.Game;
using ArmyBattle.Services;
using ArmyBattle.UI;

namespace ArmyBattle
{
    /// <summary>
    /// Главный класс приложения - точка входа в программу.
    /// Основна обязанность:
    /// - Инициализация приложения
    /// - Показ главного меню
    /// - Навигация между различными функциями
    /// - Сохранение состояния текущих армий для переиспользования
    /// </summary>
    class Program
    {
        // ПОЛЯ КЛАССА
        // Сервис для управления сохранением и загрузкой армий в JSON
        private static ArmyManager armyManager;
        
        // Сервис для проведения битв и сохранения их логов
        private static BattleManager battleManager;

        // Первая армия, загруженная или созданная в текущей сессии
        // Используется для повторного проведения боя без пересоздания
        private static IArmy _lastArmy1 = null;
        
        // Вторая армия, загруженная или созданная в текущей сессии
        // Используется для повторного проведения боя без пересоздания
        private static IArmy _lastArmy2 = null;

        static void Main(string[] args)
        {
            // Устанавливаем кодировку консоли на UTF-8 для правильного отображения русских символов
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Устанавливаем название окна консоли
            Console.Title = "Битва Армий";

            // ИНИЦИАЛИЗАЦИЯ СЕРВИСОВ
            
            // Создаем сервис для управления армиями (сохранение/загрузка)
            armyManager = new ArmyManager();
            
            // Создаем сервис для проведения битв и логирования
            battleManager = new BattleManager();

            // Флаг для выхода из главного цикла программы
            bool exit = false;

            while (!exit)
            {
                // Очищаем консоль и выводим главное меню
                ConsoleMenu.ClearConsole();
                Console.WriteLine("          ГЛАВНОЕ МЕНЮ");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("1 - Автоматическая генерация армий");
                Console.WriteLine("2 - Ручное создание армий");
                Console.WriteLine("3 - Загрузить армии с диска");
                Console.WriteLine("4 - Сохранить армии на диск");
                Console.WriteLine("5 - Информация о составе армий");
                Console.WriteLine("6 - Просмотреть историю битв");
                Console.WriteLine("0 - Выход");
                Console.WriteLine("-------------------------------------");
                Console.Write("Выбор: ");

                // Читаем выбор пользователя
                string choice = Console.ReadLine();

                // Обрабатываем выбор пользователя
                switch (choice)
                {
                    // Автоматическая генерация двух армий и битва
                    case "1":
                        CreateRandomBattle();
                        break;

                    // Ручное создание армий через интерактивное меню
                    case "2":
                        CreateManualArmies();
                        break;

                    // Загрузка ранее сохраненных армий
                    case "3":
                        LoadArmiesFromDisk();
                        break;

                    // Сохранение текущих армий в JSON файл
                    case "4":
                        SaveCurrentArmies();
                        break;

                    // Просмотр состава армий
                    case "5":
                        ShowStoredArmiesInfo();
                        break;

                    // Просмотр истории
                    case "6":
                        ShowBattleLogs();
                        break;

                    // Выход из программы
                    case "0":
                        exit = true;
                        break;

                    // Некорректный выбор
                    default:
                        Console.WriteLine("Неверный выбор!");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Создает две армии с автоматической генерацией и немедленно начинает битву.
        /// Главные армии генерируются случайно в зависимости от установленного бюджета.
        /// </summary>
        static void CreateRandomBattle()
        {
            // Очищаем экран перед началом
            ConsoleMenu.ClearConsole();

            // Получаем названия обеих армий от пользователя
            var (name1, name2) = GetArmyNames();
            
            // Получаем общий бюджет для обеих армий
            int budget = GetCommonBudget(200);

            // Создаем первую армию с красным цветом
            _lastArmy1 = new Army(name1, ConsoleColor.Red);
            
            // Создаем вторую армию с синим цветом
            _lastArmy2 = new Army(name2, ConsoleColor.Blue);

            // Генерируем случайные юниты для первой армии в рамках бюджета
            _lastArmy1.GenerateArmyWithBudget(budget);
            
            // Генерируем случайные юниты для второй армии в рамках бюджета
            _lastArmy2.GenerateArmyWithBudget(budget);

            // Запускаем битву между сгенерированными армиями
            StartBattle(_lastArmy1, _lastArmy2);
        }

        /// <summary>
        /// Позволяет пользователю вручную создать две армии через интерактивное меню.
        /// Для каждой армии пользователь выбирает тип и количество юнитов.
        /// </summary>
        static void CreateManualArmies()
        {
            // Очищаем экран перед началом
            ConsoleMenu.ClearConsole();

            // Получаем названия обеих армий от пользователя
            var (name1, name2) = GetArmyNames();
            
            // Получаем общий бюджет для обеих армий
            int budget = GetCommonBudget(200);

            // Создаем первую армию с красным цветом
            _lastArmy1 = new Army(name1, ConsoleColor.Red);
            
            // Создаем вторую армию с синим цветом
            _lastArmy2 = new Army(name2, ConsoleColor.Blue);

            // Выводим меню настройки для первой армии
            ConsoleMenu.PrintHeader("НАСТРОЙКА ПЕРВОЙ АРМИИ");
            
            // Запускаем интерактивное меню для добавления юнитов в первую армию
            SetupArmyManually(_lastArmy1, budget);

            // Выводим меню настройки для второй армии
            ConsoleMenu.PrintHeader("НАСТРОЙКА ВТОРОЙ АРМИИ");
            
            // Запускаем интерактивное меню для добавления юнитов во вторую армию
            SetupArmyManually(_lastArmy2, budget);

            // Очищаем экран перед окончательным отображением
            ConsoleMenu.ClearConsole();
            
            // Выводим финальный состав первой армии
            ConsoleMenu.PrintHeader("ИТОГОВЫЙ СОСТАВ АРМИЙ");
            _lastArmy1.DisplayArmyInfo(true);
            Console.WriteLine();
            
            // Выводим финальный состав второй армии
            _lastArmy2.DisplayArmyInfo(true);

            Console.WriteLine("\nНажмите Enter для начала битвы");
            
            // Читаем нажатую клавишу
            var key = Console.ReadKey();

            // Если нажата клавиша Enter - начинаем битву
            if (key.Key == ConsoleKey.Enter)
            {
                // Запускаем битву между вручную созданными армиями
                StartBattle(_lastArmy1, _lastArmy2);
            }
        }

        /// <summary>
        /// Интерактивное меню для ручного добавления и удаления юнитов в армию.
        /// </summary>
        static void SetupArmyManually(IArmy army, int maxBudget)
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
                Console.WriteLine("-------------------------------------");

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
                Console.WriteLine("3 - Добавить сильного бойца (ATK 20, DEF 15, HP 60) - 40");
                Console.WriteLine("4 - Удалить последнего бойца");
                Console.WriteLine("5 - Завершить настройку");
                Console.Write("Выбор: ");

                // Читаем выбор пользователя
                string choice = Console.ReadLine();

                // Обрабатываем выбор
                switch (choice)
                {
                    //  Добавить слабого бойца
                    case "1":
                        // Проверяем хватит ли бюджета для добавления слабого бойца
                        if (totalCost + 15 <= maxBudget)
                        {
                            // Создаем нового слабого бойца с текущим номером
                            var fighter = new WeakFighter(fighterNumber++);
                            
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
                            var fighter = new Archer(fighterNumber++);
                            
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

                    case "3":
                        if (totalCost + 40 <= maxBudget)
                        {
                            var fighter = new StrongFighter(fighterNumber++);
                            army.AddUnit(fighter);
                            totalCost += 40;
                            Console.WriteLine("Сильный боец добавлен!");
                        }
                        else
                            Console.WriteLine("Недостаточно бюджета!");
                        break;

                    case "4":
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

                    case "5":
                        return;

                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        // Сохраняет текущие загруженные армии на диск
        static void SaveCurrentArmies()
        {
            ConsoleMenu.ClearConsole();

            if (_lastArmy1 == null || _lastArmy2 == null)
            {
                ConsoleMenu.ShowError("Сначала создайте или загрузите армии!");
                Console.ReadKey();
                return;
            }

            ConsoleMenu.PrintHeader("СОХРАНЕНИЕ АРМИЙ");
            string saveName = ConsoleMenu.GetInput("Введите название для сохранения (без пробелов): ");
            
            armyManager.SaveArmies(_lastArmy1, _lastArmy2, saveName);
            ConsoleMenu.ShowSuccess($"Армии сохранены!");
            Console.ReadKey();
        }

        // Загружает армии из сохраненного JSON файла
        static void LoadArmiesFromDisk()
        {
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("ЗАГРУЗКА АРМИЙ");

            string[] savedFiles = armyManager.GetSavedArmies();

            if (savedFiles.Length == 0)
            {
                ConsoleMenu.ShowMessage("Нет сохраненных армий!");
                Console.ReadKey();
                return;
            }

            int choice = ConsoleMenu.ShowFileMenu(savedFiles, "ЗАГРУЗКА АРМИЙ");

            if (choice >= 1 && choice <= savedFiles.Length)
            {
                string filePath = armyManager.GetSavePath(savedFiles[choice - 1]);
                
                if (armyManager.LoadArmies(filePath, out IArmy army1, out IArmy army2))
                {
                    _lastArmy1 = army1;
                    _lastArmy2 = army2;

                    ConsoleMenu.ClearConsole();
                    ConsoleMenu.ShowSuccess("Армии успешно загружены!");
                    Console.WriteLine($"Первая армия: {_lastArmy1.Name} - {_lastArmy1.Units.Count} бойцов");
                    Console.WriteLine($"Вторая армия: {_lastArmy2.Name} - {_lastArmy2.Units.Count} бойцов");

                    Console.WriteLine("\nНажмите Enter для начала битвы или другую клавишу для возврата...");
                    var key = Console.ReadKey();

                    if (key.Key == ConsoleKey.Enter)
                    {
                        StartBattle(_lastArmy1, _lastArmy2);
                    }
                }
            }
        }

        // Запускает боевой симулятор между двумя армиями
        static void StartBattle(IArmy army1, IArmy army2)
        {
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("НАЧАЛО БИТВЫ");

            army1.DisplayArmyInfo(false);
            Console.WriteLine();
            army2.DisplayArmyInfo(false);

            ConsoleMenu.WaitForKey("\nНажмите Enter для начала битвы...");

            battleManager.StartBattle(army1, army2);

            Console.WriteLine("\nСохранить историю битвы? (д/н): ");
            if (Console.ReadLine()?.ToLower() == "д")
            {
                battleManager.SaveBattleLog("", $"{army1.Name}_vs_{army2.Name}", army1, army2);
                ConsoleMenu.ShowSuccess("История сохранена!");
            }

            ConsoleMenu.WaitForKey("\nНажмите любую клавишу для возврата в меню...");
        }

        // Показывает интерактивное меню для просмотра историй битв
        static void ShowBattleLogs()
        {
            string[] battles = battleManager.GetSavedBattles();

            if (battles.Length == 0)
            {
                ConsoleMenu.ClearConsole();
                ConsoleMenu.ShowMessage("Нет сохраненных историй битв!");
                Console.ReadKey();
                return;
            }

            bool exitMenu = false;
            while (!exitMenu)
            {
                int choice = ConsoleMenu.ShowFileMenu(battles, "ИСТОРИИ БИТВ");

                if (choice >= 1 && choice <= battles.Length)
                {
                    string filePath = battleManager.GetLogPath(battles[choice - 1]);
                    string content = File.ReadAllText(filePath);

                    ConsoleMenu.ClearConsole();
                    Console.WriteLine($"История битвы: {battles[choice - 1]}");
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine(content);
                    ConsoleMenu.WaitForKey("\nНажмите любую клавишу для возврата к списку...");
                }
                else if (choice == 0)
                {
                    exitMenu = true;
                }
            }
        }

        // Показывает интерактивное меню для просмотра состава армий из сохраненных битв
        static void ShowStoredArmiesInfo()
        {
            string[] savedBattles = battleManager.GetSavedBattleArmies();

            if (savedBattles.Length == 0)
            {
                ConsoleMenu.ClearConsole();
                ConsoleMenu.ShowMessage("Нет сохраненных битв со составом армий!\nСначала проведите битву и сохраните её.");
                Console.ReadKey();
                return;
            }

            bool exitMenu = false;
            while (!exitMenu)
            {
                int choice = ConsoleMenu.ShowFileMenu(savedBattles, "ИНФОРМАЦИЯ О СОСТАВЕ АРМИЙ");

                if (choice >= 1 && choice <= savedBattles.Length)
                {
                    var armyData = battleManager.LoadBattleArmies(savedBattles[choice - 1]);

                    if (armyData != null)
                    {
                        ConsoleMenu.ClearConsole();
                        ConsoleMenu.PrintHeader($"БИТВА: {savedBattles[choice - 1]}");

                        ConsoleMenu.DisplayArmyComposition(
                            armyData.Army1Name, armyData.Army1Color,
                            armyData.Army1Units, armyData.TotalCost1);

                        ConsoleMenu.DisplayArmyComposition(
                            armyData.Army2Name, armyData.Army2Color,
                            armyData.Army2Units, armyData.TotalCost2);

                        ConsoleMenu.WaitForKey("\nНажмите любую клавишу для возврата к списку...");
                    }
                }
                else if (choice == 0)
                {
                    exitMenu = true;
                }
            }
        }

        // Запрашивает названия обеих армий у пользователя
        static (string name1, string name2) GetArmyNames()
        {
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("НАЗВАНИЯ АРМИЙ");

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
        static int GetCommonBudget(int defaultBudget = 200)
        {
            int budget = ConsoleMenu.GetIntInput($"\nВведите бюджет для обеих армий (стандарт {defaultBudget}): ");
            if (budget <= 0)
                budget = defaultBudget;
            return budget;
        }
    }
}