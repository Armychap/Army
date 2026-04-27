using System;
using System.Threading;
using ArmyBattle.Models;
using ArmyBattle.Models.Decorators;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        public void DoAllMoves()
        {
            if (!battleInitialized)
            {
                InitializeBattle();
            }

            while (DoSingleMove())
            {
                Thread.Sleep(battleSpeed);
                Thread.Sleep(200);
            }

            EndBattle();
        }

        public bool DoSingleMove()
        {
            if (!battleInitialized)
            {
                InitializeBattle();
                _needDisplayPair = true;
            }

            if (stalemateReached)
            {
                Console.WriteLine("Битва прекращена: патовая ситуация.");
                return false;
            }

            if (currentFormation == FormationType.OneColumn)
            {
                bool alive1 = army1.HasAliveUnits();
                bool alive2 = army2.HasAliveUnits();
                if (!alive1 || !alive2)
                {
                    return false;
                }
            }
            else if (currentFormation == FormationType.ThreeColumns)
            {
                if (!HasActiveColumnPair()) return false;
            }
            else
            {
                if (_currentStrategy != null && !_currentStrategy.IsCombatActive(this)) return false;
            }

            moveCount++;
            Console.WriteLine($"Ход {moveCount}");

            // Логика баффов
            ProcessBuffs();

            // Проверка изменения здоровья
            allUnitsHealthBefore.Clear();
            foreach (var unit in army1.Units)
            {
                if (unit.IsAlive) allUnitsHealthBefore[unit] = unit.Health;
            }
            foreach (var unit in army2.Units)
            {
                if (unit.IsAlive) allUnitsHealthBefore[unit] = unit.Health;
            }

            bool anyAction = false;

            if (_currentStrategy != null)
            {
                anyAction = _currentStrategy.ProcessMove(this);
            }
            else
            {
                if (currentFormation == FormationType.OneColumn)
                {
                    bool currentAttackerIsArmy1 = attackTurn == 0 ? firstAttackerIsArmy1 : !firstAttackerIsArmy1;
                    if (currentAttackerIsArmy1)
                        PerformAttack(army1, army2, ref currentFighter1, ref currentFighter2);
                    else
                        PerformAttack(army2, army1, ref currentFighter2, ref currentFighter1);
                    anyAction = true;
                }
                else
                {
                    anyAction = ProcessThreeColumnMove();
                }
            }

            if (currentFighter1?.IsAlive == true && currentFighter2?.IsAlive == true)
                CheckAndExecuteSpecialAbilities();
            else if (currentFormation == FormationType.ThreeColumns)
                CheckSpecialAbilitiesThreeColumns();

            CheckStalemateAfterMove();

            return anyAction;
        }

        public bool DoSingleRound()
        {
            if (!battleInitialized)
            {
                InitializeBattle();
            }

            if (!(army1.HasAliveUnits() && army2.HasAliveUnits()))
            {
                Console.WriteLine("Битва завершена: одна из армий не имеет живых юнитов");
                return false;
            }

            Console.WriteLine($"Начинаем раунд {round}");

            while (!needNewRoundHeader)
            {
                if (!DoSingleMove())
                    return false;
            }

            needNewRoundHeader = false;

            return true;
        }

        public bool ProcessOneColumnMove()
        {
            bool currentAttackerIsArmy1 = attackTurn == 0 ? firstAttackerIsArmy1 : !firstAttackerIsArmy1;
            if (currentAttackerIsArmy1)
                PerformAttack(army1, army2, ref currentFighter1, ref currentFighter2);
            else
                PerformAttack(army2, army1, ref currentFighter2, ref currentFighter1);
            return true;
        }

        public bool ProcessThreeColumnMove()
        {
            bool anyAction = false;

            for (int col = 0; col < 3; col++)
            {
                var fighter1 = currentFightersArmy1[col];
                var fighter2 = currentFightersArmy2[col];

                if (fighter1 == null || fighter2 == null || !fighter1.IsAlive || !fighter2.IsAlive)
                    continue;

                anyAction = true;

                bool army1AttacksFirst = (col + attackTurn) % 2 == 0;

                if (army1AttacksFirst)
                {
                    PerformAttackInColumn(army1, army2, ref fighter1, ref fighter2, col);
                    if (fighter1?.IsAlive == true && fighter2?.IsAlive == true)
                    {
                        PerformAttackInColumn(army2, army1, ref fighter2, ref fighter1, col);
                    }
                }
                else
                {
                    PerformAttackInColumn(army2, army1, ref fighter2, ref fighter1, col);
                    if (fighter1?.IsAlive == true && fighter2?.IsAlive == true)
                    {
                        PerformAttackInColumn(army1, army2, ref fighter1, ref fighter2, col);
                    }
                }

                currentFightersArmy1[col] = fighter1;
                currentFightersArmy2[col] = fighter2;
            }

            return anyAction;
        }

        private void PerformAttack(IArmy attackingArmy, IArmy defendingArmy, ref IUnit attacker, ref IUnit defender)
        {
            if (attacker.EffectiveAttack == 0)
            {
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write(attacker.GetDisplayName(attackingArmy.Name));
                Console.ResetColor();
                Console.Write(" пропускает ход (нет атаки)");
                Console.WriteLine();

                noLethalActions++;
                if (noLethalActions >= maxNoLethalActions)
                {
                    stalemateReached = true;
                    Console.WriteLine("Патовая ситуация: слишком много ходов без смертей. Битва объявлена ничьей.");
                }

                needNewRoundHeader = false;
                attackTurn = 1 - attackTurn;
                return;
            }

            if (_needDisplayPair && attacker.IsAlive && defender.IsAlive)
            {
                Console.WriteLine();
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write($"{attacker.GetDisplayName(attackingArmy.Name)} ({attacker.PowerLevel})");
                Console.ResetColor();
                Console.Write(" vs ");
                Console.ForegroundColor = defendingArmy.Color;
                Console.Write($"{defender.GetDisplayName(defendingArmy.Name)} ({defender.PowerLevel})");
                Console.ResetColor();
                Console.WriteLine();
                _needDisplayPair = false;
            }

            Console.ForegroundColor = attackingArmy.Color;
            Console.Write(attacker.GetDisplayName(attackingArmy.Name));
            Console.ResetColor();
            Console.Write(" бьет ");
            Console.ForegroundColor = defendingArmy.Color;
            Console.Write($"{defender.GetDisplayName(defendingArmy.Name)}");
            Console.ResetColor();
            Console.WriteLine();

            int healthBefore = defender.Health;
            attacker.AttackUnit(defender);
            int damage = healthBefore - defender.Health;

            Console.WriteLine($"Урон: {damage}");
            DisplayHealthInfo();

            if (defender.IsAlive && TryRemoveOneBuffOnStrongHit(attacker, ref defender))
            {
                Console.WriteLine("У противника сброшен один бафф!");
            }

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

                defendingArmy?.RemoveDeadFighter(defender);

                var oldDefender = defender;
                defender = defendingArmy.GetNextFighterInBattleOrder();

                noLethalActions = 0;
                noHealthChangeCount = 0;

                if (oldDefender != defender)
                {
                    _needDisplayPair = true;

                    if (defender != null && attacker.IsAlive)
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write($"{attacker.GetDisplayName(attackingArmy.Name)} ({attacker.PowerLevel})");
                        Console.ResetColor();
                        Console.Write(" vs ");
                        Console.ForegroundColor = defendingArmy.Color;
                        Console.Write($"{defender.GetDisplayName(defendingArmy.Name)} ({defender.PowerLevel})");
                        Console.ResetColor();
                        Console.WriteLine();
                        _needDisplayPair = false;
                    }
                }
            }
            else
            {
                noLethalActions++;
                if (noLethalActions >= maxNoLethalActions)
                {
                    stalemateReached = true;
                    Console.WriteLine("Патовая ситуация: слишком много ходов без смертей. Битва объявлена ничьей.");
                }
            }

            attackTurn = 1 - attackTurn;
        }

        private void PerformAttackInColumn(IArmy attackingArmy, IArmy defendingArmy,
            ref IUnit? attacker, ref IUnit? defender, int column)
        {
            if (attacker?.EffectiveAttack == 0)
            {
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write(attacker?.GetDisplayName(attackingArmy.Name));
                Console.ResetColor();
                Console.WriteLine(" пропускает ход (нет атаки)");
                noLethalActions++;
                needNewRoundHeader = false;
                attackTurn = 1 - attackTurn;
                return;
            }

            Console.ForegroundColor = attackingArmy.Color;
            Console.Write(attacker?.GetDisplayName(attackingArmy.Name));
            Console.ResetColor();
            Console.Write(" бьет ");
            Console.ForegroundColor = defendingArmy.Color;
            Console.Write(defender?.GetDisplayName(defendingArmy.Name));
            Console.ResetColor();
            Console.WriteLine();

            int healthBefore = defender!.Health;
            attacker.AttackUnit(defender);
            int damage = healthBefore - defender.Health;
            Console.WriteLine($"Урон: {damage}");
            if (defender.IsAlive && TryRemoveOneBuffOnStrongHit(attacker, ref defender))
            {
                Console.WriteLine("У противника сброшен один бафф!");
            }
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

                bool isArmy1Dead = defendingArmy == army1;
                ReplaceDeadFighterInColumn(column, defendingArmy == army1);
                defender = isArmy1Dead ? currentFightersArmy1[column] : currentFightersArmy2[column];

                noLethalActions = 0;
                noHealthChangeCount = 0;
                needNewRoundHeader = true;
            }
            else
            {
                needNewRoundHeader = false;
                noLethalActions++;
            }

            attackTurn = 1 - attackTurn;
        }

        private void ReplaceUnitInArmy(IUnit oldUnit, IUnit newUnit)
        {
            var army = oldUnit.Army;
            if (army == null) return;

            int index = army.Units.IndexOf(oldUnit);
            if (index >= 0)
                army.Units[index] = newUnit;

            int orderIndex = army.AliveFightersInBattleOrder.IndexOf(oldUnit);
            if (orderIndex >= 0)
                army.AliveFightersInBattleOrder[orderIndex] = newUnit;

            if (currentFighter1 == oldUnit) currentFighter1 = newUnit;
            if (currentFighter2 == oldUnit) currentFighter2 = newUnit;
        }

        private bool TryRemoveOneBuffOnStrongHit(IUnit attacker, ref IUnit? defender)
        {
            if (defender == null || !IsStrongFighter(attacker) || !IsStrongFighter(defender))
                return false;

            if (defender is not BuffDecorator buffDecorator)
                return false;

            if (random.Next(2) != 0)
                return false;

            var newDefender = buffDecorator.GetInnerUnit();
            ReplaceUnitInArmy(defender, newDefender);
            defender = newDefender;
            return true;
        }

        private void ReplaceDeadFighterInColumn(int column, bool isArmy1Dead)
        {
            if (isArmy1Dead)
            {
                if (army1BackupQueue.Count > 0)
                {
                    currentFightersArmy1[column] = army1BackupQueue[0];
                    army1BackupQueue.RemoveAt(0);
                }
                else currentFightersArmy1[column] = null;
            }
            else
            {
                if (army2BackupQueue.Count > 0)
                {
                    currentFightersArmy2[column] = army2BackupQueue[0];
                    army2BackupQueue.RemoveAt(0);
                }
                else currentFightersArmy2[column] = null;
            }
        }

        public void PerformOneColumnAttack(IArmy attackingArmy, IArmy defendingArmy,
            ref IUnit attacker, ref IUnit defender)
        {
            PerformAttack(attackingArmy, defendingArmy, ref attacker, ref defender);
        }

        public void PerformAttackInColumnPublic(IArmy attackingArmy, IArmy defendingArmy,
            ref IUnit? attacker, ref IUnit? defender, int column)
        {
            PerformAttackInColumn(attackingArmy, defendingArmy, ref attacker, ref defender, column);
        }
    }
}