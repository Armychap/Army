using System;
using System.Collections.Generic;
using System.Linq;
using ArmyBattle.Models;
using ArmyBattle.Models.Decorators;
using ArmyBattle.Services;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        /// <summary>
        /// Создать снимок текущего состояния битвы
        /// </summary>
        public BattleMemento CreateMemento()
        {
            var memento = new BattleMemento
            {
                Round = round,
                MoveCount = moveCount,
                AttackTurn = attackTurn,
                FirstAttackerIsArmy1 = firstAttackerIsArmy1,
                NeedNewRoundHeader = needNewRoundHeader,
                CurrentFormation = currentFormation,
                AttackTurnValue = attackTurn,
                NoLethalActions = noLethalActions,
                NoHealthChangeCount = noHealthChangeCount,
                
                // Снимки юнитов
                Army1UnitSnapshots = CreateUnitSnapshots(army1),
                Army2UnitSnapshots = CreateUnitSnapshots(army2),
                
                // Порядок боя
                Army1AliveOrder = army1.AliveFightersInBattleOrder.Select(u => u.FighterNumber).ToList(),
                Army2AliveOrder = army2.AliveFightersInBattleOrder.Select(u => u.FighterNumber).ToList(),
                
                // Текущие бойцы
                CurrentFighter1Number = currentFighter1?.FighterNumber,
                CurrentFighter2Number = currentFighter2?.FighterNumber,
            };
            
            // Для трёх колонн
            for (int i = 0; i < 3; i++)
            {
                memento.ColumnsArmy1.Add(currentFightersArmy1[i]?.FighterNumber);
                memento.ColumnsArmy2.Add(currentFightersArmy2[i]?.FighterNumber);
            }
            memento.BackupQueueArmy1 = army1BackupQueue.Select(u => u.FighterNumber).ToList();
            memento.BackupQueueArmy2 = army2BackupQueue.Select(u => u.FighterNumber).ToList();
            
            return memento;
        }
        
        /// <summary>
        /// Восстановить состояние битвы из снимка
        /// </summary>
        public void RestoreMemento(BattleMemento memento)
        {
            if (memento == null) return;
            
            // Восстанавливаем основные параметры
            round = memento.Round;
            moveCount = memento.MoveCount;
            attackTurn = memento.AttackTurn;
            firstAttackerIsArmy1 = memento.FirstAttackerIsArmy1;
            needNewRoundHeader = memento.NeedNewRoundHeader;
            currentFormation = memento.CurrentFormation;
            noLethalActions = memento.NoLethalActions;
            noHealthChangeCount = memento.NoHealthChangeCount;
            
            // Восстанавливаем состояние юнитов
            RestoreUnitSnapshots(army1, memento.Army1UnitSnapshots);
            RestoreUnitSnapshots(army2, memento.Army2UnitSnapshots);
            
            // Восстанавливаем порядок боя
            RestoreAliveOrder(army1, memento.Army1AliveOrder);
            RestoreAliveOrder(army2, memento.Army2AliveOrder);
            
            // Восстанавливаем текущих бойцов
            currentFighter1 = FindUnitByNumber(army1, memento.CurrentFighter1Number);
            currentFighter2 = FindUnitByNumber(army2, memento.CurrentFighter2Number);
            
            // Восстанавливаем трёхколонное состояние
            for (int i = 0; i < 3 && i < memento.ColumnsArmy1.Count; i++)
            {
                currentFightersArmy1[i] = FindUnitByNumber(army1, memento.ColumnsArmy1[i]);
                currentFightersArmy2[i] = FindUnitByNumber(army2, memento.ColumnsArmy2[i]);
            }
            
            // Восстанавливаем резервные очереди
            army1BackupQueue.Clear();
            foreach (var num in memento.BackupQueueArmy1)
            {
                var unit = FindUnitByNumber(army1, num);
                if (unit != null) army1BackupQueue.Add(unit);
            }
            
            army2BackupQueue.Clear();
            foreach (var num in memento.BackupQueueArmy2)
            {
                var unit = FindUnitByNumber(army2, num);
                if (unit != null) army2BackupQueue.Add(unit);
            }
            
            // Восстанавливаем стратегию построения
            SetFormationStrategy(currentFormation);
            if (_currentStrategy != null && currentFormation != FormationType.OneColumn)
            {
                _currentStrategy.Reinitialize(this);
            }
            
            battleInitialized = true;
            _needDisplayPair = true;
        }
        
        /// <summary>
        /// Получить текущий тип построения
        /// </summary>
        public FormationType GetCurrentFormation()
        {
            return currentFormation;
        }
        
        private List<UnitSnapshot> CreateUnitSnapshots(IArmy army)
        {
            var snapshots = new List<UnitSnapshot>();
            foreach (var unit in army.Units)
            {
                snapshots.Add(new UnitSnapshot
                {
                    FighterNumber = unit.FighterNumber,
                    Health = unit.Health,
                    IsAlive = unit.IsAlive,
                    Attack = unit.Attack,
                    Defence = unit.Defence,
                    AppliedBuffs = GetAppliedBuffTypes(unit)
                });
            }
            return snapshots;
        }
        
        private void RestoreUnitSnapshots(IArmy army, List<UnitSnapshot> snapshots)
        {
            foreach (var snapshot in snapshots)
            {
                var unit = army.Units.FirstOrDefault(u => u.FighterNumber == snapshot.FighterNumber);
                if (unit != null)
                {
                    unit.Health = snapshot.Health;
                    unit.Attack = snapshot.Attack;
                    unit.Defence = snapshot.Defence;
                    
                    // Восстанавливаем баффы
                    IUnit restoredUnit = unit;
                    foreach (var buffType in snapshot.AppliedBuffs)
                    {
                        restoredUnit = BuffFactory.ApplyBuff(restoredUnit, buffType);
                    }
                    
                    // Если баффы были применены, заменяем юнита
                    if (restoredUnit != unit)
                    {
                        ReplaceUnitInArmy(unit, restoredUnit);  // Используем существующий метод из BattleEngineMoves.cs
                    }
                }
            }
            army.RefreshAliveFighters();
        }
        
        private void RestoreAliveOrder(IArmy army, List<int> order)
        {
            var newOrder = new List<IUnit>();
            foreach (var num in order)
            {
                var unit = army.Units.FirstOrDefault(u => u.FighterNumber == num && u.IsAlive);
                if (unit != null) newOrder.Add(unit);
            }
            army.AliveFightersInBattleOrder = newOrder;
            army.CurrentFighterIndex = 0;
        }
        
        private IUnit? FindUnitByNumber(IArmy army, int? fighterNumber)
        {
            if (fighterNumber == null) return null;
            return army.Units.FirstOrDefault(u => u.FighterNumber == fighterNumber && u.IsAlive);
        }
        
        private List<string> GetAppliedBuffTypes(IUnit unit)
        {
            var buffs = new List<string>();
            var current = unit;
            while (current is BuffDecorator decorator)
            {
                if (decorator is HorseBuffDecorator) buffs.Add("Horse");
                else if (decorator is ShieldBuffDecorator) buffs.Add("Shield");
                else if (decorator is HelmetBuffDecorator) buffs.Add("Helmet");
                else if (decorator is SpearBuffDecorator) buffs.Add("Spear");
                current = decorator.GetInnerUnit();
            }
            return buffs;
        }
    }
}