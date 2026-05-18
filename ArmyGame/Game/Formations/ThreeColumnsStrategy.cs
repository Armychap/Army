// ThreeColumnsStrategy.cs
using System;
using System.Linq;
using ArmyBattle.Models;

namespace ArmyBattle.Game.Formations
{
    /// <summary>
    /// Стратегия "Три колонны" - бой в трёх параллельных колоннах
    /// </summary>
    public class ThreeColumnsStrategy : IFormationStrategy
    {
        public string Name => "Три колонны";

        // Флаги для отслеживания, какие пары уже показаны
        private bool[] _pairDisplayed = new bool[3];
        private List<IUnit> _fightersWhoAttacked = new List<IUnit>();

        public void Initialize(BattleEngine battle)
        {
            battle.InitializeThreeColumns();
            // Сбрасываем флаги отображения пар
            for (int i = 0; i < 3; i++)
                _pairDisplayed[i] = false;
        }

        public bool IsCombatActive(BattleEngine battle)
        {
            return battle.HasActiveColumnPair();
        }

        public void DisplayRoundHeader(BattleEngine battle, int round)
        {
            Console.WriteLine($"\nРАУНД {round} (Три колонны)");
        }

        public void DisplayBattleOrder(BattleEngine battle)
        {
            Console.WriteLine($"Порядок боя {battle.GetArmy1().Name} vs {battle.GetArmy2().Name}");
            for (int col = 0; col < 3; col++)
            {
                var f1 = battle.GetCurrentFighterInColumn(col, true);
                var f2 = battle.GetCurrentFighterInColumn(col, false);
                Console.Write($"Колонна {col + 1}: ");

                Console.ForegroundColor = battle.GetArmy1().Color;
                Console.Write(f1 != null ? $"{f1.FighterNumber}({f1.PowerLevel.Substring(0, 3)})" : "Пусто");
                Console.ResetColor();

                Console.Write("  vs  ");
                Console.ForegroundColor = battle.GetArmy2().Color;
                Console.Write(f2 != null ? $"{f2.FighterNumber}({f2.PowerLevel.Substring(0, 3)})" : "Пусто");
                Console.ResetColor();
                Console.WriteLine();
            }
            // Резерв армии 1
            Console.ForegroundColor = battle.GetArmy1().Color;
            Console.Write($"Резерв {battle.GetArmy1().Name}: ");
            Console.ResetColor();
            Console.WriteLine(string.Join("→", battle.GetArmy1BackupQueue().Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})")));

            // Резерв армии 2
            Console.ForegroundColor = battle.GetArmy2().Color;
            Console.Write($"Резерв {battle.GetArmy2().Name}: ");
            Console.ResetColor();
            Console.WriteLine(string.Join("←", battle.GetArmy2BackupQueue().Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})")));

            Console.WriteLine();
        }

        public bool ProcessMove(BattleEngine battle)
        {
            _fightersWhoAttacked.Clear();
            bool anyAction = false;

            for (int col = 0; col < 3; col++)
            {
                var fighter1 = battle.GetCurrentFighterInColumn(col, true);
                var fighter2 = battle.GetCurrentFighterInColumn(col, false);

                // Пропускаем если нет пары
                if (fighter1 == null || fighter2 == null || !fighter1.IsAlive || !fighter2.IsAlive)
                {
                    _pairDisplayed[col] = false;
                    continue;
                }

                // Выводим пару если ещё не показывали в этом раунде
                if (!_pairDisplayed[col])
                {
                    Console.WriteLine();
                    Console.ForegroundColor = battle.GetArmy1().Color;
                    Console.Write($"{fighter1.GetDisplayName(battle.GetArmy1().Name)} ({fighter1.PowerLevel})");
                    Console.ResetColor();
                    Console.Write(" vs ");
                    Console.ForegroundColor = battle.GetArmy2().Color;
                    Console.Write($"{fighter2.GetDisplayName(battle.GetArmy2().Name)} ({fighter2.PowerLevel})");
                    Console.ResetColor();
                    Console.WriteLine();
                    _pairDisplayed[col] = true;
                }

                anyAction = true;

                bool army1AttacksFirst = (col + battle.AttackTurn) % 2 == 0;

                // Сохраняем ссылки до атаки
                var originalFighter1 = fighter1;
                var originalFighter2 = fighter2;

                if (army1AttacksFirst)
                {
                    _fightersWhoAttacked.Add(fighter1);
                    battle.PerformAttackInColumnPublic(battle.GetArmy1(), battle.GetArmy2(),
                        ref fighter1, ref fighter2, col);

                    if (fighter1?.IsAlive == true && fighter2?.IsAlive == true)
                    {
                        _fightersWhoAttacked.Add(fighter2);
                        battle.PerformAttackInColumnPublic(battle.GetArmy2(), battle.GetArmy1(),
                            ref fighter2, ref fighter1, col);
                    }
                }
                else
                {
                    _fightersWhoAttacked.Add(fighter2);
                    battle.PerformAttackInColumnPublic(battle.GetArmy2(), battle.GetArmy1(),
                        ref fighter2, ref fighter1, col);

                    if (fighter1?.IsAlive == true && fighter2?.IsAlive == true)
                    {
                        _fightersWhoAttacked.Add(fighter1);
                        battle.PerformAttackInColumnPublic(battle.GetArmy1(), battle.GetArmy2(),
                            ref fighter1, ref fighter2, col);
                    }
                }

                // Обновляем колонну в движке
                battle.UpdateCurrentFighterInColumn(col, true, fighter1);
                battle.UpdateCurrentFighterInColumn(col, false, fighter2);

                // Если кто-то изменился (умер и заменился), сбрасываем флаг
                if (fighter1 != originalFighter1 || fighter2 != originalFighter2)
                {
                    _pairDisplayed[col] = false;
                }
            }

            if (anyAction)
            {
                battle.CheckAndExecuteSpecialAbilitiesForNonAttackers(_fightersWhoAttacked);
            }

            return anyAction;
        }

        public void Reinitialize(BattleEngine battle)
        {
            battle.ReinitializeThreeColumns();
            for (int i = 0; i < 3; i++)
                _pairDisplayed[i] = false;
        }
    }
}