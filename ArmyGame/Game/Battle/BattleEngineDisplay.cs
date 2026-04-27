using System;
using System.Linq;
using System.Collections.Generic;
using ArmyBattle.Models;
using ArmyBattle.Models.Decorators;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        private void DisplayRoundHeader()
        {
            if (_currentStrategy != null)
            {
                _currentStrategy.DisplayRoundHeader(this, round);
            }
            else
            {
                if (currentFormation == FormationType.OneColumn)
                {
                    Console.WriteLine();
                    Console.Write($"РАУНД {round}: ");
                    Console.ForegroundColor = army1.Color;
                    Console.Write($"{army1.Name} {currentFighter1?.FighterNumber}");
                    Console.ResetColor();
                    Console.Write($" ({currentFighter1?.PowerLevel}) vs ");
                    Console.ForegroundColor = army2.Color;
                    Console.Write($"{army2.Name} {currentFighter2?.FighterNumber}");
                    Console.ResetColor();
                    Console.WriteLine($" ({currentFighter2?.PowerLevel})");
                }
                else
                {
                    Console.WriteLine($"\nРАУНД {round} (Три колонны)");
                    for (int col = 0; col < 3; col++)
                    {
                        var f1 = currentFightersArmy1[col];
                        var f2 = currentFightersArmy2[col];
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
        }

        public void DisplayBattleOrder()
        {
            if (_currentStrategy != null)
            {
                _currentStrategy.DisplayBattleOrder(this);
            }
            else
            {
                if (currentFormation == FormationType.OneColumn)
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
                            _ => unit.PowerLevel.Length <= 4 ? unit.PowerLevel.ToLowerInvariant() : unit.PowerLevel.Substring(0, 4).ToLowerInvariant()
                        };
                        return $"{unit.FighterNumber} ({shortType})";
                    }

                    var order1 = string.Join(" -> ", army1.AliveFightersInBattleOrder.Select(FormatUnit));
                    var order2 = string.Join(" -> ", army2.AliveFightersInBattleOrder.Select(FormatUnit));

                    Console.WriteLine($"{army1.Name}: {order1}");
                    Console.WriteLine($"{army2.Name}: {order2}");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine($"Порядок боя {army1.Name} vs {army2.Name}");
                    for (int col = 0; col < 3; col++)
                    {
                        var f1 = currentFightersArmy1[col];
                        var f2 = currentFightersArmy2[col];
                        Console.Write($"Колонна {col + 1}: ");
                        Console.Write(f1 != null ? $"{f1.FighterNumber}({f1.PowerLevel.Substring(0, 3)})" : "Пусто");
                        Console.Write("  vs  ");
                        Console.Write(f2 != null ? $"{f2.FighterNumber}({f2.PowerLevel.Substring(0, 3)})" : "Пусто");
                        Console.WriteLine();
                    }
                    Console.WriteLine($"Резерв {army1.Name}: {string.Join("->", army1BackupQueue.Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})"))}");
                    Console.WriteLine($"Резерв {army2.Name}: {string.Join("<-", army2BackupQueue.Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})"))}");
                    Console.WriteLine();
                }
            }
        }

        private void DisplayHealthInfo()
        {
            Console.WriteLine($"Здоровье {currentFighter1?.FighterNumber}: {Math.Max(0, currentFighter1?.Health ?? 0)}/{currentFighter1?.MaxHealth ?? 0}");
            Console.WriteLine($"Здоровье {currentFighter2?.FighterNumber}: {Math.Max(0, currentFighter2?.Health ?? 0)}/{currentFighter2?.MaxHealth ?? 0}");
            DisplayBuffsOnUnit(currentFighter1, army1);
            DisplayBuffsOnUnit(currentFighter2, army2);
            Console.WriteLine();
        }

        private void DisplayBuffsOnUnit(IUnit? unit, IArmy army)
        {
            if (unit == null || !unit.IsAlive) return;

            var buffNames = new System.Collections.Generic.List<string>();
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
    }
}