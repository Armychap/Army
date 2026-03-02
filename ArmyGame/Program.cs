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
        private static ArmyManager armyManager = null!;
        
        // Сервис для проведения битв и сохранения их логов
        private static BattleManager battleManager = null!;

        // Первая армия, загруженная или созданная в текущей сессии
        // Используется для повторного проведения боя без пересоздания
        private static IArmy? _lastArmy1 = null;
        
        // Вторая армия, загруженная или созданная в текущей сессии
        // Используется для повторного проведения боя без пересоздания
        private static IArmy? _lastArmy2 = null;

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
                Console.WriteLine();
                Console.WriteLine("1. Новая игра");
                Console.WriteLine("2. Загрузить игру (продолжить)");
                Console.WriteLine("3. История");
                Console.WriteLine("4. Выйти");
                Console.Write("\nВыбор: ");

                // Читаем выбор пользователя
                string? choice = Console.ReadLine();

                // Обрабатываем выбор пользователя
                switch (choice)
                {
                    // Новая игра с выбором способа создания армий
                    case "1":
                        StartNewGame();
                        break;

                    // Загрузка ранее сохраненных армий
                    case "2":
                        LoadArmiesFromDisk();
                        break;

                    // Просмотр истории
                    case "3":
                        ShowBattleLogs();
                        break;

                    // Выход из программы
                    case "4":
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
        /// Управляет процессом начала новой игры.
        /// Запрашивает названия армий, бюджет и способ создания для каждой армии.
        /// </summary>
        static void StartNewGame()
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

            // Спрашиваем способ создания первой армии
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader($"СОЗДАНИЕ АРМИИ: {name1}");

            bool useAutoForArmy1 = AskCreationMethod();

            if (useAutoForArmy1)
            {
                // Автоматическое создание первой армии
                _lastArmy1.GenerateArmyWithBudget(budget);
                
                // Очищаем экран и показываем информацию об созданной армии
                ConsoleMenu.ClearConsole();
                ConsoleMenu.PrintHeader($"СОЗДАНА АРМИЯ: {name1}");
                _lastArmy1.DisplayArmyInfo(true);
                ConsoleMenu.WaitForKey("\nНажмите любую клавишу для продолжения...");
            }
            else
            {
                // Ручное создание первой армии
                ConsoleMenu.PrintHeader("НАСТРОЙКА ПЕРВОЙ АРМИИ");
                SetupArmyManually(_lastArmy1, budget);
            }

            // Спрашиваем способ создания второй армии
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader($"СОЗДАНИЕ АРМИИ: {name2}");

            bool useAutoForArmy2 = AskCreationMethod();

            if (useAutoForArmy2)
            {
                // Автоматическое создание второй армии
                _lastArmy2.GenerateArmyWithBudget(budget);
                
                // Очищаем экран и показываем информацию об созданной армии
                ConsoleMenu.ClearConsole();
                ConsoleMenu.PrintHeader($"СОЗДАНА АРМИЯ: {name2}");
                _lastArmy2.DisplayArmyInfo(true);
                ConsoleMenu.WaitForKey("\nНажмите любую клавишу для продолжения...");
            }
            else
            {
                // Ручное создание второй армии
                ConsoleMenu.PrintHeader("НАСТРОЙКА ВТОРОЙ АРМИИ");
                SetupArmyManually(_lastArmy2, budget);
            }

            // Очищаем экран перед окончательным отображением
            ConsoleMenu.ClearConsole();
            
            // Выводим финальный состав обеих армий
            ConsoleMenu.PrintHeader("ИТОГОВЫЙ СОСТАВ АРМИЙ");
            _lastArmy1.DisplayArmyInfo(true);
            Console.WriteLine();
            
            _lastArmy2.DisplayArmyInfo(true);

            Console.WriteLine("\nНажмите Enter для начала битвы");
            
            // Читаем нажатую клавишу
            var key = Console.ReadKey();

            // Если нажата клавиша Enter - начинаем битву
            if (key.Key == ConsoleKey.Enter)
            {
                // Запускаем битву между созданными армиями
                StartBattle(_lastArmy1, _lastArmy2);
            }
        }

        /// <summary>
        /// Спрашивает у пользователя способ создания армии: автоматический или ручной.
        /// Возвращает true для автоматического создания, false для ручного.
        /// </summary>
        static bool AskCreationMethod()
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
                Console.WriteLine("3 - Добавить сильного бойца (ATK 20, DEF 15, HP 60) - 40");
                Console.WriteLine("4 - Удалить последнего бойца");
                Console.WriteLine("5 - Завершить настройку");
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
            ConsoleMenu.PrintHeader("ЗАГРУЗКА И ПРОДОЛЖЕНИЕ ИГРЫ");

            string[] savedFiles = armyManager.GetSavedArmies();

            if (savedFiles.Length == 0)
            {
                ConsoleMenu.ShowMessage("Нет сохраненных армий!");
                Console.ReadKey();
                return;
            }

            int choice = ConsoleMenu.ShowFileMenu(savedFiles, "ЗАГРУЗКА И ПРОДОЛЖЕНИЕ ИГРЫ");

            if (choice >= 1 && choice <= savedFiles.Length)
            {
                string filePath = armyManager.GetSavePath(savedFiles[choice - 1]);
                
                if (armyManager.LoadArmies(filePath, out IArmy army1, out IArmy army2))
                {
                    _lastArmy1 = army1;
                    _lastArmy2 = army2;

                    ConsoleMenu.ClearConsole();
                    ConsoleMenu.ShowSuccess("Игра загружена! Восстанавливаю боевое состояние...");
                    Console.WriteLine($"\n{_lastArmy1.Name}: {_lastArmy1.Units.Count} всего бойцов, живых: {_lastArmy1.AliveCount()}");
                    Console.WriteLine($"{_lastArmy2.Name}: {_lastArmy2.Units.Count} всего бойцов, живых: {_lastArmy2.AliveCount()}");

                    Console.WriteLine("\nНажмите Enter для продолжения боя или другую клавишу для возврата...");
                    var key = Console.ReadKey();

                    if (key.Key == ConsoleKey.Enter)
                    {
                        // Продолжаем боевой цикл с загруженным состоянием
                        ContinueBattle(_lastArmy1, _lastArmy2);
                    }
                }
            }
        }

        /// <summary>
        /// Продолжает боевой цикл со стороны загруженной игры.
        /// Восстанавливает боевое состояние на основе текущего состояния юнитов (их здоровья и статуса).
        /// </summary>
        static void ContinueBattle(IArmy army1, IArmy army2)
        {
            // Запускаем боевой цикл - система автоматически восстановит боевое состояние
            StartBattle(army1, army2);
        }

        // Запускает боевой симулятор между двумя армиями с интерактивным меню
        static void StartBattle(IArmy army1, IArmy army2)
        {
            // Создаем боевой движок для пошагового управления битвой
            BattleEngine battle = new BattleEngine(army1, army2, 400);
            
            // Инициализируем битву (готовим армии)
            battle.InitializeBattle();

            // Показываем заголовок битвы
            try { Console.Clear(); } catch { }
            Console.WriteLine("НАЧАЛО БИТВЫ");
            Console.WriteLine($"{army1.Name} против {army2.Name}");
            Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
            Console.WriteLine();

            // Меню управления боем
            bool battleActive = true;
            while (battleActive && (army1.HasAliveUnits() && army2.HasAliveUnits()))
            {
                // Выводим меню управления боем
                Console.WriteLine("\nМеню действий");
                Console.WriteLine("1. Сделать ход");
                Console.WriteLine("2. Автоматически пройти до конца");
                Console.WriteLine("3. Сохранить игру");
                Console.WriteLine("4. Выйти (назад в меню)");
                Console.Write("Выбор: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        // Выполняем один раунд (оба удара + проверка способностей)
                        if (!battle.DoSingleRound())
                        {
                            // Битва закончилась
                            battleActive = false;
                        }
                        break;

                    case "2":
                        // Выполняем все раунды до конца без меню
                        Console.WriteLine("\nАвтоматическое проведение боя...\n");
                        while (battle.DoSingleRound())
                        {
                            System.Threading.Thread.Sleep(400);
                        }
                        battleActive = false;
                        break;

                    case "3":
                        // Сохраняем текущое состояние игры и выходим в главное меню
                        SaveGameDuringBattle(army1, army2);
                        return;

                    case "4":
                        // Выход без завершения битвы
                        Console.WriteLine("\nВы уверены? Битва будет потеряна (д/н): ");
                        if (Console.ReadLine()?.ToLower() == "д")
                        {
                            battleActive = false;
                            ConsoleMenu.WaitForKey("\nНажмите любую клавишу для возврата в меню...");
                            return;
                        }
                        break;

                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }

            // После завершения битвы - показываем меню
            ConsoleMenu.ClearConsole();
            Console.WriteLine("     БИТВА ЗАВЕРШЕНА");
            Console.WriteLine(new string('=', 40));
            
            // Определяем победителя
            if (army1.HasAliveUnits())
            {
                Console.ForegroundColor = army1.Color;
                Console.WriteLine($"ПОБЕДИТЕЛЬ: {army1.Name}!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = army2.Color;
                Console.WriteLine($"ПОБЕДИТЕЛЬ: {army2.Name}!");
                Console.ResetColor();
            }

            // Автоматически сохраняем историю битвы
            Console.WriteLine("\nСохраняю историю битвы...");
            battleManager.SaveBattleLog("", $"{army1.Name}_vs_{army2.Name}", army1, army2);
            ConsoleMenu.ShowSuccess("История битвы сохранена!");

            // Меню после завершения битвы
            bool exitBattle = false;
            while (!exitBattle)
            {
                Console.WriteLine("\nМеню действий");
                Console.WriteLine("1. Сохранить состояние игры");
                Console.WriteLine("2. Выйти (назад в меню)");
                Console.Write("Выбор: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        // Сохраняем состояние игры (завершенной) и возвращаемся в главное меню
                        SaveGameState(army1, army2, true);
                        exitBattle = true;
                        break;

                    case "2":
                        // Выход в главное меню
                        exitBattle = true;
                        break;

                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }
        }

        /// <summary>
        /// Сохраняет состояние текущей игры (армий) для возможности продолжения позже.
        /// Возвращает true если успешно сохранилось, иначе false.
        /// </summary>
        /// <summary>
        /// Сохраняет состояние игры автоматически во время боя (без запроса названия).
        /// Использует названия армий для создания названия сохранения.
        /// </summary>
        static void SaveGameDuringBattle(IArmy army1, IArmy army2)
        {
            // Автоматически генерируем имя сохранения на основе названия армий
            string saveName = $"{army1.Name}_vs_{army2.Name}";
            
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("СОХРАНЕНИЕ СОСТОЯНИЯ ИГРЫ");
            
            // Сохраняем состояние армий для продолжения игры
            armyManager.SaveArmies(army1, army2, saveName);
            
            // Сохраняем историю битвы с меткой что это незавершенная игра
            string gameStatus = "[Игра сохранена в процессе - может быть продолжена]\n\n";
            battleManager.SaveBattleLog(gameStatus, saveName, army1, army2);
            
            ConsoleMenu.ShowSuccess($"Игра сохранена как '{saveName}'!");
            Console.ReadKey();
        }

        /// <summary>
        /// Сохраняет состояние завершенной игры с возможностью задать название.
        /// Возвращает true если успешно сохранилось, иначе false.
        /// </summary>
        static bool SaveGameState(IArmy army1, IArmy army2, bool isGameComplete = false)
        {
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("СОХРАНЕНИЕ СОСТОЯНИЯ ИГРЫ");
            string saveName = ConsoleMenu.GetInput("Введите название для сохранения (без пробелов): ");
            
            if (!string.IsNullOrWhiteSpace(saveName))
            {
                // Сохраняем состояние армий для продолжения игры
                armyManager.SaveArmies(army1, army2, saveName);
                
                // Сохраняем историю битвы (стадия игры)
                string gameStatus = isGameComplete ? "" : "[Игра сохранена в процессе - может быть продолжена]\n\n";
                battleManager.SaveBattleLog(gameStatus, $"{army1.Name}_vs_{army2.Name}", army1, army2);
                
                ConsoleMenu.ShowSuccess($"Игра сохранена как '{saveName}'!");
                Console.ReadKey();
                return true;
            }
            
            return false;
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
                // Очищаем экран и выводим меню
                ConsoleMenu.ClearConsole();
                ConsoleMenu.PrintHeader("ИСТОРИИ БИТВ");
                Console.WriteLine("Доступные варианты:");
                
                // Выводим красивые имена битв
                for (int i = 0; i < battles.Length; i++)
                {
                    string displayName = battleManager.GetBattleDisplayName(battles[i]);
                    Console.WriteLine($"{i + 1}. {displayName}");
                }
                
                Console.Write("\nВыберите номер (0 - выход): ");
                
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= battles.Length)
                    {
                        string filePath = battleManager.GetLogPath(battles[choice - 1]);
                        string content = File.ReadAllText(filePath);

                        ConsoleMenu.ClearConsole();
                        string displayName = battleManager.GetBattleDisplayName(battles[choice - 1]);
                        Console.WriteLine($"История битвы: {displayName}");
                        Console.WriteLine();
                        Console.WriteLine(content);
                        ConsoleMenu.WaitForKey("\nНажмите любую клавишу для возврата к списку...");
                    }
                    else if (choice == 0)
                    {
                        exitMenu = true;
                    }
                    else
                    {
                        Console.WriteLine("Неверный выбор!");
                        Console.ReadKey();
                    }
                }
                else
                {
                    Console.WriteLine("Неверный ввод!");
                    Console.ReadKey();
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
        static int GetCommonBudget(int defaultBudget)
        {
            int budget = ConsoleMenu.GetIntInput($"\nВведите бюджет для обеих армий: ");
            if (budget <= 0)
                budget = defaultBudget;
            return budget;
        }
    }
}