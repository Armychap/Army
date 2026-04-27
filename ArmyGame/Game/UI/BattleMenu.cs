using System;
using System.IO;
using System.Linq;
using ArmyBattle.Models;
using ArmyBattle.Game;
using ArmyBattle.Services;
using ArmyBattle.Models.Decorators;
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
        public static bool StartBattle(IArmy army1, IArmy army2, FormationType formation = FormationType.OneColumn)
        {
            var originalOutput = Console.Out;
            var logCapture = new StringWriter();
            var compositeWriter = new CompositeTextWriter(originalOutput, logCapture);
            Console.SetOut(compositeWriter);

            try
            {
                BattleEngine battle = new BattleEngine(army1, army2, 400);

                //  ДЛЯ НОВОЙ БИТВЫ - просто инициализируем с выбранным построением
                battle.InitializeBattle(formation);  // ← ИСПРАВЛЕНО: убрать RestoreFromSave

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
                    DisplayEndGameStats(army1, army2, battle);

                    fullLog = logCapture.ToString();
                    battleManager?.SaveBattleLog(fullLog, logName, army1, army2, useTimestamp: true);

                    // Удаляем файл продолжения
                    string? savePath = armyManager?.GetSavePath(logName);
                    if (!string.IsNullOrWhiteSpace(savePath) && File.Exists(savePath))
                    {
                        try { File.Delete(savePath); } catch { }
                    }

                    Console.SetOut(originalOutput);
                    Console.WriteLine("\nНажмите любую клавишу для возврата в главное меню...");
                    Console.ReadKey();
                    return true;

                }
                else
                {
                    fullLog += "\nИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                    battleManager?.SaveBattleLog(fullLog, logName, army1, army2, useTimestamp: true);

                    // Восстанавливаем консоль перед сохранением
                    Console.SetOut(originalOutput);

                    // Используем единый формат имени сохранения
                    armyManager?.SaveArmies(army1, army2, logName, battle.Round, battle.AttackTurn, battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader, logName, battle.MoveCount, formation);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.SetOut(originalOutput);
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.ReadKey();
                return false;
            }
        }

        private static void DisplayEndGameStats(IArmy army1, IArmy army2, BattleEngine battle)
        {
            ConsoleMenu.ClearConsole();
            Console.WriteLine("БИТВА ЗАВЕРШЕНА");
            Console.WriteLine(new string('=', 40));

            if (battle.StalemateReached)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("НИЧЬЯ!");
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

            // Статистика
            Console.WriteLine("\nСТАТИСТИКА БИТВЫ:");
            Console.WriteLine($"Всего ходов: {battle.MoveCount}");

            // Армия 1
            Console.WriteLine($"\n{army1.Name}:");
            Console.WriteLine($"Выжило бойцов: {army1.AliveCount()}/{army1.Units.Count}");
            var alive1 = army1.Units.Where(u => u.IsAlive).ToList();
            if (alive1.Any())
            {
                Console.Write("  Выжившие: ");
                for (int i = 0; i < alive1.Count; i++)
                {
                    var unit = alive1[i];
                    Console.Write($"{unit.GetDisplayName(army1.Name)} ({unit.PowerLevel})");
                    if (i < alive1.Count - 1) Console.Write(", ");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("  Все бойцы погибли");
            }
            Console.WriteLine($"Добавлено бойцов: {battle.Army1AddedFightersCount}");
            Console.WriteLine($"Надето баффов: {battle.Army1BuffsAppliedCount}");

            // Армия 2
            Console.WriteLine($"\n{army2.Name}:");
            Console.WriteLine($"Выжило бойцов: {army2.AliveCount()}/{army2.Units.Count}");
            var alive2 = army2.Units.Where(u => u.IsAlive).ToList();
            if (alive2.Any())
            {
                Console.Write("  Выжившие: ");
                for (int i = 0; i < alive2.Count; i++)
                {
                    var unit = alive2[i];
                    Console.Write($"{unit.GetDisplayName(army2.Name)} ({unit.PowerLevel})");
                    if (i < alive2.Count - 1) Console.Write(", ");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("  Все бойцы погибли");
            }
            Console.WriteLine($"Добавлено бойцов: {battle.Army2AddedFightersCount}");
            Console.WriteLine($"Надето баффов: {battle.Army2BuffsAppliedCount}");
        }


        /// <summary>
        /// Продолжает боевой цикл со стороны загруженной игры.
        /// Восстанавливает боевое состояние на основе текущего состояния юнитов (их здоровья и статуса).
        /// </summary>
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

                        //  Объявляем переменные здесь, ДО использования
                        if (!string.IsNullOrWhiteSpace(path) && armyManager?.LoadArmies(path, out IArmy? army1, out IArmy? army2, out int round, out int attackTurn, out bool firstAtt, out bool needHeader, out string? battleLogName, out int moveCount, out FormationType currentFormation) == true && army1 != null && army2 != null)
                        {
                            // Выводим лог предыдущих ходов битвы
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

                            //  Используем переменные, которые получили из LoadArmies
                            ContinueBattle(army1, army2, round, attackTurn, firstAtt, needHeader, save, battleLogName ?? save, moveCount, currentFormation);
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
                Console.WriteLine("2. Автоматически пройти до конца");
                Console.WriteLine("3. Посмотреть состояние");
                Console.WriteLine("4. Посмотреть баффы");
                Console.WriteLine("5. Изменить боевое построение");
                Console.WriteLine("6. Сохранить игру");
                Console.WriteLine("7. Выйти (назад в меню)");
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
                        // Посмотреть состояние (порядок боя)
                        Console.WriteLine();
                        battle.DisplayBattleOrder();
                        break;

                    case "4":
                        // Посмотреть баффы бойцов
                        DisplayBuffs(army1, army2);
                        break;

                    case "5":
                        ChangeFormationDuringBattle(battle);
                        break;

                    case "6":
                        // Сохраняем текущее состояние игры и выходим в главное меню
                        SaveGameDuringBattle(army1, army2, battle, saveName, battleLogName);
                        return true;  //  Указываем что пользователь вышел
                    case "7":
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
                    .Where(u => u is BuffDecorator)
                    .ToList();

                if (!buffedUnits.Any())
                {
                    Console.WriteLine("  Нет бойцов с баффами.");
                    return;
                }

                foreach (var unit in buffedUnits)
                {
                    // Собираем названия всех баффов на этом юните
                    var buffNames = new List<string>();
                    var current = unit;
                    while (current is BuffDecorator decorator)
                    {
                        string buffName = decorator switch
                        {
                            HorseBuffDecorator => "Конь",
                            ShieldBuffDecorator => "Щит",
                            HelmetBuffDecorator => "Шлем",
                            SpearBuffDecorator => "Копье",
                            _ => "?"
                        };
                        buffNames.Add(buffName);
                        current = decorator.GetInnerUnit();
                    }

                    Console.WriteLine($"  {unit.GetDisplayName(army.Name)}: {string.Join(", ", buffNames)}");
                }
            }

            PrintArmyBuffs(army1);
            Console.WriteLine();
            PrintArmyBuffs(army2);

            Console.WriteLine();
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
        }

        /// <summary>
        /// Продолжает боевой цикл со стороны загруженной игры.
        /// </summary>
        public static void ContinueBattle(IArmy army1, IArmy army2, int currentRound, int attackTurn, bool firstAttackerIsArmy1, bool needNewRoundHeader, string saveName, string battleLogName, int moveCount = 0, FormationType formation = FormationType.OneColumn)
        {
            var originalOutput = Console.Out;
            var logCapture = new StringWriter();
            var compositeWriter = new CompositeTextWriter(originalOutput, logCapture);
            Console.SetOut(compositeWriter);

            try
            {
                BattleEngine battle = new BattleEngine(army1, army2, 400);

                //  Восстанавливаем состояние БЕЗ вызова InitializeBattle
                battle.SetBattleState(currentRound, attackTurn, firstAttackerIsArmy1, needNewRoundHeader);
                battle.SetMoveCount(moveCount);
                battle.SetFormationStrategy(formation);

                //  Восстанавливаем текущих бойцов и специфичные для стратегии данные
                if (formation == FormationType.OneColumn)
                {
                    battle.SetCurrentFightersForContinuation();
                }
                else if (formation == FormationType.ThreeColumns)
                {
                    battle.InitializeThreeColumns();  // Восстанавливаем трёхколонный порядок
                }
                else if (formation == FormationType.Wall)
                {
                    // Для стенки вызываем Reinitialize, чтобы перестроить пары
                    battle.GetCurrentStrategy()?.Reinitialize(battle);
                }

                battle.SetBattleInitialized(true);

                try { Console.Clear(); } catch { }
                Console.WriteLine("ПРОДОЛЖЕНИЕ БИТВЫ");
                Console.WriteLine($"{army1.Name} против {army2.Name}");
                Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
                Console.WriteLine();

                string logName = string.IsNullOrWhiteSpace(battleLogName) ? saveName : battleLogName;

                // Загружаем существующий лог, если есть
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

                if (userExited)
                {
                    string newLog = existingLog + logCapture.ToString();
                    newLog += "\nИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                    battleManager?.SaveBattleLog(newLog, logName, army1, army2, useTimestamp: false);
                    return;
                }

                string fullLog = existingLog + logCapture.ToString();
                bool battleFinished = !battle.IsCombatActive || battle.StalemateReached;

                if (battleFinished)
                {
                    if (fullLog.Contains("ИГРА НЕ ЗАВЕРШЕНА"))
                    {
                        int markerIndex = fullLog.IndexOf("ИГРА НЕ ЗАВЕРШЕНА");
                        if (markerIndex >= 0)
                        {
                            fullLog = fullLog.Substring(0, markerIndex).TrimEnd();
                        }
                    }

                    battleManager?.SaveBattleLog(fullLog, logName, army1, army2, useTimestamp: false);

                    if (!string.IsNullOrWhiteSpace(saveName))
                    {
                        string? savePath = armyManager?.GetSavePath(saveName);
                        if (!string.IsNullOrWhiteSpace(savePath) && File.Exists(savePath))
                        {
                            try { File.Delete(savePath); } catch { }
                        }
                    }

                    ConsoleMenu.ClearConsole();
                    Console.WriteLine("БИТВА ЗАВЕРШЕНА");
                    Console.WriteLine(new string('=', 40));

                    if (battle.StalemateReached)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("НИЧЬЯ!");
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

                    Console.WriteLine("\nСТАТИСТИКА БИТВЫ:");
                    Console.WriteLine($"Всего ходов: {battle.MoveCount}");

                    Console.WriteLine($"\n{army1.Name}:");
                    Console.WriteLine($"Выжило бойцов: {army1.AliveCount()}/{army1.Units.Count}");
                    var alive1 = army1.Units.Where(u => u.IsAlive).ToList();
                    if (alive1.Any())
                    {
                        Console.Write("  Выжившие: ");
                        for (int i = 0; i < alive1.Count; i++)
                        {
                            var unit = alive1[i];
                            Console.Write($"{unit.GetDisplayName(army1.Name)} ({unit.PowerLevel})");
                            if (i < alive1.Count - 1) Console.Write(", ");
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine("  Все бойцы погибли");
                    }

                    Console.WriteLine($"\n{army2.Name}:");
                    Console.WriteLine($"Выжило бойцов: {army2.AliveCount()}/{army2.Units.Count}");
                    var alive2 = army2.Units.Where(u => u.IsAlive).ToList();
                    if (alive2.Any())
                    {
                        Console.Write("  Выжившие: ");
                        for (int i = 0; i < alive2.Count; i++)
                        {
                            var unit = alive2[i];
                            Console.Write($"{unit.GetDisplayName(army2.Name)} ({unit.PowerLevel})");
                            if (i < alive2.Count - 1) Console.Write(", ");
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine("  Все бойцы погибли");
                    }

                    Console.WriteLine("\nНажмите любую клавишу для возврата в главное меню...");
                    Console.ReadKey();
                    return;
                }
                else
                {
                    fullLog += "\nИГРА НЕ ЗАВЕРШЕНА\nСостояние армий сохранено для продолжения.";
                    battleManager?.SaveBattleLog(fullLog, logName, army1, army2, useTimestamp: false);
                    armyManager?.SaveArmies(army1, army2, saveName, battle.Round, battle.AttackTurn,
                        battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader, logName, battle.MoveCount, formation);


                }
            }
            catch (Exception ex)
            {
                Console.SetOut(originalOutput);
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}