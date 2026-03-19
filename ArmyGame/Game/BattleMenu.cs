using System;
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
            RunBattleMenu(battle, army1, army2);
        }

        /// <summary>
        /// Продолжает боевой цикл со стороны загруженной игры.
        /// Восстанавливает боевое состояние на основе текущего состояния юнитов (их здоровья и статуса).
        /// </summary>
        /// <summary>
        /// Восстанавливает боевое состояние на основе текущего состояния юнитов (их здоровья и статуса).
        /// </summary>
        public static void ContinueBattle(IArmy army1, IArmy army2, int currentRound, int attackTurn, bool firstAttackerIsArmy1, bool needNewRoundHeader)
        {
            // Создаем боевой движок для пошагового управления битвой
            BattleEngine battle = new BattleEngine(army1, army2, 400);
            
            // Устанавливаем сохраненное состояние битвы
            battle.SetBattleState(currentRound, attackTurn, firstAttackerIsArmy1, needNewRoundHeader);
            
            // Инициализируем битву (готовим армии)
            battle.InitializeBattle();

            // Показываем заголовок битвы
            try { Console.Clear(); } catch { }
            Console.WriteLine("ПРОДОЛЖЕНИЕ БИТВЫ");
            Console.WriteLine($"{army1.Name} против {army2.Name}");
            Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
            Console.WriteLine();

            // Запускаем меню битвы
            RunBattleMenu(battle, army1, army2);
        }

        /// <summary>
        /// Запускает меню управления битвой
        /// </summary>
        private static void RunBattleMenu(BattleEngine battle, IArmy army1, IArmy army2)
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
                        SaveGameDuringBattle(army1, army2, battle);
                        return;

                    case "4":
                        // Выход без завершения битвы
                        Console.WriteLine("\nВы уверены? Битва будет потеряна (д/н): ");
                        if (Console.ReadLine()?.ToLower() == "д")
                        {
                            // Автоматически сохранить игру перед выходом
                            string saveName = $"{army1.Name}_vs_{army2.Name}";
                            armyManager.SaveArmies(army1, army2, saveName, battle.Round, battle.AttackTurn, battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader);
                            string gameStatus = "";
                            battleManager.SaveBattleLog(gameStatus, saveName, army1, army2);
                            ConsoleMenu.ShowSuccess($"Игра сохранена как '{saveName}'!");
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

            // Автоматически сохраняем историю битвы
            Console.WriteLine("\nСохраняю историю битвы");
            battleManager.SaveBattleLog("", $"{army1.Name} vs {army2.Name}", army1, army2);
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
        /// Сохраняет состояние игры автоматически во время боя (без запроса названия).
        /// Использует названия армий для создания названия сохранения.
        /// </summary>
        public static void SaveGameDuringBattle(IArmy army1, IArmy army2)
        {
            SaveGameDuringBattle(army1, army2, null);
        }

        /// <summary>
        /// Сохраняет состояние игры автоматически во время боя (без запроса названия).
        /// Использует названия армий для создания названия сохранения.
        /// </summary>
        public static void SaveGameDuringBattle(IArmy army1, IArmy army2, BattleEngine? battle)
        {
            // Автоматически генерируем имя сохранения на основе названия армий
            string saveName = $"{army1.Name} vs {army2.Name}";
            
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("СОХРАНЕНИЕ СОСТОЯНИЯ ИГРЫ");
            
            // Сохраняем состояние армий для продолжения игры
            if (battle != null)
            {
                armyManager.SaveArmies(army1, army2, saveName, battle.Round, battle.AttackTurn, battle.FirstAttackerIsArmy1, battle.NeedNewRoundHeader);
            }
            else
            {
                armyManager.SaveArmies(army1, army2, saveName);
            }
            
            // Сохраняем историю битвы с меткой что это незавершенная игра
            string gameStatus = "";
            battleManager.SaveBattleLog(gameStatus, saveName, army1, army2);
            
            ConsoleMenu.ShowSuccess($"Игра сохранена как '{saveName}'!");
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
                    armyManager.SaveArmies(army1, army2, saveName);
                }
                
                // Сохраняем историю битвы (стадия игры)
                string gameStatus = "";
                battleManager.SaveBattleLog(gameStatus, $"{army1.Name} vs {army2.Name}", army1, army2);
                
                ConsoleMenu.ShowSuccess($"Игра сохранена как '{saveName}'!");
                Console.ReadKey();
                return true;
            }
            
            return false;
        }
    }
}