using System.Collections.Generic;
using ArmyBattle.Models;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        private int noLethalActions = 0;
        private const int maxNoLethalActions = 80;
        private bool stalemateReached = false;
        private int noHealthChangeCount = 0;
        private Dictionary<IUnit, int> allUnitsHealthBefore = new();
        private const int maxNoHealthChangeActions = 30;

        public bool StalemateReached => stalemateReached;

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

        private void ResetStalemateCounters()
        {
            noLethalActions = 0;
            noHealthChangeCount = 0;
        }
    }
}