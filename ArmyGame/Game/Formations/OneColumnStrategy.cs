// OneColumnStrategy.cs
using System;
using System.Linq;
using ArmyBattle.Models;

namespace ArmyBattle.Game.Formations
{
    /// <summary>
    /// Стратегия "Одна колонна" - классический пошаговый бой 1 на 1
    /// </summary>
    public class OneColumnStrategy : IFormationStrategy
    {
        public string Name => "Одна колонна";

        public void Initialize(BattleEngine battle)
        {
            battle.SetCurrentFighter1(battle.GetArmy1().GetNextFighterInBattleOrder());
            battle.SetCurrentFighter2(battle.GetArmy2().GetNextFighterInBattleOrder());
        }

        public bool IsCombatActive(BattleEngine battle)
        {
            return battle.GetArmy1().HasAliveUnits() && battle.GetArmy2().HasAliveUnits();
        }

        public void DisplayRoundHeader(BattleEngine battle, int round)
        {
            // Ничего не выводим
        }

        public void DisplayBattleOrder(BattleEngine battle)
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

            var order1 = string.Join(" -> ", battle.GetArmy1().AliveFightersInBattleOrder.Select(FormatUnit));
            var order2 = string.Join(" -> ", battle.GetArmy2().AliveFightersInBattleOrder.Select(FormatUnit));

            Console.WriteLine($"{battle.GetArmy1().Name}: {order1}");
            Console.WriteLine($"{battle.GetArmy2().Name}: {order2}");
            Console.WriteLine();

            // Показываем текущую пару
            if (battle.GetCurrentFighter1()?.IsAlive == true && battle.GetCurrentFighter2()?.IsAlive == true)
            {
                Console.WriteLine("Текущая пара:");
                Console.ForegroundColor = battle.GetArmy1().Color;
                Console.Write($"{battle.GetCurrentFighter1()?.GetDisplayName(battle.GetArmy1().Name)} ({battle.GetCurrentFighter1()?.PowerLevel})");
                Console.ResetColor();
                Console.Write(" vs ");
                Console.ForegroundColor = battle.GetArmy2().Color;
                Console.Write($"{battle.GetCurrentFighter2()?.GetDisplayName(battle.GetArmy2().Name)} ({battle.GetCurrentFighter2()?.PowerLevel})");
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public bool ProcessMove(BattleEngine battle)
        {
            bool anyAction = false;

            // Получаем текущих бойцов
            var fighter1 = battle.GetCurrentFighter1();
            var fighter2 = battle.GetCurrentFighter2();

            // Если первый боец мёртв или null - берём следующего
            if (fighter1?.IsAlive != true)
            {
                fighter1 = battle.GetArmy1().GetNextFighterInBattleOrder();
                battle.SetCurrentFighter1(fighter1);
            }

            // Если второй боец мёртв или null - берём следующего
            if (fighter2?.IsAlive != true)
            {
                fighter2 = battle.GetArmy2().GetNextFighterInBattleOrder();
                battle.SetCurrentFighter2(fighter2);
            }

            // Если оба живы - проводим атаку
            if (fighter1?.IsAlive == true && fighter2?.IsAlive == true)
            {
                // Первый удар
                battle.PerformOneColumnAttack(battle.GetArmy1(), battle.GetArmy2(),
                    ref fighter1, ref fighter2);
                anyAction = true;

                // Проверяем, живы ли оба после первого удара
                if (fighter1?.IsAlive == true && fighter2?.IsAlive == true)
                {
                    // Второй удар
                    battle.PerformOneColumnAttack(battle.GetArmy2(), battle.GetArmy1(),
                        ref fighter2, ref fighter1);
                    anyAction = true;
                }

                // Если кто-то умер, берём следующего бойца
                if (fighter1?.IsAlive != true)
                {
                    fighter1 = battle.GetArmy1().GetNextFighterInBattleOrder();
                }

                if (fighter2?.IsAlive != true)
                {
                    fighter2 = battle.GetArmy2().GetNextFighterInBattleOrder();
                }

                // Обновляем текущих бойцов в battle
                battle.SetCurrentFighter1(fighter1);
                battle.SetCurrentFighter2(fighter2);
            }

            return anyAction;
        }

        public void Reinitialize(BattleEngine battle)
        {
            // Для одной колонны не требуется специальная реинициализация
        }
    }
}