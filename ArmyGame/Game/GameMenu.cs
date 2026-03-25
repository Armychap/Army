using System;
using System.IO;
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

            bool backToMain = false;

            while (!backToMain)
            {
                ConsoleMenu.ClearConsole();
                ConsoleMenu.PrintHeader("ИСТОРИЯ БИТВ");

                // Показываем список битв с номерами
                for (int i = 0; i < battles.Length; i++)
                {
                    string displayName = battleManager.GetBattleDisplayName(battles[i]);
                    Console.WriteLine($"{i + 1}. {displayName}");
                }

                Console.WriteLine("\n0. Назад в главное меню");
                Console.Write("\nВыберите битву для просмотра: ");

                string? choice = Console.ReadLine();
                if (int.TryParse(choice, out int index) && index >= 1 && index <= battles.Length)
                {
                    string selectedBattle = battles[index - 1];
                    string logContent = battleManager.GetBattleLog(selectedBattle);

                    if (!string.IsNullOrEmpty(logContent))
                    {
                        // Очищаем экран и показываем заголовок
                        ConsoleMenu.ClearConsole();
                        ConsoleMenu.PrintHeader($"БИТВА: {battleManager.GetBattleDisplayName(selectedBattle)}");

                        // Выводим полный лог битвы
                        Console.WriteLine(logContent);

                        Console.WriteLine("\n" + new string('=', 50));
                        Console.WriteLine("Нажмите любую клавишу для возврата в список игр...");
                        Console.ReadKey();
                        // После просмотра возвращаемся в список (цикл продолжается)
                    }
                    else
                    {
                        ConsoleMenu.ShowMessage("История битвы еще не записана (битва не завершена или не начата)!");
                        Console.ReadKey();
                    }
                }
                else if (choice == "0")
                {
                    backToMain = true;
                }
                else
                {
                    ConsoleMenu.ShowMessage("Неверный выбор!");
                    Console.ReadKey();
                }
            }
        }

        // Продолжает незавершенную игру из сохранений
        public static void ContinueSavedGame()
        {
            string[] unfinished = armyManager.GetUnfinishedGames();

            if (unfinished.Length == 0)
            {
                ConsoleMenu.ClearConsole();
                ConsoleMenu.ShowMessage("Нет незавершенных игр для продолжения.");
                Console.ReadKey();
                return;
            }

            bool backToMenu = false;
            while (!backToMenu)
            {
                ConsoleMenu.ClearConsole();
                ConsoleMenu.PrintHeader("ПРОДОЛЖЕНИЕ ИГРЫ");

                for (int i = 0; i < unfinished.Length; i++)
                    Console.WriteLine($"{i + 1}. {unfinished[i]}");

                Console.WriteLine("\n0. Назад в главное меню");
                Console.Write("Выберите сохранение: ");

                string? input = Console.ReadLine();
                if (int.TryParse(input, out int idx))
                {
                    if (idx == 0)
                    {
                        backToMenu = true;
                        continue;
                    }

                    if (idx >= 1 && idx <= unfinished.Length)
                    {
                        var save = unfinished[idx - 1];
                        string path = armyManager.GetSavePath(save);

                        if (armyManager.LoadArmies(path, out IArmy army1, out IArmy army2, out int round, out int attackTurn, out bool firstAtt, out bool needHeader, out string battleLogName))
                        {
                            // Выводим лог предыдущих ходов битвы
                            string logPath = Path.Combine("Logs", save + ".txt");
                            if (File.Exists(logPath))
                            {
                                string logContent = File.ReadAllText(logPath);
                                ConsoleMenu.ClearConsole();
                                ConsoleMenu.PrintHeader("ИСТОРИЯ БИТВЫ");
                                Console.WriteLine(logContent);
                                ConsoleMenu.WaitForKey("\nНажмите любую клавишу для продолжения битвы...");
                            }

                            BattleMenu.ContinueBattle(army1, army2, round, attackTurn, firstAtt, needHeader, save, battleLogName);
                            return;
                        }
                        else
                        {
                            ConsoleMenu.ShowMessage("Не удалось загрузить сохранение.");
                            Console.ReadKey();
                        }
                    }
                    else
                    {
                        ConsoleMenu.ShowMessage("Неверный выбор.");
                        Console.ReadKey();
                    }
                }
                else
                {
                    ConsoleMenu.ShowMessage("Неверный ввод.");
                    Console.ReadKey();
                }
            }
        }

        // Удаляет всю историю битв с подтверждением
        public static void DeleteAllHistory()
        {
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("УДАЛЕНИЕ ИСТОРИИ БИТВ");

            Console.WriteLine("Вы уверены, что хотите удалить ВСЮ историю битв?");
            Console.WriteLine("Это действие невозможно отменить!");
            Console.WriteLine("\n1. Да, удалить все");
            Console.WriteLine("2. Отменить");
            Console.Write("\nВыбор: ");

            string? choice = Console.ReadLine();

            if (choice == "1")
            {
                if (battleManager.DeleteAllBattleLogs())
                {
                    ConsoleMenu.ClearConsole();
                    ConsoleMenu.ShowSuccess("Вся история битв успешно удалена!");
                    Console.ReadKey();
                }
                else
                {
                    ConsoleMenu.ClearConsole();
                    ConsoleMenu.ShowMessage("Ошибка при удалении истории!");
                    Console.ReadKey();
                }
            }
            else if (choice == "2")
            {
                // Отмена - просто возвращаемся
            }
            else
            {
                ConsoleMenu.ShowMessage("Неверный выбор!");
                Console.ReadKey();
            }
        }
    }
}