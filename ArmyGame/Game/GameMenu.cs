using System;
using ArmyBattle.Models;
using ArmyBattle.Services;
using ArmyBattle.UI;

namespace ArmyBattle
{
    /// <summary>
    /// Класс для обработки главного меню игры
    /// </summary>
    static class GameMenu
    {
        // Ссылки на сервисы (инициализируются в Program)
        private static ArmyManager armyManager;
        private static BattleManager battleManager;
        private static IArmy? _lastArmy1;
        private static IArmy? _lastArmy2;

        /// <summary>
        /// Инициализация ссылок на сервисы и армии
        /// </summary>
        public static void Initialize(ArmyManager am, BattleManager bm, ref IArmy? army1, ref IArmy? army2)
        {
            armyManager = am;
            battleManager = bm;
            _lastArmy1 = army1;
            _lastArmy2 = army2;
        }

        /// <summary>
        /// Управляет процессом начала новой игры.
        /// Запрашивает названия армий, бюджет и способ создания для каждой армии.
        /// </summary>
        public static void StartNewGame()
        {
            // Очищаем экран перед началом
            ConsoleMenu.ClearConsole();

            // Получаем названия обеих армий от пользователя
            var (name1, name2) = ArmyCreation.GetArmyNames();
            
            // Получаем общий бюджет для обеих армий
            int budget = ArmyCreation.GetCommonBudget(200);

            // Создаем первую армию с красным цветом
            _lastArmy1 = new Army(name1, ConsoleColor.Red);
            
            // Создаем вторую армию с синим цветом
            _lastArmy2 = new Army(name2, ConsoleColor.Blue);

            // Спрашиваем способ создания первой армии
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader($"СОЗДАНИЕ АРМИИ: {name1}");

            bool useAutoForArmy1 = ArmyCreation.AskCreationMethod();

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
                ArmyCreation.SetupArmyManually(_lastArmy1, budget);
            }

            // Спрашиваем способ создания второй армии
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader($"СОЗДАНИЕ АРМИИ: {name2}");

            bool useAutoForArmy2 = ArmyCreation.AskCreationMethod();

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
                ArmyCreation.SetupArmyManually(_lastArmy2, budget);
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
                BattleMenu.StartBattle(_lastArmy1, _lastArmy2);
            }
        }

        // Загружает армии из сохраненного JSON файла
        public static void LoadArmiesFromDisk()
        {
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("ЗАГРУЗКА НЕЗАКОНЧЕННЫХ ИГР");

            string[] savedFiles = armyManager.GetUnfinishedGames();

            if (savedFiles.Length == 0)
            {
                ConsoleMenu.ShowMessage("Нет незаконченных игр для продолжения!");
                Console.ReadKey();
                return;
            }

            int choice = ConsoleMenu.ShowFileMenu(savedFiles, "Незаконченные игры");

            if (choice >= 1 && choice <= savedFiles.Length)
            {
                string filePath = armyManager.GetSavePath(savedFiles[choice - 1]);
                
                if (armyManager.LoadArmies(filePath, out IArmy army1, out IArmy army2, out int currentRound, out int attackTurn, out bool firstAttackerIsArmy1, out bool needNewRoundHeader))
                {
                    _lastArmy1 = army1;
                    _lastArmy2 = army2;

                    ConsoleMenu.ClearConsole();
                    ConsoleMenu.ShowSuccess("Игра загружена! Восстанавливаю боевое состояние...");
                    Console.WriteLine($"\n{_lastArmy1.Name}: {_lastArmy1.Units.Count} всего бойцов, живых: {_lastArmy1.AliveCount()}");
                    Console.WriteLine($"{_lastArmy2.Name}: {_lastArmy2.Units.Count} всего бойцов, живых: {_lastArmy2.AliveCount()}");

                    // Проверяем, есть ли возможность продолжить бой
                    if (_lastArmy1.HasAliveUnits() && _lastArmy2.HasAliveUnits())
                    {
                        Console.WriteLine("\nНажмите Enter для продолжения боя или другую клавишу для возврата...");
                        var key = Console.ReadKey();

                        if (key.Key == ConsoleKey.Enter)
                        {
                            // Продолжаем боевой цикл с загруженным состоянием
                            BattleMenu.ContinueBattle(_lastArmy1, _lastArmy2, currentRound, attackTurn, firstAttackerIsArmy1, needNewRoundHeader);
                        }
                    }
                    else
                    {
                        // Битва уже завершена - показываем результаты
                        Console.WriteLine("\nБитва уже завершена!");
                        if (_lastArmy1.HasAliveUnits())
                        {
                            Console.ForegroundColor = _lastArmy1.Color;
                            Console.WriteLine($"Победитель: {_lastArmy1.Name}!");
                            Console.ResetColor();
                        }
                        else if (_lastArmy2.HasAliveUnits())
                        {
                            Console.ForegroundColor = _lastArmy2.Color;
                            Console.WriteLine($"Победитель: {_lastArmy2.Name}!");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine("Ничья - обе армии уничтожены!");
                        }
                        Console.ReadKey();
                    }
                }
            }
        }

        // Показывает интерактивное меню для просмотра историй битв
        public static void ShowBattleLogs()
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

        // Удаляет всю историю битв и сохранений
        public static void DeleteAllHistory()
        {
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("Удаление всей истории");

            Console.WriteLine("Это действие удалит все сохраненные игры и истории битв!");
            Console.Write("Вы уверены? (д/н): ");
            
            string? choice = Console.ReadLine();
            if (choice?.ToLower() != "д")
            {
                Console.WriteLine("Удаление отменено.");
                Console.ReadKey();
                return;
            }

            try
            {
                // Удаляем все файлы из папки Saves
                string savesPath = Path.Combine(Directory.GetCurrentDirectory(), "Saves");
                if (Directory.Exists(savesPath))
                {
                    string[] saveFiles = Directory.GetFiles(savesPath);
                    foreach (string file in saveFiles)
                    {
                        File.Delete(file);
                    }
                    Console.WriteLine($"Удалено {saveFiles.Length} сохранений.");
                }

                // Удаляем все файлы из папки Logs
                string logsPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                if (Directory.Exists(logsPath))
                {
                    string[] logFiles = Directory.GetFiles(logsPath);
                    foreach (string file in logFiles)
                    {
                        File.Delete(file);
                    }
                    Console.WriteLine($"Удалено {logFiles.Length} историй битв.");
                }

                ConsoleMenu.ShowSuccess("Вся история успешно удалена!");
            }
            catch (Exception ex)
            {
                ConsoleMenu.ShowError($"Ошибка при удалении: {ex.Message}");
            }

            Console.ReadKey();
        }
    }
}