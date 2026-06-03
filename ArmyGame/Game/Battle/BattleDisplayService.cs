using System;
using System.Collections.Generic;
using System.Linq;
using ArmyBattle.Models;
using ArmyBattle.Models.Decorators;
using ArmyBattle.Game.Formations;

namespace ArmyBattle.Game
{
    /// <summary>
    /// Сервис отображения информации о битве в консоль.
    /// Отвечает ТОЛЬКО за презентацию информации, не содержит логику битвы.
    /// SRP: единственная ответственность - визуализация состояния боя.
    /// </summary>
    public class BattleDisplayService
    {
        private readonly BattleEngine _battleEngine;

        public BattleDisplayService(BattleEngine battleEngine)
        {
            _battleEngine = battleEngine;
        }

        /// <summary>
        /// Отображает заголовок раунда с учетом текущей формации
        /// </summary>
        public void DisplayRoundHeader(int round, FormationType formationType, IUnit? fighter1, IUnit? fighter2, 
            IUnit?[] fighters1, IUnit?[] fighters2, IArmy army1, IArmy army2)
        {
            if (formationType == FormationType.OneColumn)
            {
                Console.WriteLine();
                Console.Write($"РАУНД {round}: ");
                Console.ForegroundColor = army1.Color;
                Console.Write($"{army1.Name} {fighter1?.FighterNumber}");
                Console.ResetColor();
                Console.Write($" ({fighter1?.PowerLevel}) vs ");
                Console.ForegroundColor = army2.Color;
                Console.Write($"{army2.Name} {fighter2?.FighterNumber}");
                Console.ResetColor();
                Console.WriteLine($" ({fighter2?.PowerLevel})");
            }
            else
            {
                Console.WriteLine($"\nРАУНД {round} (Три колонны)");
                for (int col = 0; col < 3; col++)
                {
                    var f1 = fighters1[col];
                    var f2 = fighters2[col];
                    if (f1 != null && f2 != null && f1.IsAlive && f2.IsAlive)
                    {
                        Console.ForegroundColor = army1.Color;
                        Console.Write($"К{col + 1}: {f1.FighterNumber}({f1.PowerLevel.Substring(0, 3)}) ");
                        Console.ResetColor();
                        Console.Write(" vs ");
                        Console.ForegroundColor = army2.Color;
                        Console.Write($"{f2.FighterNumber}({f2.PowerLevel.Substring(0, 3)})");
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                }
                Console.WriteLine(new string('═', 40) + "\n");
            }
        }

        /// <summary>
        /// Отображает порядок боя (в каком порядке бойцы встают друг против друга)
        /// </summary>
        public void DisplayBattleOrder(FormationType formationType, IUnit?[] fighters1, IUnit?[] fighters2,
            List<IUnit> backup1, List<IUnit> backup2, IArmy army1, IArmy army2)
        {
            if (formationType == FormationType.OneColumn)
            {
                Console.WriteLine("Порядок боя");

                string FormatUnit(IUnit unit)
                {
                    string shortType = unit.PowerLevel.ToLowerInvariant() switch
                    {
                        "слабый" => "слаб",
                        "маг" => "маг",
                        "стена" => "стен",
                        "гуляй город" => "стен",
                        "лучник" => "луч",
                        "лекарь" => "лек",
                        "сильный" => "сил",
                        _ => unit.PowerLevel.Length <= 4 
                            ? unit.PowerLevel.ToLowerInvariant() 
                            : unit.PowerLevel.Substring(0, 4).ToLowerInvariant()
                    };
                    return $"{unit.FighterNumber} ({shortType})";
                }

                var order1 = string.Join(" -> ", army1.AliveFightersInBattleOrder.Select(FormatUnit));
                var order2 = string.Join(" -> ", army2.AliveFightersInBattleOrder.Select(FormatUnit));

                Console.ForegroundColor = army1.Color;
                Console.WriteLine($"{army1.Name}: {order1}");
                Console.ResetColor();
                Console.ForegroundColor = army2.Color;
                Console.WriteLine($"{army2.Name}: {order2}");
                Console.ResetColor();
                Console.WriteLine();
            }
            else
            {
                Console.Write("Порядок боя ");
                Console.ForegroundColor = army1.Color;
                Console.Write($"{army1.Name}");
                Console.ResetColor();
                Console.Write(" vs ");
                Console.ForegroundColor = army2.Color;
                Console.Write($"{army2.Name}");
                Console.ResetColor();
                Console.WriteLine();

                for (int col = 0; col < 3; col++)
                {
                    var f1 = fighters1[col];
                    var f2 = fighters2[col];
                    Console.Write($"Колонна {col + 1}: ");
                    Console.ForegroundColor = army1.Color;
                    Console.Write(f1 != null ? $"{f1.FighterNumber}({f1.PowerLevel.Substring(0, 3)})" : "Пусто");
                    Console.ResetColor();
                    Console.Write("  vs  ");
                    Console.ForegroundColor = army2.Color;
                    Console.Write(f2 != null ? $"{f2.FighterNumber}({f2.PowerLevel.Substring(0, 3)})" : "Пусто");
                    Console.ResetColor();
                    Console.WriteLine();
                }

                Console.Write("Резерв ");
                Console.ForegroundColor = army1.Color;
                Console.Write($"{army1.Name}");
                Console.ResetColor();
                Console.WriteLine($": {string.Join("->", backup1.Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})"))}");

                Console.Write("Резерв ");
                Console.ForegroundColor = army2.Color;
                Console.Write($"{army2.Name}");
                Console.ResetColor();
                Console.WriteLine($": {string.Join("<-", backup2.Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})"))}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Отображает текущее здоровье обоих бойцов и их баффы
        /// </summary>
        public void DisplayHealthInfo(IUnit? fighter1, IUnit? fighter2, IArmy army1, IArmy army2)
        {
            Console.WriteLine($"Здоровье {fighter1?.FighterNumber}: {Math.Max(0, fighter1?.Health ?? 0)}/{fighter1?.MaxHealth ?? 0}");
            Console.WriteLine($"Здоровье {fighter2?.FighterNumber}: {Math.Max(0, fighter2?.Health ?? 0)}/{fighter2?.MaxHealth ?? 0}");
            DisplayBuffsOnUnit(fighter1, army1);
            DisplayBuffsOnUnit(fighter2, army2);
            Console.WriteLine();
        }

