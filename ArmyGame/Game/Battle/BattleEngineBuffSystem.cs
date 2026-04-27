using System;
using System.Linq;
using ArmyBattle.Models;
using ArmyBattle.Models.Decorators;
using ArmyBattle.Services;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        private void ProcessBuffs()
        {
            var army1StrongFighters = army1.Units
                .Where(u => u.IsAlive && u != currentFighter1 && u != currentFighter2
                            && IsStrongFighter(u) && CanEquipBuff(u, u.Army))
                .ToList();

            if (army1StrongFighters.Any())
            {
                var chosen = army1StrongFighters[random.Next(army1StrongFighters.Count)];
                EquipBuff(chosen);
            }

            var army2StrongFighters = army2.Units
                .Where(u => u.IsAlive && u != currentFighter1 && u != currentFighter2
                            && IsStrongFighter(u) && CanEquipBuff(u, u.Army))
                .ToList();

            if (army2StrongFighters.Any())
            {
                var chosen = army2StrongFighters[random.Next(army2StrongFighters.Count)];
                EquipBuff(chosen);
            }
        }

        private bool IsStrongFighter(IUnit unit)
        {
            var realUnit = UnwrapToStrongFighter(unit);
            return realUnit != null;
        }

        private IUnit? UnwrapToStrongFighter(IUnit unit)
        {
            while (unit is BuffDecorator decorator)
            {
                unit = decorator.GetInnerUnit();
            }
            return unit is StrongFighter ? unit : null;
        }

        private bool CanEquipBuff(IUnit unit, IArmy army)
        {
            var realUnit = UnwrapToStrongFighter(unit);
            if (realUnit == null) return false;

            int index = army.AliveFightersInBattleOrder.IndexOf(unit);
            if (index == -1) return false;

            if (index > 0 && army.AliveFightersInBattleOrder[index - 1] is WeakFighter wf1 && wf1.IsAlive
                && wf1 != currentFighter1 && wf1 != currentFighter2)
                return true;

            if (index < army.AliveFightersInBattleOrder.Count - 1
                && army.AliveFightersInBattleOrder[index + 1] is WeakFighter wf2 && wf2.IsAlive
                && wf2 != currentFighter1 && wf2 != currentFighter2)
                return true;

            return false;
        }

        private void EquipBuff(IUnit unit)
        {
            IUnit buffedUnit = BuffFactory.ApplyRandomBuff(unit);
            ReplaceUnitInArmy(unit, buffedUnit);

            if (buffedUnit.Army == army1)
                Army1BuffsAppliedCount++;
            else if (buffedUnit.Army == army2)
                Army2BuffsAppliedCount++;

            Console.WriteLine($"{buffedUnit.GetDisplayName(buffedUnit.Army?.Name ?? "")} надевает бафф!");
            Console.WriteLine($"Атака {buffedUnit.EffectiveAttack}, Защита {buffedUnit.EffectiveDefence}");
        }
    }
}