using System;
using ArmyBattle.Models;
using ArmyBattle.Services;
using ArmyBattle.UI;

namespace ArmyBattle
{
    /// <summary>
    /// Управляет результатами битв, сохранениями армий и их просмотром
    /// </summary>
    static class BattleResults
    {
        // Ссылки на сервисы
        private static ArmyManager? armyManager;
        private static BattleManager? battleManager;
        // Последние загруженные армии
        private static IArmy? _lastArmy1;
        private static IArmy? _lastArmy2;

        /// <summary>
        /// Инициализирует ссылки на сервисы и армии для сохранения
        /// </summary>
        public static void Initialize(ArmyManager am, BattleManager bm, ref IArmy? army1, ref IArmy? army2)
        {
            armyManager = am;
            battleManager = bm;
            _lastArmy1 = army1;
            _lastArmy2 = army2;
        }

        // Сохраняет текущие загруженные армии на диск
        /// <summary>
        /// Сохраняет текущие боевые армии с пользовательским названием
        /// </summary>
        public static void SaveCurrentArmies()
        {
            ConsoleMenu.ClearConsole();

            if (_lastArmy1 == null || _lastArmy2 == null)
            {
                ConsoleMenu.ShowError("Сначала создайте или загрузите армии!");
                Console.ReadKey();
                return;
            }

            ConsoleMenu.PrintHeader("СОХРАНЕНИЕ АРМИЙ");
            string? saveName = ConsoleMenu.GetInput("Введите название для сохранения (без пробелов): ");
            
            if (!string.IsNullOrWhiteSpace(saveName))
            {
                armyManager?.SaveArmies(_lastArmy1, _lastArmy2, saveName);
            }
            ConsoleMenu.ShowSuccess($"Армии сохранены!");
            Console.ReadKey();
        }

        // Показывает интерактивное меню для просмотра состава армий из сохраненных битв
        /// <summary>
        /// Показывает информацию о составе армий из сохранённых боёв
        /// </summary>
        public static void ShowStoredArmiesInfo()
        {
            string[] savedBattles = battleManager?.GetSavedBattleArmies() ?? Array.Empty<string>();

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
                    var armyData = battleManager?.LoadBattleArmies(savedBattles[choice - 1]);

                    if (armyData != null)
                    {
                        ConsoleMenu.ClearConsole();
                        ConsoleMenu.PrintHeader($"БИТВА: {savedBattles[choice - 1]}");

                        ConsoleMenu.DisplayArmyComposition(
                            armyData.Army1Name ?? "Армия 1", armyData.Army1Color,
                            armyData.Army1Units ?? new List<UnitSaveData>() , armyData.TotalCost1);

                        ConsoleMenu.DisplayArmyComposition(
                            armyData.Army2Name ?? "Армия 2", armyData.Army2Color,
                            armyData.Army2Units ?? new List<UnitSaveData>(), armyData.TotalCost2);

                        ConsoleMenu.WaitForKey("\nНажмите любую клавишу для возврата к списку...");
                    }
                }
                else if (choice == 0)
                {
                    exitMenu = true;
                }
            }
        }
    }
}