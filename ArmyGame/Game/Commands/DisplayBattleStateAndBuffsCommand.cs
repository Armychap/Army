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
        /// <summary>
        /// Название команды, отображаемое в меню
        /// </summary>
        public string Name => "Просмотр состояния и баффов";
        
        /// <summary>
        /// Можно ли отменить команду. Просмотр не меняет состояние, поэтому false
        /// </summary>
        public bool CanUndo => false;

        /// <summary>
        /// Движок битвы для получения порядка ходов
        /// </summary>
        private readonly BattleEngine _battle;
        
        /// <summary>
        /// Первая армия для отображения её баффов
        /// </summary>
        private readonly IArmy _army1;
        
        /// <summary>
        /// Вторая армия для отображения её баффов
        /// </summary>
        private readonly IArmy _army2;

        /// <summary>
        /// Конструктор команды просмотра состояния
        /// </summary>
        public DisplayBattleStateAndBuffsCommand(BattleEngine battle, IArmy army1, IArmy army2)
        {
            _battle = battle;
            _army1 = army1;
            _army2 = army2;
        }

        /// <summary>
        /// Выполняет команду: выводит порядок ходов и список активных баффов
        /// </summary>
        public void Execute()
        {
            Console.WriteLine();
            _battle.DisplayBattleOrder();
            Console.WriteLine();
            DisplayBuffs(_army1, _army2);
        }

        /// <summary>
        /// Отмена команды. Не требуется, так как команда только отображает информацию
        /// </summary>
        public void Undo()
        {
        }

        /// <summary>
        /// Отображает всех бойцов с баффами для обеих армий
        /// </summary>
        private static void DisplayBuffs(IArmy army1, IArmy army2)
        {
            /// <summary>
            /// Локальная функция для вывода баффов одной армии
            /// </summary>
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