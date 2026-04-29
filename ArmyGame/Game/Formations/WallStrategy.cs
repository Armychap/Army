// WallStrategy.cs
using System;
using System.Collections.Generic;
using System.Linq;
using ArmyBattle.Models;

namespace ArmyBattle.Game.Formations
{
    /// <summary>
    /// Стратегия "Стенка на стенку" - все бойцы выстраиваются в ряд и сражаются парами
    /// </summary>
    public class WallStrategy : IFormationStrategy
    {
        public string Name => "Стенка";

        private List<(IUnit? attacker, IUnit? defender)> _pairs = new();
        private bool _needRebuildPairs = true;
        private bool[] _pairDisplayed;  // Флаги отображения для каждой пары
        private List<IUnit> _fightersWhoAttacked = new List<IUnit>();
        private List<IUnit> _savedArmy1 = new();
        private List<IUnit> _savedArmy2 = new();

        public void Initialize(BattleEngine battle)
        {
            _needRebuildPairs = true;
            RebuildPairs(battle);
        }

        public bool IsCombatActive(BattleEngine battle)
        {
            bool hasActive = _pairs.Any(p => p.attacker?.IsAlive == true && p.defender?.IsAlive == true);
            return hasActive;
        }

        public void DisplayRoundHeader(BattleEngine battle, int round)
        {
            Console.WriteLine($"\nРАУНД {round} (Стенка)");
        }

        public void DisplayBattleOrder(BattleEngine battle)
        {
            Console.WriteLine($"Порядок боя {battle.GetArmy1().Name} vs {battle.GetArmy2().Name}");
            Console.WriteLine();

            // Показываем пары
            for (int i = 0; i < _pairs.Count; i++)
            {
                var pair = _pairs[i];
                string attackerStr = pair.attacker != null && pair.attacker.IsAlive
                    ? $"{pair.attacker.FighterNumber}({pair.attacker.PowerLevel.Substring(0, 3)})"
                    : "---";
                string defenderStr = pair.defender != null && pair.defender.IsAlive
                    ? $"{pair.defender.FighterNumber}({pair.defender.PowerLevel.Substring(0, 3)})"
                    : "---";

                Console.Write($"{i + 1,2}. ");
                Console.ForegroundColor = battle.GetArmy1().Color;
                Console.Write($"{attackerStr}");
                Console.ResetColor();
                Console.Write("  vs  ");
                Console.ForegroundColor = battle.GetArmy2().Color;
                Console.Write($"{defenderStr}");
                Console.ResetColor();
                Console.WriteLine();
            }

            //  Показываем бойцов без пары (только если они есть)
            var solo1 = _savedArmy1.Skip(_pairs.Count).ToList();  // бойцы, которые не попали в пары (сверх minCount)
            var solo2 = _savedArmy2.Skip(_pairs.Count).ToList();

            if (solo1.Any() || solo2.Any())
            {
                Console.WriteLine("\nРезерв:");
                if (solo1.Any())
                {
                    Console.ForegroundColor = battle.GetArmy1().Color;
                    Console.Write($"{battle.GetArmy1().Name}: ");
                    Console.ResetColor();
                    Console.WriteLine(string.Join(", ", solo1.Select(f => $"{f.FighterNumber}({f.PowerLevel.Substring(0, 3)})")));
                }
                if (solo2.Any())
                {
                    Console.ForegroundColor = battle.GetArmy2().Color;
                    Console.Write($"{battle.GetArmy2().Name}: ");
                    Console.ResetColor();
                    Console.WriteLine(string.Join(", ", solo2.Select(f => $"{f.FighterNumber}({f.PowerLevel.Substring(0, 3)})")));
                }
            }
            Console.WriteLine();
        }

