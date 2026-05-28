using System;
using System.Linq;
using ArmyBattle.Models;
using ArmyBattle.Models.Decorators;
using ArmyBattle.Services;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        /// <summary>
        /// Обрабатывает применение случайных буффов к сильным бойцам каждой армии
        /// </summary>
        private void ProcessBuffs()
        {
            var army1StrongFighters = army1.Units
                .Where(u => u.Army != null && u.IsAlive && u != currentFighter1 && u != currentFighter2
                            && !(currentFormation == FormationType.ThreeColumns && (currentFightersArmy1.Contains(u) || currentFightersArmy2.Contains(u)))
                            && IsStrongFighter(u) && CanEquipBuff(u, u.Army!))
                .ToList();

            if (army1StrongFighters.Any())
            {
                var chosen = army1StrongFighters[random.Next(army1StrongFighters.Count)];
                EquipBuff(chosen);
            }

            var army2StrongFighters = army2.Units
                .Where(u => u.Army != null && u.IsAlive && u != currentFighter1 && u != currentFighter2
                            && !(currentFormation == FormationType.ThreeColumns && (currentFightersArmy1.Contains(u) || currentFightersArmy2.Contains(u)))
                            && IsStrongFighter(u) && CanEquipBuff(u, u.Army!))
                .ToList();

            if (army2StrongFighters.Any())
            {
                var chosen = army2StrongFighters[random.Next(army2StrongFighters.Count)];
                EquipBuff(chosen);
            }
        }

        /// <summary>
        /// Проверяет, является ли юнит сильным бойцом (с учётом декораторов)
        /// </summary>
        private bool IsStrongFighter(IUnit unit)
        {
            var realUnit = UnwrapToStrongFighter(unit);
            return realUnit != null;
        }

        /// <summary>
        /// Разворачивает юнита от буффов (декораторов) и проверяет, является ли он сильным
        /// </summary>
        private IUnit? UnwrapToStrongFighter(IUnit unit)
        {
            while (unit is BuffDecorator decorator)
            {
                unit = decorator.GetInnerUnit();
            }
            return unit is StrongFighter ? unit : null;
        }

        /// <summary>
        /// Проверяет, может ли юнит надеть бафф (должен быть рядом со слабым бойцом)
        /// </summary>
        private bool CanEquipBuff(IUnit unit, IArmy army)
        {
            var realUnit = UnwrapToStrongFighter(unit);
            if (realUnit == null) return false;

            int index = army.AliveFightersInBattleOrder.IndexOf(unit);
            if (index == -1) return false;

            if (index > 0 && army.AliveFightersInBattleOrder[index - 1] is WeakFighter wf1 && wf1.IsAlive
                && wf1 != currentFighter1 && wf1 != currentFighter2
                && !(currentFormation == FormationType.ThreeColumns && (currentFightersArmy1.Contains(wf1) || currentFightersArmy2.Contains(wf1))))
                return true;

            if (index < army.AliveFightersInBattleOrder.Count - 1
                && army.AliveFightersInBattleOrder[index + 1] is WeakFighter wf2 && wf2.IsAlive
                && wf2 != currentFighter1 && wf2 != currentFighter2
                && !(currentFormation == FormationType.ThreeColumns && (currentFightersArmy1.Contains(wf2) || currentFightersArmy2.Contains(wf2))))
                return true;

            return false;
        }

        /// <summary>
        /// Надевает случайный бафф на юнита и обновляет его в армии
        /// </summary>
        private void EquipBuff(IUnit unit)
        {
            var source = FindBuffSource(unit, unit.Army!);
            IUnit buffedUnit = BuffFactory.ApplyRandomBuff(unit);
            ReplaceUnitInArmy(unit, buffedUnit);

            if (buffedUnit.Army == army1)
                Army1BuffsAppliedCount++;
            else if (buffedUnit.Army == army2)
                Army2BuffsAppliedCount++;

            var buffName = GetBuffName(buffedUnit);
            if (source != null)
            {
                Console.WriteLine($"{source.GetDisplayName(source.Army?.Name ?? "")} дает бафф {buffName} {buffedUnit.GetDisplayName(buffedUnit.Army?.Name ?? "")}!");
            }
            else
            {
                Console.WriteLine($"{buffedUnit.GetDisplayName(buffedUnit.Army?.Name ?? "")} надевает бафф {buffName}!");
            }
            Console.WriteLine($"Атака {buffedUnit.EffectiveAttack}, Защита {buffedUnit.EffectiveDefence}");

            if (_view != null)
            {
                    _view.DisplayBuff(buffedUnit, buffName, buffedUnit.EffectiveAttack, buffedUnit.EffectiveDefence, source);
            }
        }

        private IUnit? FindBuffSource(IUnit strongUnit, IArmy army)
        {
            int index = army.AliveFightersInBattleOrder.IndexOf(strongUnit);
            if (index == -1) return null;

            if (index > 0 && army.AliveFightersInBattleOrder[index - 1] is WeakFighter wf1 && wf1.IsAlive)
                return wf1;

            if (index < army.AliveFightersInBattleOrder.Count - 1 && army.AliveFightersInBattleOrder[index + 1] is WeakFighter wf2 && wf2.IsAlive)
                return wf2;

            return null;
        }

        private static string GetBuffName(IUnit unit)
        {
            return unit switch
            {
                HorseBuffDecorator => "Конь",
                ShieldBuffDecorator => "Щит",
                HelmetBuffDecorator => "Шлем",
                SpearBuffDecorator => "Копьё",
                _ => "бафф"
            };
        }
    }
}