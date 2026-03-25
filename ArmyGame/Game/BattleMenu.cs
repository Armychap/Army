using System;
using System.IO;
using ArmyBattle.Models;
using ArmyBattle.Game;
using ArmyBattle.Services;
using ArmyBattle.UI;

namespace ArmyBattle
{
    /// <summary>
    /// Класс для управления битвами и их меню
    /// </summary>
    static class BattleMenu
    {
        // Ссылки на сервисы
        private static ArmyManager armyManager;
        private static BattleManager battleManager;

        /// <summary>
        /// Инициализация ссылок на сервисы
        /// </summary>
        public static void Initialize(ArmyManager am, BattleManager bm)
        {
            armyManager = am;
            battleManager = bm;
        }

        // Запускает боевой симулятор между двумя армиями с интерактивным меню
        public static void StartBattle(IArmy army1, IArmy army2)
        {
            // Захватываем вывод для логирования
            var originalOutput = Console.Out;
            var logCapture = new StringWriter();
            var compositeWriter = new CompositeTextWriter(originalOutput, logCapture);
            Console.SetOut(compositeWriter);

            try
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

                // Запускаем меню битвы
                RunBattleMenu(battle, army1, army2, "");

                // Восстанавливаем вывод
                Console.SetOut(originalOutput);

                // Сохраняем лог битвы (новая битва, используем timestamp)
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string logName = $"{army1.Name} vs {army2.Name}_{timestamp}";
                battleManager.SaveBattleLog(logCapture.ToString(), logName, army1, army2, useTimestamp: false);
            }
            catch (Exception ex)
            {
                Console.SetOut(originalOutput);
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Продолжает боевой цикл со стороны загруженной игры.
        /// Восстанавливает боевое состояние на основе текущего состояния юнитов (их здоровья и статуса).
        /// </summary>
        /// <summary>
        /// Восстанавливает боевое состояние на основе текущего состояния юнитов (их здоровья и статуса).
        /// </summary>
        public static void ContinueBattle(IArmy army1, IArmy army2, int currentRound, int attackTurn, bool firstAttackerIsArmy1, bool needNewRoundHeader, string saveName, string battleLogName)
        {
            // Захватываем вывод для логирования
            var originalOutput = Console.Out;
            var logCapture = new StringWriter();
            var compositeWriter = new CompositeTextWriter(originalOutput, logCapture);
            Console.SetOut(compositeWriter);

            try
            {
                // Создаем боевой движок для пошагового управления битвой
                BattleEngine battle = new BattleEngine(army1, army2, 400);
                
                // Устанавливаем сохраненное состояние битвы
                battle.SetBattleState(currentRound, attackTurn, firstAttackerIsArmy1, needNewRoundHeader);
                
                // Инициализируем армии для продолжения (обновляем живых бойцов без перемешивания)
                army1.RefreshAliveFighters();
                army2.RefreshAliveFighters();
                Console.WriteLine($"Армия 1 '{army1.Name}': {army1.Units.Count} юнитов всего, {army1.AliveFighters.Count} живых");
                Console.WriteLine($"Армия 2 '{army2.Name}': {army2.Units.Count} юнитов всего, {army2.AliveFighters.Count} живых");
                battle.SetCurrentFighters(null, null); // Бойцы выбираются в DoSingleRound
                battle.SetCurrentFightersForContinuation();
                battle.SetBattleInitialized(true);

                // Показываем заголовок битвы
                try { Console.Clear(); } catch { }
                Console.WriteLine("ПРОДОЛЖЕНИЕ БИТВЫ");
                Console.WriteLine($"{army1.Name} против {army2.Name}");
                Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
                Console.WriteLine();

                // Запускаем меню битвы
                RunBattleMenu(battle, army1, army2, saveName);

                // Восстанавливаем вывод
                Console.SetOut(originalOutput);

                // Сохраняем лог битвы (перезаписываем существующий файл)
                string logName = string.IsNullOrWhiteSpace(battleLogName) ? saveName : battleLogName;
                battleManager.SaveBattleLog(logCapture.ToString(), logName, army1, army2, useTimestamp: false);
            }
            catch (Exception ex)
            {
                Console.SetOut(originalOutput);
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Запускает меню управления битвой
        /// </summary>
        private static void RunBattleMenu(BattleEngine battle, IArmy army1, IArmy army2, string saveName)
        {
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
                        // Выполняем один ход
                        if (!battle.DoSingleMove())
                        {
                            // Битва закончилась
                            battleActive = false;
                        }
                        break;

                    case "2":
                        // Выполняем все ходы до конца без меню
                        Console.WriteLine("\nАвтоматическое проведение боя...\n");
                        while (battle.DoSingleMove())
                        {
                            System.Threading.Thread.Sleep(400);
                        }
                        battleActive = false;
                        break;

                    case "3":
                        // Сохраняем текущое состояние игры и выходим в главное меню
                        SaveGameDuringBattle(army1, army2, battle, saveName);
                        return;

                    case "4":
                        // Выход без завершения битвы
                        Console.WriteLine("\nВы уверены? Битва будет потеряна (д/н): ");
                        if (Console.ReadLine()?.ToLower() == "д")
                        {
                            // Автоматически сохранить игру перед выходом
                            string exitSaveName = string.IsNullOrWhiteSpace(saveName) ? $"{army1.Name}_vs_{army2.Name}" : saveName;
                            armyManager.SaveArmies(army1, army2, exitSaveName, battle.Round, battle.AttackTurn, battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader);
                            string gameStatus = "ИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                            bool isContinuation = !string.IsNullOrWhiteSpace(saveName);
                            battleManager.SaveBattleLog(gameStatus, exitSaveName, army1, army2, useTimestamp: !isContinuation);
                            ConsoleMenu.ShowSuccess($"Игра сохранена как '{exitSaveName}'!");
                            Console.ReadKey();
                            
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
            Console.WriteLine("БИТВА ЗАВЕРШЕНА");
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

            // Если это было продолжением сохраненной игры, удаляем файл сохранения
            if (!string.IsNullOrWhiteSpace(saveName))
            {
                string savePath = armyManager.GetSavePath(saveName);
                if (File.Exists(savePath))
                {
                    try
                    {
                        File.Delete(savePath);
                        Console.WriteLine($"Сохраненная игра '{saveName}' удалена (игра завершена).");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Не удалось удалить файл сохранения: {ex.Message}");
                    }
                }
            }

            // Меню после завершения битвы
            Console.WriteLine("\nМеню действий");
            Console.WriteLine("1. Выйти в главное меню");
            Console.Write("Выбор: ");

            // Ждем любого ввода для выхода
            Console.ReadLine();
        }

        /// <summary>
        /// Сохраняет состояние игры автоматически во время боя (без запроса названия).
        /// Использует названия армий для создания названия сохранения.
        /// </summary>
        public static void SaveGameDuringBattle(IArmy army1, IArmy army2)
        {
            SaveGameDuringBattle(army1, army2, null, "");
        }

        /// <summary>
        /// Сохраняет состояние игры автоматически во время боя (без запроса названия).
        /// Использует названия армий для создания названия сохранения.
        /// </summary>
        public static void SaveGameDuringBattle(IArmy army1, IArmy army2, BattleEngine? battle, string saveName)
        {
            // Используем переданное имя сохранения или генерируем новое
            string actualSaveName = string.IsNullOrWhiteSpace(saveName) ? $"{army1.Name} vs {army2.Name}" : saveName;
            
            // Определяем имя лога битвы
            bool isContinuation = !string.IsNullOrWhiteSpace(saveName);
            string battleLogName = isContinuation ? actualSaveName : $"{army1.Name} vs {army2.Name}_{DateTime.Now:yyyyMMdd_HHmmss}";
            
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("СОХРАНЕНИЕ СОСТОЯНИЯ ИГРЫ");
            
            // Сохраняем состояние армий для продолжения игры
            if (battle != null)
            {
                armyManager.SaveArmies(army1, army2, actualSaveName, battle.Round, battle.AttackTurn, battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader, battleLogName);
            }
            else
            {
                armyManager.SaveArmies(army1, army2, actualSaveName, battleLogName: battleLogName);
            }
            
            // Сохраняем историю битвы с информацией о незавершенной игре
            string gameStatus = "ИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
            string logFilePath = Path.Combine("Logs", battleLogName + ".txt");
            if (File.Exists(logFilePath))
            {
                // Дописываем к существующему логу
                File.AppendAllText(logFilePath, "\n" + gameStatus);
            }
            else
            {
                // Создаем новый лог
                battleManager.SaveBattleLog(gameStatus, battleLogName, army1, army2, useTimestamp: false);
            }
            
            ConsoleMenu.ShowSuccess($"Игра сохранена как '{actualSaveName}'!");
            Console.ReadKey();
        }

        /// <summary>
        /// Сохраняет состояние завершенной игры с возможностью задать название.
        /// Возвращает true если успешно сохранилось, иначе false.
        /// </summary>
        public static bool SaveGameState(IArmy army1, IArmy army2, bool isGameComplete = false)
        {
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("СОХРАНЕНИЕ СОСТОЯНИЯ ИГРЫ");
            string saveName = ConsoleMenu.GetInput("Введите название для сохранения (без пробелов): ");
            
            if (!string.IsNullOrWhiteSpace(saveName))
            {
                // Сохраняем состояние армий только для незавершенных игр
                if (!isGameComplete)
                {
                    armyManager.SaveArmies(army1, army2, saveName, battleLogName: saveName);
                }
                
                // Сохраняем историю битвы (стадия игры)
                string gameStatus = isGameComplete ? "" : "ИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                battleManager.SaveBattleLog(gameStatus, $"{army1.Name} vs {army2.Name}", army1, army2);
                
                ConsoleMenu.ShowSuccess($"Игра сохранена как '{saveName}'!");
                Console.ReadKey();
                return true;
            }
            
            return false;
        }
    }
}