        public bool ProcessMove(BattleEngine battle)
        {
            _fightersWhoAttacked.Clear();
            bool anyAction = false;
            if (_needRebuildPairs || !AreAllPairsValid())
            {
                RebuildPairs(battle);
                _needRebuildPairs = false;
                // Сбрасываем флаги отображения для всех пар
                _pairDisplayed = new bool[_pairs.Count];
            }

            if (!IsCombatActive(battle))
                return false;

            for (int i = 0; i < _pairs.Count; i++)
            {
                var pair = _pairs[i];

                if (pair.attacker?.IsAlive == true && pair.defender?.IsAlive == true)
                {
                    anyAction = true;
                    // Выводим пару если ещё не показывали в этом раунде
                    if (_pairDisplayed == null || !_pairDisplayed[i])
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = battle.GetArmy1().Color;
                        Console.Write($"{pair.attacker.GetDisplayName(battle.GetArmy1().Name)} ({pair.attacker.PowerLevel})");
                        Console.ResetColor();
                        Console.Write(" vs ");
                        Console.ForegroundColor = battle.GetArmy2().Color;
                        Console.Write($"{pair.defender.GetDisplayName(battle.GetArmy2().Name)} ({pair.defender.PowerLevel})");
                        Console.ResetColor();
                        Console.WriteLine();

                        if (_pairDisplayed == null)
                            _pairDisplayed = new bool[_pairs.Count];
                        _pairDisplayed[i] = true;
                    }

                    bool army1AttacksFirst = battle.GetRandom().Next(2) == 0;

                    if (army1AttacksFirst)
                    {
                        _fightersWhoAttacked.Add(pair.attacker);
                        PerformWallAttack(battle, battle.GetArmy1(), battle.GetArmy2(),
                            ref pair.attacker, ref pair.defender);
                        if (pair.attacker?.IsAlive == true && pair.defender?.IsAlive == true)
                        {
                            _fightersWhoAttacked.Add(pair.defender);
                            PerformWallAttack(battle, battle.GetArmy2(), battle.GetArmy1(),
                                ref pair.defender, ref pair.attacker);
                        }
                    }
                    else
                    {
                        _fightersWhoAttacked.Add(pair.defender);
                        PerformWallAttack(battle, battle.GetArmy2(), battle.GetArmy1(),
                            ref pair.defender, ref pair.attacker);
                        if (pair.attacker?.IsAlive == true && pair.defender?.IsAlive == true)
                        {
                            _fightersWhoAttacked.Add(pair.attacker);
                            PerformWallAttack(battle, battle.GetArmy1(), battle.GetArmy2(),
                                ref pair.attacker, ref pair.defender);
                        }
                    }

                    if (pair.attacker?.IsAlive == false || pair.defender?.IsAlive == false)
                    {
                        _needRebuildPairs = true;
                        // Сбрасываем флаг для этой пары
                        if (_pairDisplayed != null)
                            _pairDisplayed[i] = false;
                    }

                    _pairs[i] = pair;
                }
            }

            if (anyAction)
            {
                battle.CheckAndExecuteSpecialAbilitiesForNonAttackers(_fightersWhoAttacked);
            }

            return anyAction;
        }

        private void PerformWallAttack(BattleEngine battle, IArmy attackingArmy, IArmy defendingArmy,
            ref IUnit? attacker, ref IUnit? defender)
        {
            if (attacker == null || defender == null) return;

            if (attacker.EffectiveAttack == 0)
            {
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write(attacker.GetDisplayName(attackingArmy.Name));
                Console.ResetColor();
                Console.WriteLine(" пропускает ход (нет атаки)");
                return;
            }

            Console.ForegroundColor = attackingArmy.Color;
            Console.Write(attacker.GetDisplayName(attackingArmy.Name));
            Console.ResetColor();
            Console.Write(" бьет ");
            Console.ForegroundColor = defendingArmy.Color;
            Console.Write(defender.GetDisplayName(defendingArmy.Name));
            Console.ResetColor();
            Console.WriteLine();

            int healthBefore = defender.Health;
            attacker.AttackUnit(defender);
            int damage = healthBefore - defender.Health;
            Console.WriteLine($"Урон: {damage}");
            Console.WriteLine($"Здоровье {defender.FighterNumber}: {Math.Max(0, defender.Health)}/{defender.MaxHealth}");

            if (!defender.IsAlive)
            {
                Console.WriteLine();
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write(attacker.GetDisplayName(attackingArmy.Name));
                Console.ResetColor();
                Console.Write(" убивает ");
                Console.ForegroundColor = defendingArmy.Color;
                Console.Write(defender.GetDisplayName(defendingArmy.Name));
                Console.ResetColor();
                Console.WriteLine();

                defendingArmy.RemoveDeadFighter(defender);
            }
        }

        private void RebuildPairs(BattleEngine battle)
        {
            _pairs.Clear();

            var alive1 = battle.GetArmy1().AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();
            var alive2 = battle.GetArmy2().AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();

            _savedArmy1 = alive1;
            _savedArmy2 = alive2;

            int minCount = Math.Min(alive1.Count, alive2.Count);

            for (int i = 0; i < minCount; i++)
            {
                _pairs.Add((alive1[i], alive2[i]));
            }
        }

        private bool AreAllPairsValid()
        {
            foreach (var pair in _pairs)
            {
                if (pair.attacker?.IsAlive == true && pair.defender?.IsAlive == true)
                    return true;
            }
            return false;
        }

        public void Reinitialize(BattleEngine battle)
        {
            _needRebuildPairs = true;
            _pairDisplayed = null;
            RebuildPairs(battle);
        }
    }
}