        /// <summary>
        /// Отображает все надетые баффы на юните
        /// </summary>
        private void DisplayBuffsOnUnit(IUnit? unit, IArmy army)
        {
            if (unit == null || !unit.IsAlive) return;

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

            if (buffNames.Count > 0)
            {
                Console.WriteLine($"Бафы {unit.FighterNumber}: {string.Join(", ", buffNames)}");
            }
        }

        /// <summary>
        /// Отображает итоговую информацию о завершении битвы
        /// </summary>
        public void DisplayBattleEnd(bool stalemateReached, IArmy army1, IArmy army2, 
            int moveCount, int addedFighters1, int addedFighters2, int buffs1, int buffs2)
        {
            Console.WriteLine();
            Console.WriteLine("БИТВА ЗАВЕРШЕНА");
            Console.WriteLine(new string('=', 40));

            bool army1Wins = army1.HasAliveUnits();
            bool army2Wins = army2.HasAliveUnits();

            if (stalemateReached && army1Wins && army2Wins)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("НИЧЬЯ!");
                Console.ResetColor();
            }
            else if (army1Wins)
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

            Console.WriteLine();
            Console.WriteLine($"Ходов сыграно: {moveCount}");
            Console.WriteLine($"Армия {army1.Name}: добавлено бойцов {addedFighters1}, баффов надето {buffs1}");
            Console.WriteLine($"Армия {army2.Name}: добавлено бойцов {addedFighters2}, баффов надето {buffs2}");
        }
    }
}
