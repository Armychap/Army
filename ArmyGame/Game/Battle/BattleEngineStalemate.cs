using System.Collections.Generic;
using ArmyBattle.Models;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        // Счётчик ходов без летальных действий (атак, которые убивают)
        private int noLethalActions = 0;
        private const int maxNoLethalActions = 80;
        // Флаг патовой ситуации
        private bool stalemateReached = false;
        // Счётчик ходов без изменения здоровья
        private int noHealthChangeCount = 0;
        // История здоровья юнитов перед ходом
        private Dictionary<IUnit, int> allUnitsHealthBefore = new();
        private const int maxNoHealthChangeActions = 30;

        public bool StalemateReached => stalemateReached;

        /// <summary>
        /// Проверяет условия для патовой ситуации: стагнация здоровья
        /// </summary>
        private void CheckStalemateAfterMove()
        {
            bool anyHealthChanged = false;
            foreach (var unit in army1.Units)
            {
                if (unit.IsAlive && allUnitsHealthBefore.ContainsKey(unit) && allUnitsHealthBefore[unit] != unit.Health)
                {
                    anyHealthChanged = true;
                    break;
                }
            }
            
            if (!anyHealthChanged)
            {
                foreach (var unit in army2.Units)
                {
                    if (unit.IsAlive && allUnitsHealthBefore.ContainsKey(unit) && allUnitsHealthBefore[unit] != unit.Health)
                    {
                        anyHealthChanged = true;
                        break;
                    }
                }
            }

            if (!anyHealthChanged)
            {
                noHealthChangeCount++;
                if (noHealthChangeCount >= maxNoHealthChangeActions)
                {
                    stalemateReached = true;
                    Console.WriteLine();
                    Console.WriteLine("НИЧЬЯ: Жизнь ни одного бойца не изменялась в течение 10 ходов!");
                }
            }
            else
            {
                noHealthChangeCount = 0;
            }
        }

        /// <summary>
        /// Обнуляет счётчики стагнации при сбросе условий
        /// </summary>
        private void ResetStalemateCounters()
        {
            noLethalActions = 0;
            noHealthChangeCount = 0;
        }
    }
}