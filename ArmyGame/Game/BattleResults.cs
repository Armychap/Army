using System;
using ArmyBattle.Models;
using ArmyBattle.Services;
using ArmyBattle.UI;

namespace ArmyBattle
{
    /// <summary>
    /// Класс для отображения результатов битв и управления сохранениями
    /// </summary>
    static class BattleResults
    {
        // Ссылки на сервисы
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

        // Сохраняет текущие загруженные армии на диск
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
            string saveName = ConsoleMenu.GetInput("Введите название для сохранения (без пробелов): ");
            
            armyManager.SaveArmies(_lastArmy1, _lastArmy2, saveName);
            ConsoleMenu.ShowSuccess($"Армии сохранены!");
            Console.ReadKey();
        }

        // Показывает интерактивное меню для просмотра состава армий из сохраненных битв
        public static void ShowStoredArmiesInfo()
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
    }
}