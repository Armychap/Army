using System;
using System.Collections.Generic;
using System.Linq;
using ArmyBattle.Models;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        private void CheckAndExecuteSpecialAbilities()
        {
            if (currentFighter1 == null || currentFighter2 == null || !currentFighter1.IsAlive || !currentFighter2.IsAlive)
                return;

            Console.WriteLine();

            ExecuteSpecialAbilitiesForArmy(army1, army2, typeof(Archer));
            if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
                ExecuteSpecialAbilitiesForArmy(army2, army1, typeof(Archer));

            if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
            {
                ExecuteSpecialAbilitiesForArmy(army1, army2, typeof(Wizard));
                if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
                    ExecuteSpecialAbilitiesForArmy(army2, army1, typeof(Wizard));
            }

            if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
            {
                ExecuteSpecialAbilitiesForArmy(army1, army2, typeof(Healer));
                if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
                    ExecuteSpecialAbilitiesForArmy(army2, army1, typeof(Healer));
            }

            Console.WriteLine();
        }

        private void ExecuteSpecialAbilitiesForArmy(IArmy attackingArmy, IArmy defendingArmy, Type? unitType = null)
        {
            var unitsCopy = attackingArmy.Units.ToList();
            foreach (var unit in unitsCopy)
            {
                if (!unit.IsAlive) continue;

                if (unitType != null && unit.GetRootType() != unitType) continue;

                if (unit == currentFighter1 || unit == currentFighter2) continue;

                if (unit.SpecialAbility == null) continue;

                var realUnit = unit.GetRootUnit();
                bool isHealing = realUnit is Healer;
                bool isCloning = realUnit is Wizard;
                IUnit? target;

                if (realUnit is Archer)
                {
                    int range = random.Next(1, defendingArmy.AliveCount() + 1);
                    var possibleTargets = defendingArmy.AliveFightersInBattleOrder.Where((u, index) => index < range && u.IsAlive).ToList();
                    if (possibleTargets.Count == 0) continue;
                    target = possibleTargets[random.Next(possibleTargets.Count)];
                }
                else if (isCloning)
                {
                    var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && u != unit && !u.Is<Healer>() && !u.Is<StrongFighter>() && !u.Is<Wizard>()).ToList();
                    if (possibleTargets.Count == 0) continue;
                    target = possibleTargets[random.Next(possibleTargets.Count)];
                }
                else if (isHealing)
                {
                    var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && !u.Is<StrongFighter>()).ToList();
                    if (possibleTargets.Count == 0) continue;

                    var filtered = possibleTargets.Where(u => u != unit).ToList();
                    if (filtered.Count > 0)
                    {
                        target = filtered[random.Next(filtered.Count)];
                    }
                    else
                    {
                        target = unit;
                    }
                }
                else
                {
                    target = attackingArmy == army1 ? currentFighter2 : currentFighter1;
                    if (target == null || !target.IsAlive) continue;
                }

                if (unit.CanUseSpecialAbility(target))
                {
                    int healthBefore = (isHealing || isCloning) ? 0 : (target?.Health ?? 0);

                    if (isCloning)
                    {
                        int beforeCount = attackingArmy.Units.Count;
                        unit.UseSpecialAbility(target);
                        if (attackingArmy.Units.Count > beforeCount)
                            IncrementAddedFighters(attackingArmy);
                    }
                    else if (realUnit is Archer)
                    {
                        unit.UseSpecialAbility(target);
                    }

                    ConsoleColor abilityColor = GetAbilityColor(realUnit.GetType());

                    if (!isHealing)
                    {
                        string unitTypeName = realUnit is Archer ? "лучник" : "маг";

                        if (realUnit is Wizard)
                        {
                            if (unit.SpecialAbility is CloneAbility ca && ca.ChosenToClone != null)
                            {
                                Console.ForegroundColor = abilityColor;
                                Console.Write(unitTypeName + " ");
                                Console.ForegroundColor = attackingArmy.Color;
                                Console.Write(unit.GetDisplayName(attackingArmy.Name));
                                Console.ForegroundColor = abilityColor;
                                Console.Write(" клонирует ");
                                Console.ForegroundColor = abilityColor;
                                Console.Write($"({ca.ChosenToClone.PowerLevel}) ");
                                Console.ForegroundColor = attackingArmy.Color;
                                Console.Write(ca.ChosenToClone.GetDisplayName(attackingArmy.Name));
                                Console.ResetColor();
                                Console.WriteLine();
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = abilityColor;
                            Console.Write(unitTypeName + " ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(unit.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" стреляет в ");
                            Console.ForegroundColor = defendingArmy.Color;
                            Console.Write(target?.GetDisplayName(defendingArmy.Name));
                            Console.ResetColor();
                            Console.WriteLine();
                        }

                        if (!(realUnit is Wizard))
                        {
                            int damage = healthBefore - (target?.Health ?? 0);
                            Console.WriteLine($"Урон: {damage}");
                            Console.Write($"Здоровье ");
                            Console.ForegroundColor = defendingArmy.Color;
                            Console.Write(target?.GetDisplayName(defendingArmy.Name));
                            Console.ResetColor();
                            Console.WriteLine($": {Math.Max(0, target?.Health ?? 0)}/{target?.MaxHealth ?? 0}");
                        }

                        if (!target?.IsAlive ?? false)
                        {
                            Console.ForegroundColor = abilityColor;
                            Console.Write(unitTypeName + " ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(unit.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" убивает ");
                            Console.ForegroundColor = defendingArmy.Color;
                            Console.Write(target?.GetDisplayName(defendingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" специальной способностью!");
                            Console.ResetColor();
                            Console.WriteLine();

                            defendingArmy.RemoveDeadFighter(target);
                            if (defendingArmy == army1)
                                currentFighter1 = army1.GetNextFighterInBattleOrder();
                            else
                                currentFighter2 = army2.GetNextFighterInBattleOrder();
                            needNewRoundHeader = true;
                        }
                    }

                    if (isHealing)
                    {
                        unit.UseSpecialAbility(target);
                    }

                    if (isHealing)
                    {
                        if (unit.SpecialAbility is SpecialAbility sa && sa.LastHealed != null)
                        {
                            Console.ForegroundColor = abilityColor;
                            Console.Write("лекарь ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(unit.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" лечит ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(sa.LastHealed.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.ResetColor();
                            Console.WriteLine();

                            Console.Write($"Здоровье ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(sa.LastHealed.GetDisplayName(attackingArmy.Name));
                            Console.ResetColor();
                            Console.WriteLine($": {sa.LastHealed.Health}/{sa.LastHealed.MaxHealth}");
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        public void CheckAndExecuteSpecialAbilitiesForNonAttackers(List<IUnit> fightersWhoAttacked)
        {
            Console.WriteLine();

            var army1UnitsCopy = army1.Units.ToList();
            var army2UnitsCopy = army2.Units.ToList();

            foreach (var unit in army1UnitsCopy)
            {
                if (!unit.IsAlive) continue;
                if (fightersWhoAttacked.Contains(unit)) continue;
                if (unit.SpecialAbility == null) continue;

                var oldF1 = currentFighter1;
                var oldF2 = currentFighter2;
                currentFighter1 = unit;

                ExecuteSpecialAbilitiesForSingleUnit(unit, army1, army2);

                currentFighter1 = oldF1;
                currentFighter2 = oldF2;
            }

            foreach (var unit in army2UnitsCopy)
            {
                if (!unit.IsAlive) continue;
                if (fightersWhoAttacked.Contains(unit)) continue;
                if (unit.SpecialAbility == null) continue;

                var oldF1 = currentFighter1;
                var oldF2 = currentFighter2;
                currentFighter2 = unit;

                ExecuteSpecialAbilitiesForSingleUnit(unit, army2, army1);

                currentFighter1 = oldF1;
                currentFighter2 = oldF2;
            }

            Console.WriteLine();
        }

        private void ExecuteSpecialAbilitiesForSingleUnit(IUnit unit, IArmy attackingArmy, IArmy defendingArmy)
        {
            var realUnit = unit.GetRootUnit();
            bool isHealing = realUnit is Healer;
            bool isCloning = realUnit is Wizard;
            IUnit? target;

            if (realUnit is Archer)
            {
                int range = random.Next(1, defendingArmy.AliveCount() + 1);
                var possibleTargets = defendingArmy.AliveFightersInBattleOrder.Where((u, index) => index < range && u.IsAlive).ToList();
                if (possibleTargets.Count == 0) return;
                target = possibleTargets[random.Next(possibleTargets.Count)];
            }
            else if (isCloning)
            {
                var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && u != unit && !u.Is<Healer>() && !u.Is<StrongFighter>() && !u.Is<Wizard>()).ToList();
                if (possibleTargets.Count == 0) return;
                target = possibleTargets[random.Next(possibleTargets.Count)];
            }
            else if (isHealing)
            {
                var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && !u.Is<StrongFighter>()).ToList();
                if (possibleTargets.Count == 0) return;

                var filtered = possibleTargets.Where(u => u != unit).ToList();
                target = filtered.Count > 0 ? filtered[random.Next(filtered.Count)] : unit;
            }
            else
            {
                target = attackingArmy == army1 ? currentFighter2 : currentFighter1;
                if (target == null || !target.IsAlive) return;
            }

            if (unit.CanUseSpecialAbility(target))
            {
                int healthBefore = (isHealing || isCloning) ? 0 : (target?.Health ?? 0);

                if (isCloning)
                {
                    int beforeCount = attackingArmy.Units.Count;
                    unit.UseSpecialAbility(target);
                    if (attackingArmy.Units.Count > beforeCount)
                        IncrementAddedFighters(attackingArmy);
                }
                else if (realUnit is Archer)
                {
                    unit.UseSpecialAbility(target);
                }

                ConsoleColor abilityColor = GetAbilityColor(realUnit.GetType());

                if (!isHealing)
                {
                    string unitTypeName = realUnit is Archer ? "лучник" : "маг";

                    if (realUnit is Wizard)
                    {
                        if (unit.SpecialAbility is CloneAbility ca && ca.ChosenToClone != null)
                        {
                            Console.ForegroundColor = abilityColor;
                            Console.Write(unitTypeName + " ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(unit.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" клонирует ");
                            Console.ForegroundColor = abilityColor;
                            Console.Write($"({ca.ChosenToClone.PowerLevel}) ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(ca.ChosenToClone.GetDisplayName(attackingArmy.Name));
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = abilityColor;
                        Console.Write(unitTypeName + " ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(unit.GetDisplayName(attackingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.Write(" стреляет в ");
                        Console.ForegroundColor = defendingArmy.Color;
                        Console.Write(target?.GetDisplayName(defendingArmy.Name));
                        Console.ResetColor();
                        Console.WriteLine();
                    }

                    if (!(realUnit is Wizard))
                    {
                        int damage = healthBefore - (target?.Health ?? 0);
                        Console.WriteLine($"Урон: {damage}");
                        Console.Write($"Здоровье ");
                        Console.ForegroundColor = defendingArmy.Color;
                        Console.Write(target?.GetDisplayName(defendingArmy.Name));
                        Console.ResetColor();
                        Console.WriteLine($": {Math.Max(0, target?.Health ?? 0)}/{target?.MaxHealth ?? 0}");
                    }

                    if (!target?.IsAlive ?? false)
                    {
                        Console.ForegroundColor = abilityColor;
                        Console.Write(unitTypeName + " ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(unit.GetDisplayName(attackingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.Write(" убивает ");
                        Console.ForegroundColor = defendingArmy.Color;
                        Console.Write(target?.GetDisplayName(defendingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.Write(" специальной способностью!");
                        Console.ResetColor();
                        Console.WriteLine();

                        defendingArmy.RemoveDeadFighter(target);
                        needNewRoundHeader = true;
                    }
                }

                if (isHealing)
                {
                    unit.UseSpecialAbility(target);
                    if (unit.SpecialAbility is SpecialAbility sa && sa.LastHealed != null)
                    {
                        Console.ForegroundColor = abilityColor;
                        Console.Write("лекарь ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(unit.GetDisplayName(attackingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.Write(" лечит ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(sa.LastHealed.GetDisplayName(attackingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.Write($"Здоровье ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(sa.LastHealed.GetDisplayName(attackingArmy.Name));
                        Console.ResetColor();
                        Console.WriteLine($": {sa.LastHealed.Health}/{sa.LastHealed.MaxHealth}");
                    }
                }

                Console.WriteLine();
            }
        }

        private void CheckSpecialAbilitiesThreeColumns()
        {
            for (int col = 0; col < 3; col++)
            {
                var f1 = currentFightersArmy1[col];
                var f2 = currentFightersArmy2[col];
                if (f1?.IsAlive == true && f2?.IsAlive == true)
                {
                    var oldF1 = currentFighter1; var oldF2 = currentFighter2;
                    currentFighter1 = f1; currentFighter2 = f2;
                    CheckAndExecuteSpecialAbilities();
                    currentFighter1 = oldF1; currentFighter2 = oldF2;
                }
            }
        }

        private ConsoleColor GetAbilityColor(Type unitType)
        {
            if (unitType == typeof(Archer)) return ConsoleColor.Yellow;
            if (unitType == typeof(Wizard)) return ConsoleColor.Magenta;
            if (unitType == typeof(Healer)) return ConsoleColor.Green;
            return ConsoleColor.White;
        }

        private void IncrementAddedFighters(IArmy army)
        {
            if (army == army1)
                Army1AddedFightersCount++;
            else if (army == army2)
                Army2AddedFightersCount++;
        }
    }
}