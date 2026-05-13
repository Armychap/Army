using System;
using System.Collections.Generic;
using System.Linq;
using ArmyBattle.Models;
using ArmyBattle.Models.Decorators;
using ArmyBattle.UI;

namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для отображения состояния битвы и баффов
    /// </summary>
    public class DisplayBattleStateAndBuffsCommand : ICommand
    {
        public string Name => "Просмотр состояния и баффов";
        public bool CanUndo => false;

        private readonly BattleEngine _battle;
        private readonly IArmy _army1;
        private readonly IArmy _army2;

        public DisplayBattleStateAndBuffsCommand(BattleEngine battle, IArmy army1, IArmy army2)
        {
            _battle = battle;
            _army1 = army1;
            _army2 = army2;
        }

        public void Execute()
        {
            Console.WriteLine();
            _battle.DisplayBattleOrder();
            Console.WriteLine();
            DisplayBuffs(_army1, _army2);
        }

        public void Undo()
        {
            // Просмотр не требует отмены
        }

        private static void DisplayBuffs(IArmy army1, IArmy army2)
        {
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
                    var buffNames = new List<string>();
                    var current = unit;
                    while (current is BuffDecorator decorator)
                    {
                        string buffName = decorator switch
                        {
                            HorseBuffDecorator => "Конь",
                            ShieldBuffDecorator => "Щит",
                            HelmetBuffDecorator => "Шлем",
                            SpearBuffDecorator => "Копьё",
                            _ => "?"
                        };
                        buffNames.Add(buffName);
                        current = decorator.GetInnerUnit();
                    }

                    Console.WriteLine($"  {unit.GetDisplayName(army.Name)}: {string.Join(", ", buffNames)}");
                }
            }

            Console.WriteLine("Баффы");
            PrintArmyBuffs(army1);
            Console.WriteLine();
            PrintArmyBuffs(army2);
        }
    }
}
