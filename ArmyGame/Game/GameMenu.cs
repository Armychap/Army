using System;
using System.IO;
using ArmyBattle.Models;
using ArmyBattle.Services;
using ArmyBattle.UI;

namespace ArmyBattle
{
    public enum FormationType 
    { 
        OneColumn,    // Одна колонна
        ThreeColumns, // Три колонны
        Wall          // Стенка
    }
    
    /// <summary>
    /// Класс для обработки главного меню игры
    /// </summary>
    static class GameMenu
    {
        // Ссылки на сервисы (инициализируются в Program)
        private static ArmyManager? armyManager;
        private static BattleManager? battleManager;
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
                //
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

            // Применяем настройки наблюдателей к созданным армиям
            ObserverManager.ApplySettingsToArmies(_lastArmy1, _lastArmy2);

            Console.WriteLine("\nНастройка боевого построения:");
            FormationType selectedFormation = ArmyCreation.AskFormationType();
            Console.WriteLine($"Выбрано построение: {selectedFormation}");

            Console.WriteLine("\nНажмите Enter для начала битвы");

            // Читаем нажатую клавишу
            var key = Console.ReadKey();

            // Если нажата клавиша Enter - начинаем битву
            if (key.Key == ConsoleKey.Enter)
            {
                // Запускаем битву и получаем информацию о результате
                bool battleFinished = BattleMenu.StartBattle(_lastArmy1, _lastArmy2, selectedFormation);

                if (battleFinished)
                {
                    ConsoleMenu.ShowSuccess("Битва завершена. Результат сохранен в историю.");
                    Console.ReadKey();
                }
            }
        }

        // Показывает интерактивное меню для просмотра историй битв
        // GameMenu.cs
        public static void ShowBattleLogs()
        {
            // Получаем только завершенные битвы, исключая незавершенные игры
            string[] battles = battleManager?.GetFinishedBattles() ?? Array.Empty<string>();

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

                for (int i = 0; i < battles.Length; i++)
                {
                    string displayName = battleManager?.GetBattleDisplayName(battles[i]) ?? "Неизвестная битва";
                    Console.WriteLine($"{i + 1}. {displayName}");
                }

                Console.WriteLine("\n0. Назад в главное меню");
                Console.Write("\nВыберите битву для просмотра: ");

                string? choice = Console.ReadLine();
                if (int.TryParse(choice, out int index) && index >= 1 && index <= battles.Length)
                {
                    string selectedBattle = battles[index - 1];
                    string logContent = battleManager?.GetBattleLog(selectedBattle) ?? "";

                    if (!string.IsNullOrEmpty(logContent))
                    {
                        ConsoleMenu.ClearConsole();
                        ConsoleMenu.PrintHeader($"БИТВА: {battleManager?.GetBattleDisplayName(selectedBattle) ?? "Неизвестная битва"}");

                        Console.WriteLine(logContent);

                        Console.WriteLine("\n" + new string('=', 50));
                        Console.WriteLine("Нажмите любую клавишу для возврата в список игр...");
                        Console.ReadKey();
                    }
                    else
                    {
                        ConsoleMenu.ShowMessage("История битвы пуста!");
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
            string[] unfinished = armyManager?.GetUnfinishedGames() ?? Array.Empty<string>();

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
                        string? path = armyManager?.GetSavePath(save);

                        if (!string.IsNullOrWhiteSpace(path) && armyManager?.LoadArmies(path, out IArmy? army1, out IArmy? army2, out int round, out int attackTurn, out bool firstAtt, out bool needHeader, out string? battleLogName, out int moveCount, out FormationType currentFormation) == true && army1 != null && army2 != null)
                        {
                            // Выводим лог предыдущих ходов битвы
                            // Используем battleLogName для поиска логов, если он есть, иначе используем имя сохранения
                            string logFileName = !string.IsNullOrWhiteSpace(battleLogName) ? battleLogName : save;
                            string logPath = Path.Combine("Logs", logFileName + ".txt");
                            if (File.Exists(logPath))
                            {
                                string logContent = File.ReadAllText(logPath);
                                ConsoleMenu.ClearConsole();
                                ConsoleMenu.PrintHeader("ИСТОРИЯ БИТВЫ");
                                Console.WriteLine(logContent);
                                ConsoleMenu.WaitForKey("\nНажмите любую клавишу для продолжения битвы...");
                            }

                            BattleMenu.ContinueBattle(army1, army2, round, attackTurn, firstAtt, needHeader, save, battleLogName ?? save, moveCount, currentFormation);
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
                if (battleManager?.DeleteAllBattleLogs() == true)
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

        /// <summary>
        /// Настройка параметров прокси для юнитов
        /// </summary>
        public static void ConfigureProxySettings()
        {
            bool exit = false;
            while (!exit)
            {
                ConsoleMenu.ClearConsole();
                ConsoleMenu.PrintHeader("НАСТРОЙКИ ПРОКСИ");

                Console.WriteLine("Текущие настройки:");
                Console.WriteLine($"1. Логирование урона в файл: {(ObserverManager.IsDamageLogEnabled() ? "ВКЛ" : "ВЫКЛ")}");
                Console.WriteLine($"2. Бип при смерти юнита: {(ObserverManager.IsDeathBeepEnabled() ? "ВКЛ" : "ВЫКЛ")}");
                Console.WriteLine("3. Сбросить настройки");
                Console.WriteLine("4. Сохранить и выйти");
                Console.WriteLine("0. Назад");

                Console.Write("\nВыбор: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ObserverManager.SetDamageLogEnabled(!ObserverManager.IsDamageLogEnabled(), _lastArmy1, _lastArmy2);
                        ConsoleMenu.ShowMessage($"Логирование урона: {(ObserverManager.IsDamageLogEnabled() ? "ВКЛЮЧЕНО" : "ВЫКЛЮЧЕНО")}");
                        Console.ReadKey();
                        break;
                    case "2":
                        ObserverManager.SetDeathBeepEnabled(!ObserverManager.IsDeathBeepEnabled(), _lastArmy1, _lastArmy2);
                        ConsoleMenu.ShowMessage($"Звук при смерти: {(ObserverManager.IsDeathBeepEnabled() ? "ВКЛЮЧЕНО" : "ВЫКЛЮЧЕНО")}");
                        Console.ReadKey();
                        break;
                    case "3":
                        ProxySettings.Reset();
                        ObserverManager.LoadSettings(_lastArmy1, _lastArmy2);
                        ConsoleMenu.ShowMessage("Настройки сброшены!");
                        Console.ReadKey();
                        break;
                    case "4":
                        ProxySettings.Save();
                        ConsoleMenu.ShowSuccess("Настройки сохранены!");
                        Console.ReadKey();
                        exit = true;
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        ConsoleMenu.ShowMessage("Неверный выбор!");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}