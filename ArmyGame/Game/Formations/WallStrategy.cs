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
        /// <summary>
        /// Название стратегии построения
        /// </summary>
        public string Name => "Стенка";

        /// <summary>
        /// Список пар бойцов, сражающихся друг с другом
        /// </summary>
        private List<(IUnit? attacker, IUnit? defender)> _pairs = new();

        /// <summary>
        /// Флаг необходимости перестроения пар после изменений в составах
        /// </summary>
        private bool _needRebuildPairs = true;

        /// <summary>
        /// Флаги отображения для каждой пары в текущем раунде
        /// </summary>
        private bool[] _pairDisplayed;

        /// <summary>
        /// Список бойцов, которые уже атаковали в текущем ходу
        /// </summary>
        private List<IUnit> _fightersWhoAttacked = new List<IUnit>();

        /// <summary>
        /// Сохранённый порядок бойцов армии 1 для перестроения
        /// </summary>
        private List<IUnit> _savedArmy1 = new();

        /// <summary>
        /// Сохранённый порядок бойцов армии 2 для перестроения
        /// </summary>
        private List<IUnit> _savedArmy2 = new();

        /// <summary>
        /// Инициализирует стратегию: перестраивает пары бойцов
        /// </summary>
        public void Initialize(BattleEngine battle)
        {
            _needRebuildPairs = true;
            RebuildPairs(battle);
        }

        /// <summary>
        /// Проверяет, активна ли битва (есть хотя бы одна пара живых бойцов)
        /// </summary>
        public bool IsCombatActive(BattleEngine battle)
        {
            return battle.GetArmy1().HasAliveUnits() && battle.GetArmy2().HasAliveUnits();
        }

        /// <summary>
        /// Отображает заголовок раунда для стратегии "Стенка"
        /// </summary>
        public void DisplayRoundHeader(BattleEngine battle, int round)
        {
            Console.WriteLine($"\nРАУНД {round} (Стенка)");
        }

        /// <summary>
        /// Отображает текущее состояние всех пар и резервных бойцов
        /// </summary>
        public void DisplayBattleOrder(BattleEngine battle)
        {
            Console.WriteLine($"Порядок боя {battle.GetArmy1().Name} vs {battle.GetArmy2().Name}");
            Console.WriteLine();

            // Показываем все пары
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

            // Показываем бойцов без пары (лишние бойцы, которые не попали в пары)
            var solo1 = _savedArmy1.Skip(_pairs.Count).ToList();
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

        /// <summary>
        /// Обрабатывает один ход битвы в стратегии "Стенка"
        /// </summary>
        /// <returns>Было ли совершено какое-либо действие</returns>
        public bool ProcessMove(BattleEngine battle)
        {
            _fightersWhoAttacked.Clear();
            bool anyAction = false;

            // Перестраиваем пары если нужно
            if (_needRebuildPairs || !AreAllPairsValid())
            {
                RebuildPairs(battle);
                _needRebuildPairs = false;
                _pairDisplayed = new bool[_pairs.Count];
            }

            if (!IsCombatActive(battle))
                return false;

            // Обрабатываем каждую пару
            for (int i = 0; i < _pairs.Count; i++)
            {
                var pair = _pairs[i];

                // Пропускаем если кто-то из пары умер
                if (pair.attacker?.IsAlive != true || pair.defender?.IsAlive != true)
                {
                    _needRebuildPairs = true;
                    continue;
                }

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

                // Случайным образом определяем, кто атакует первым
                bool army1AttacksFirst = battle.GetRandom().Next(2) == 0;

                if (army1AttacksFirst)
                {
                    // Первой атакует армия 1
                    _fightersWhoAttacked.Add(pair.attacker);
                    PerformWallAttack(battle, battle.GetArmy1(), battle.GetArmy2(),
                        ref pair.attacker, ref pair.defender);

                    // Если оба живы - ответная атака армии 2
                    if (pair.attacker?.IsAlive == true && pair.defender?.IsAlive == true)
                    {
                        _fightersWhoAttacked.Add(pair.defender);
                        PerformWallAttack(battle, battle.GetArmy2(), battle.GetArmy1(),
                            ref pair.defender, ref pair.attacker);
                    }
                }
                else
                {
                    // Первой атакует армия 2
                    _fightersWhoAttacked.Add(pair.defender);
                    PerformWallAttack(battle, battle.GetArmy2(), battle.GetArmy1(),
                        ref pair.defender, ref pair.attacker);

                    // Если оба живы - ответная атака армии 1
                    if (pair.attacker?.IsAlive == true && pair.defender?.IsAlive == true)
                    {
                        _fightersWhoAttacked.Add(pair.attacker);
                        PerformWallAttack(battle, battle.GetArmy1(), battle.GetArmy2(),
                            ref pair.attacker, ref pair.defender);
                    }
                }

                _pairs[i] = pair;
            }

            // Проверяем специальные способности у бойцов, которые не атаковали
            if (anyAction)
            {
                battle.CheckAndExecuteSpecialAbilitiesForNonAttackers(_fightersWhoAttacked);
            }

            return anyAction;
        }

        /// <summary>
        /// Выполняет атаку одного бойца на другого в стратегии "Стенка"
        /// </summary>
        private void PerformWallAttack(BattleEngine battle, IArmy attackingArmy, IArmy defendingArmy,
            ref IUnit? attacker, ref IUnit? defender)
        {
            if (attacker == null || defender == null) return;

            // Если у атакующего нулевая атака - пропускает ход
            if (attacker.EffectiveAttack == 0)
            {
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write(attacker.GetDisplayName(attackingArmy.Name));
                Console.ResetColor();
                Console.WriteLine(" пропускает ход (нет атаки)");
                return;
            }

            // Выводим информацию об атаке
            Console.ForegroundColor = attackingArmy.Color;
            Console.Write(attacker.GetDisplayName(attackingArmy.Name));
            Console.ResetColor();
            Console.Write(" бьет ");
            Console.ForegroundColor = defendingArmy.Color;
            Console.Write(defender.GetDisplayName(defendingArmy.Name));
            Console.ResetColor();
            Console.WriteLine();

            // Наносим урон
            int healthBefore = defender.Health;
            attacker.AttackUnit(defender);
            int damage = healthBefore - defender.Health;
            Console.WriteLine($"Урон: {damage}");
            Console.WriteLine($"Здоровье {defender.FighterNumber}: {Math.Max(0, defender.Health)}/{defender.MaxHealth}");

            // Если защитник умер
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

                // Удаляем мёртвого из армии
                defendingArmy.RemoveDeadFighter(defender);

                // Помечаем, что нужно перестроить пары
                _needRebuildPairs = true;
            }
        }

        /// <summary>
        /// Перестраивает пары бойцов на основе текущих живых составов армий
        /// </summary>
        private void RebuildPairs(BattleEngine battle)
        {
            _pairs.Clear();

            // Получаем живых бойцов в правильном порядке
            var alive1 = battle.GetArmy1().AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();
            var alive2 = battle.GetArmy2().AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();

            _savedArmy1 = alive1;
            _savedArmy2 = alive2;

            // Создаём пары, пока хватает бойцов с обеих сторон
            int minCount = Math.Min(alive1.Count, alive2.Count);

            for (int i = 0; i < minCount; i++)
            {
                _pairs.Add((alive1[i], alive2[i]));
            }

            // Если нет ни одной пары, битва закончена
            if (_pairs.Count == 0)
            {
                Console.WriteLine("Нет активных пар - битва завершена!");
            }
        }

        /// <summary>
        /// Проверяет, есть ли хотя бы одна валидная пара живых бойцов
        /// </summary>
        private bool AreAllPairsValid()
        {
            foreach (var pair in _pairs)
            {
                if (pair.attacker?.IsAlive == true && pair.defender?.IsAlive == true)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Переинициализирует стратегию: сбрасывает флаги и перестраивает пары
        /// </summary>
        public void Reinitialize(BattleEngine battle)
        {
            _needRebuildPairs = true;
            _pairDisplayed = null;
            RebuildPairs(battle);
        }
    }
}