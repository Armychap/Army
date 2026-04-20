using System;
using System.IO;
using System.Linq;
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
        private static ArmyManager? armyManager;
        private static BattleManager? battleManager;

        /// <summary>
        /// Инициализация ссылок на сервисы
        /// </summary>
        public static void Initialize(ArmyManager am, BattleManager bm)
        {
            armyManager = am;
            battleManager = bm;
        }

        // Запускает боевой симулятор между двумя армиями с интерактивным меню
        // Возвращает true если битва завершена естественным путем, иначе false
        public static bool StartBattle(IArmy army1, IArmy army2)
        {
            var originalOutput = Console.Out;
            var logCapture = new StringWriter();
            var compositeWriter = new CompositeTextWriter(originalOutput, logCapture);
            Console.SetOut(compositeWriter);

            try
            {
                BattleEngine battle = new BattleEngine(army1, army2, 400);
                battle.InitializeBattle();

                try { Console.Clear(); } catch { }
                Console.WriteLine("НАЧАЛО БИТВЫ");
                Console.WriteLine($"{army1.Name} против {army2.Name}");
                Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
                Console.WriteLine();

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string logName = $"{army1.Name} vs {army2.Name}";

                bool userExited = RunBattleMenu(battle, army1, army2, "", logName);

                Console.SetOut(originalOutput);

                string fullLog = logCapture.ToString();

                if (userExited)
                {
                    fullLog += "\nИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                    battleManager?.SaveBattleLog(fullLog, logName, army1, army2, useTimestamp: true);
                    return false;
                }

                 bool battleFinished = !battle.IsCombatActive || battle.StalemateReached;

                if (battleFinished)
                {
                    battleManager?.SaveBattleLog(fullLog, logName, army1, army2, useTimestamp: true);

                    //  Удаляем файл продолжения из Saves (используем logName как имя сохранения)
                    string? savePath = armyManager?.GetSavePath(logName);
                    if (!string.IsNullOrWhiteSpace(savePath) && File.Exists(savePath))
                    {
                        try
                        {
                            File.Delete(savePath);
                        }
                        catch { }
                    }

                    //  ПОСЛЕ восстановления консоли - выводим результаты ВТОРУЮ раз
                    ConsoleMenu.ClearConsole();
                    Console.WriteLine("БИТВА ЗАВЕРШЕНА");
                    Console.WriteLine(new string('=', 40));

                    // Определяем результат
                    if (battle.StalemateReached)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("НИЧЬЯ! Жизнь обоих бойцов не изменялась в течение 10 ходов.");
                        Console.ResetColor();
                    }
                    else if (army1.HasAliveUnits())
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

                    Console.WriteLine($"\nРаундов: {battle.Round}");
                    Console.WriteLine($"Ходов: {battle.MoveCount}");

                    Console.WriteLine($"\nРаундов: {battle.Round}");
                    Console.WriteLine($"Ходов: {battle.MoveCount}");

                    Console.WriteLine("\nМеню действий");
                    Console.WriteLine("1. Выйти в главное меню");
                    Console.Write("Выбор: ");
                    Console.ReadLine();

                    return true;
                }
                else
                {
                    fullLog += "\nИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                    battleManager?.SaveBattleLog(fullLog, logName, army1, army2, useTimestamp: true);

                    //  Восстанавливаем консоль перед сохранением
                    Console.SetOut(originalOutput);

                    //  Используем единый формат имени сохранения
                    armyManager?.SaveArmies(army1, army2, logName, battle.Round, battle.AttackTurn, battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader, logName, battle.MoveCount);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.SetOut(originalOutput);
                Console.WriteLine($"Ошибка: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Продолжает боевой цикл со стороны загруженной игры.
        /// Восстанавливает боевое состояние на основе текущего состояния юнитов (их здоровья и статуса).
        /// </summary>
        /// <summary>
        /// Восстанавливает боевое состояние на основе текущего состояния юнитов (их здоровья и статуса).
        /// </summary>
        // BattleMenu.cs
        public static void ContinueBattle(IArmy army1, IArmy army2, int currentRound, int attackTurn, bool firstAttackerIsArmy1, bool needNewRoundHeader, string saveName, string battleLogName, int moveCount = 0)
        {
            var originalOutput = Console.Out;
            var logCapture = new StringWriter();
            var compositeWriter = new CompositeTextWriter(originalOutput, logCapture);
            Console.SetOut(compositeWriter);

            try
            {
                BattleEngine battle = new BattleEngine(army1, army2, 400);
                battle.SetBattleState(currentRound, attackTurn, firstAttackerIsArmy1, needNewRoundHeader);
                battle.SetMoveCount(moveCount);

                // Восстанавливаем порядок бойцов и индекс уже в LoadArmies, здесь не перемешиваем заново.
                battle.SetCurrentFightersForContinuation();
                battle.SetBattleInitialized(true);

                try { Console.Clear(); } catch { }
                Console.WriteLine("ПРОДОЛЖЕНИЕ БИТВЫ");
                Console.WriteLine($"{army1.Name} против {army2.Name}");
                Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
                Console.WriteLine();

                string logName = string.IsNullOrWhiteSpace(battleLogName) ? saveName : battleLogName;

                //  Загружаем существующий лог, если есть
                string existingLog = "";
                string existingLogPath = Path.Combine("Logs", $"{logName}.txt");
                if (File.Exists(existingLogPath))
                {
                    existingLog = File.ReadAllText(existingLogPath);
                    Console.WriteLine("Предыдущая история битвы загружена.");
                    Console.WriteLine();
                }

                bool userExited = RunBattleMenu(battle, army1, army2, saveName, logName);

                Console.SetOut(originalOutput);

                // Если пользователь вышел из меню, сохраняем лог с указанием что игра не завершена
                if (userExited)
                {
                    // Битва не завершена - добавляем статус к существующему логу
                    string newLog = existingLog + logCapture.ToString();
                    newLog += "\nИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                    battleManager?.SaveBattleLog(newLog, logName, army1, army2, useTimestamp: false);
                    return;
                }

                //  Сохраняем с existingLog + новый лог
                string fullLog = existingLog + logCapture.ToString();

                // Проверяем, была ли битва завершена
                bool battleFinished = !battle.IsCombatActive || battle.StalemateReached;
                if (battleFinished)
                {
                    // Битва завершена - формируем итоговый лог без метки незавершенной игры
                    if (fullLog.Contains("ИГРА НЕ ЗАВЕРШЕНА"))
                    {
                        int markerIndex = fullLog.IndexOf("ИГРА НЕ ЗАВЕРШЕНА");
                        if (markerIndex >= 0)
                        {
                            fullLog = fullLog.Substring(0, markerIndex).TrimEnd();
                        }
                    }

                    battleManager?.SaveBattleLog(fullLog, logName, army1, army2, useTimestamp: false);

                    // Удаляем файл продолжения из Saves (игра больше не продолжится)
                    if (!string.IsNullOrWhiteSpace(saveName))
                    {
                        string? savePath = armyManager?.GetSavePath(saveName);
                        if (!string.IsNullOrWhiteSpace(savePath) && File.Exists(savePath))
                        {
                            try
                            {
                                File.Delete(savePath);
                                Console.WriteLine($"Сохраненная игра '{saveName}' удалена (игра завершена).");
                            }
                            catch { }
                        }
                    }

                    //  ПОСЛЕ восстановления консоли - выводим результаты
                    ConsoleMenu.ClearConsole();
                    Console.WriteLine("БИТВА ЗАВЕРШЕНА");
                    Console.WriteLine(new string('=', 40));

                    // Определяем результат
                    if (battle.StalemateReached)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("НИЧЬЯ! Жизнь обоих бойцов не изменялась в течение 10 ходов.");
                        Console.ResetColor();
                    }
                    else if (army1.HasAliveUnits())
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

                    Console.WriteLine($"\nРаундов: {battle.Round}");
                    Console.WriteLine($"Ходов: {battle.MoveCount}");

                    Console.WriteLine("\nМеню действий");
                    Console.WriteLine("1. Выйти в главное меню");
                    Console.Write("Выбор: ");
                    Console.ReadLine();
                }
                else
                {
                    // Битва не завершена - добавляем статус
                    fullLog += "\nИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                    battleManager?.SaveBattleLog(fullLog, logName, army1, army2, useTimestamp: false);

                    // Обновляем состояние незавершенной игры
                    armyManager?.SaveArmies(army1, army2, saveName, battle.Round, battle.AttackTurn, battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader, logName, battle.MoveCount);
                }
            }
            catch (Exception ex)
            {
                Console.SetOut(originalOutput);
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Запускает меню управления битвой
        /// Возвращает true если пользователь вышел через case "4" (сохранение и выход), иначе false
        /// </summary>
        private static bool RunBattleMenu(BattleEngine battle, IArmy army1, IArmy army2, string saveName, string battleLogName = "")
        {
            // Меню управления боем
            bool battleActive = true;
            while (battleActive && (army1.HasAliveUnits() && army2.HasAliveUnits()))
            {
                // Выводим меню управления боем
                Console.WriteLine("\nМеню действий");
                Console.WriteLine("1. Сделать ход");
                Console.WriteLine("2. Пройти раунд");
                Console.WriteLine("3. Автоматически пройти до конца");
                Console.WriteLine("4. Посмотреть состояние");
                Console.WriteLine("5. Посмотреть баффы");
                Console.WriteLine("6. Изменить боевое построение");
                Console.WriteLine("7. Сохранить игру");
                Console.WriteLine("8. Выйти (назад в меню)");
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
                        // Пройти текущий раунд (до следующей смены раунда)
                        if (battle.NeedNewRoundHeader)
                        {
                            if (!battle.DoSingleMove())
                            {
                                battleActive = false;
                                break;
                            }
                        }

                        while (!battle.NeedNewRoundHeader && army1.HasAliveUnits() && army2.HasAliveUnits())
                        {
                            if (!battle.DoSingleMove())
                            {
                                battleActive = false;
                                break;
                            }
                        }
                        break;

                    case "3":
                        // Выполняем все ходы до конца без меню
                        Console.WriteLine("\nАвтоматическое проведение боя...\n");
                        while (battle.DoSingleMove())
                        {
                            System.Threading.Thread.Sleep(400);
                        }
                        battleActive = false;
                        break;

                    case "4":
                        // Посмотреть состояние (порядок боя)
                        battle.DisplayBattleOrder();
                        Console.WriteLine("Нажмите любую клавишу для продолжения...");
                        Console.ReadKey();
                        break;

                    case "5":
                        // Посмотреть баффы бойцов
                        DisplayBuffs(army1, army2);
                        break;

                    case "6":
                        ChangeFormationDuringBattle(battle);
                        break;

                    case "7":
                        // Сохраняем текущее состояние игры и выходим в главное меню
                        SaveGameDuringBattle(army1, army2, battle, saveName, battleLogName);
                        return true;  //  Указываем что пользователь вышел
                    case "8":
                        Console.WriteLine("\nВы уверены? Битва будет потеряна (д/н): ");
                        if (Console.ReadLine()?.ToLower() == "д")
                        {
                            //  Используем battleLogName в качестве имени сохранения для единообразия
                            string exitSaveName = string.IsNullOrWhiteSpace(saveName) ? battleLogName : saveName;
                            string exitLogName = string.IsNullOrWhiteSpace(battleLogName) ? saveName : battleLogName;

                            Console.WriteLine("\nИГРА НЕ ЗАВЕРШЕНА");
                            Console.WriteLine("Состояние армий сохранено для продолжения.");

                            //  Сохраняем армии с состоянием битвы и логом
                            armyManager?.SaveArmies(army1, army2, exitSaveName, battle.Round, battle.AttackTurn, battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader, exitLogName, battle.MoveCount);

                            ConsoleMenu.ShowSuccess($"Игра сохранена!");
                            Console.ReadKey();

                            battleActive = false;
                            return true;  //  Указываем что пользователь вышел
                        }
                        break;

                    default:
                        Console.WriteLine("Неверный выбор!");
                        break;
                }
            }

            //  НЕ выводим финальное меню здесь - оно попадает в логирование
            // Возвращаем false чтобы сигнализировать что битва естественно завершена
            return false;  //  Возвращаем false - битва завершена естественным путем
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
        public static void SaveGameDuringBattle(IArmy army1, IArmy army2, BattleEngine? battle, string saveName, string battleLogNameParam = "")
        {
            // Используем переданное имя сохранения или генерируем новое
            string actualSaveName = string.IsNullOrWhiteSpace(saveName) ? $"{army1.Name} vs {army2.Name}" : saveName;

            // Определяем имя лога битвы
            bool isContinuation = !string.IsNullOrWhiteSpace(saveName);
            string battleLogName = !string.IsNullOrWhiteSpace(battleLogNameParam) ? battleLogNameParam : (isContinuation ? actualSaveName : $"{army1.Name} vs {army2.Name}");

            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("СОХРАНЕНИЕ СОСТОЯНИЯ ИГРЫ");

            // Сохраняем состояние армий для продолжения игры
            if (battle != null)
            {
                armyManager?.SaveArmies(army1, army2, actualSaveName, battle.Round, battle.AttackTurn, battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader, battleLogName, battle.MoveCount);
            }
            else
            {
                armyManager?.SaveArmies(army1, army2, actualSaveName, battleLogName: battleLogName);
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
                // Создаем новый лог с только статусом (основные логи добавятся позже при завершении)
                if (!Directory.Exists("Logs"))
                    Directory.CreateDirectory("Logs");
                File.WriteAllText(logFilePath, gameStatus);
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
            string? saveName = ConsoleMenu.GetInput("Введите название для сохранения (без пробелов): ");

            if (!string.IsNullOrWhiteSpace(saveName))
            {
                // Сохраняем состояние армий только для незавершенных игр
                if (!isGameComplete)
                {
                    armyManager?.SaveArmies(army1, army2, saveName, battleLogName: saveName);
                }

                // Сохраняем историю битвы (стадия игры)
                string gameStatus = isGameComplete ? "" : "ИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                battleManager?.SaveBattleLog(gameStatus, $"{army1.Name} vs {army2.Name}", army1, army2);

                ConsoleMenu.ShowSuccess($"Игра сохранена как '{saveName}'!");
                Console.ReadKey();
                return true;
            }

            return false;
        }

        private static void DisplayBuffs(IArmy army1, IArmy army2)
        {
            Console.Clear();
            Console.WriteLine("Бафы бойцов");
            Console.WriteLine(new string('=', 40));

            void PrintArmyBuffs(IArmy army)
            {
                Console.ForegroundColor = army.Color;
                Console.WriteLine($"{army.Name}:");
                Console.ResetColor();

                var buffedUnits = army.Units
                    .Where(u => u is StrongFighter sf && sf.Buffs.Count > 0)
                    .Cast<StrongFighter>()
                    .ToList();

                if (!buffedUnits.Any())
                {
                    Console.WriteLine("  Нет бойцов с баффами.");
                    return;
                }

                foreach (var sf in buffedUnits)
                {
                    var buffNames = sf.Buffs.Select(b => b.Name);
                    Console.WriteLine($"  {sf.GetDisplayName(army.Name)}: {string.Join(", ", buffNames)}");
                }
            }

            PrintArmyBuffs(army1);
            Console.WriteLine();
            PrintArmyBuffs(army2);

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ChangeFormationDuringBattle(BattleEngine battle)
        {
            Console.WriteLine("\nВыберите тип построения:");
            Console.WriteLine("1. Одна колонна");
            Console.WriteLine("2. Три колонны");
            Console.WriteLine("3. Стенка");
            Console.Write("Ваш выбор: ");

            string? input = Console.ReadLine();

            FormationType chosenFormation = input switch
            {
                "1" => FormationType.OneColumn,
                "2" => FormationType.ThreeColumns,
                "3" => FormationType.Wall,
                _ => FormationType.OneColumn
            };

            battle.ReinitializeFormation(chosenFormation);
            Console.WriteLine($"Выбрано построение: {chosenFormation}");

            if (chosenFormation != FormationType.OneColumn)
            {
                Console.WriteLine("Режим три колонны активирован.");
            }

            Console.WriteLine("Нажмите любую клавишу для возврата в меню боя...");
            Console.ReadKey(true);
        }

    }
}