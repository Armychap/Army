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
        /// <summary>
        /// Название стратегии построения
        /// </summary>
        public string Name => "Три колонны";

        /// <summary>
        /// Флаги для отслеживания, какие пары уже были показаны в текущем раунде
        /// </summary>
        private bool[] _pairDisplayed = new bool[3];
        
        /// <summary>
        /// Список бойцов, которые уже атаковали в текущем ходу
        /// </summary>
        private List<IUnit> _fightersWhoAttacked = new List<IUnit>();

        /// <summary>
        /// Инициализирует стратегию: создаёт три колонны и сбрасывает флаги
        /// </summary>
        public void Initialize(BattleEngine battle)
        {
            battle.InitializeThreeColumns();
            // Сбрасываем флаги отображения пар
            for (int i = 0; i < 3; i++)
                _pairDisplayed[i] = false;
        }

        /// <summary>
        /// Проверяет, активна ли битва (есть хотя бы одна активная пара в колоннах)
        /// </summary>
        public bool IsCombatActive(BattleEngine battle)
        {
            return battle.HasActiveColumnPair();
        }

        /// <summary>
        /// Отображает заголовок раунда для стратегии трёх колонн
        /// </summary>
        public void DisplayRoundHeader(BattleEngine battle, int round)
        {
            Console.WriteLine($"\nРАУНД {round} (Три колонны)");
        }

        /// <summary>
        /// Отображает текущее состояние всех трёх колонн и резервов армий
        /// </summary>
        public void DisplayBattleOrder(BattleEngine battle)
        {
            Console.WriteLine($"Порядок боя {battle.GetArmy1().Name} vs {battle.GetArmy2().Name}");
            
            // Выводим каждую колонну
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

        /// <summary>
        /// Обрабатывает один ход битвы в стратегии "Три колонны"
        /// </summary>
        /// <returns>Было ли совершено какое-либо действие</returns>
        public bool ProcessMove(BattleEngine battle)
        {
            _fightersWhoAttacked.Clear();
            bool anyAction = false;

            // Обрабатываем каждую из трёх колонн
            for (int col = 0; col < 3; col++)
            {
                var fighter1 = battle.GetCurrentFighterInColumn(col, true);
                var fighter2 = battle.GetCurrentFighterInColumn(col, false);

                // Пропускаем если нет пары (один из бойцов отсутствует)
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

                // Определяем, кто атакует первым в этой колонне
                bool army1AttacksFirst = (col + battle.AttackTurn) % 2 == 0;

                // Сохраняем ссылки до атаки для сравнения
                var originalFighter1 = fighter1;
                var originalFighter2 = fighter2;

                if (army1AttacksFirst)
                {
                    // Первой атакует армия 1
                    _fightersWhoAttacked.Add(fighter1);
                    battle.PerformAttackInColumnPublic(battle.GetArmy1(), battle.GetArmy2(),
                        ref fighter1, ref fighter2, col);

                    // Если оба живы - ответная атака армии 2
                    if (fighter1?.IsAlive == true && fighter2?.IsAlive == true)
                    {
                        _fightersWhoAttacked.Add(fighter2);
                        battle.PerformAttackInColumnPublic(battle.GetArmy2(), battle.GetArmy1(),
                            ref fighter2, ref fighter1, col);
                    }
                }
                else
                {
                    // Первой атакует армия 2
                    _fightersWhoAttacked.Add(fighter2);
                    battle.PerformAttackInColumnPublic(battle.GetArmy2(), battle.GetArmy1(),
                        ref fighter2, ref fighter1, col);

                    // Если оба живы - ответная атака армии 1
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

                // Если кто-то изменился (умер и заменился), сбрасываем флаг для повторного вывода
                if (fighter1 != originalFighter1 || fighter2 != originalFighter2)
                {
                    _pairDisplayed[col] = false;
                }
            }

            // Проверяем специальные способности у бойцов, которые не атаковали
            if (anyAction)
            {
                battle.CheckAndExecuteSpecialAbilitiesForNonAttackers(_fightersWhoAttacked);
            }

            return anyAction;
        }

        /// <summary>
        /// Переинициализирует стратегию: заново строит колонны и сбрасывает флаги
        /// </summary>
        public void Reinitialize(BattleEngine battle)
        {
            battle.ReinitializeThreeColumns();
            for (int i = 0; i < 3; i++)
                _pairDisplayed[i] = false;
        }
    }
}