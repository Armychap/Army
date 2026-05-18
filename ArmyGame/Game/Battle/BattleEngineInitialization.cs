using System;
using System.Linq;
using ArmyBattle.Models;
using ArmyBattle.Game.Formations;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        // Текущая выбранная формация
        private FormationType currentFormation = FormationType.OneColumn;

        /// <summary>
        /// Настраивает битву с выбранной формацией
        /// </summary>
        public void InitializeBattle(FormationType formation = FormationType.OneColumn)
        {
            if (battleInitialized)
                return;

            currentFormation = formation;
            SetFormationStrategy(formation);

            if (_currentStrategy != null)
            {
                _currentStrategy.Initialize(this);
            }

            round = 0;
            moveCount = 0;
            noHealthChangeCount = 0;
            needNewRoundHeader = true;
            attackTurn = 0;
            battleInitialized = true;

            _needDisplayPair = true;
            _lastDisplayedFighter1 = null;
            _lastDisplayedFighter2 = null;
        }

        /// <summary>
        /// Настраивает текущую стратегию на основе типа формации
        /// </summary>
        public void SetFormationStrategy(FormationType type)
        {
            currentFormation = type;
            _currentStrategy = type switch
            {
                FormationType.OneColumn => new OneColumnStrategy(),
                FormationType.ThreeColumns => new ThreeColumnsStrategy(),
                FormationType.Wall => new WallStrategy(),
                _ => new OneColumnStrategy()
            };
        }

        /// <summary>
        /// Настраивает трёхколонный режим боя
        /// </summary>
        public void InitializeThreeColumnBattle()
        {
            currentFormation = FormationType.ThreeColumns;
            army1BackupQueue.Clear();
            army2BackupQueue.Clear();

            for (int i = 0; i < 3; i++)
            {
                currentFightersArmy1[i] = null;
                currentFightersArmy2[i] = null;
            }

            var alive1 = army1.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();
            var alive2 = army2.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();

            int pairsToTake = Math.Min(3, Math.Min(alive1.Count, alive2.Count));

            for (int i = 0; i < pairsToTake; i++)
            {
                currentFightersArmy1[i] = alive1[i];
                currentFightersArmy2[i] = alive2[i];
            }

            // Остальные бойцы идут в резерв
            army1BackupQueue.AddRange(alive1.Skip(pairsToTake));
            army2BackupQueue.AddRange(alive2.Skip(pairsToTake));
        }

        /// <summary>
        /// Переходит на новую формацию, сохраняя текущее состояние боя
        /// </summary>
        public void ReinitializeFormation(FormationType newFormation)
        {
            if (currentFormation == newFormation) return;

            if (currentFormation == FormationType.ThreeColumns)
            {
                for (int i = 0; i < 3; i++)
                {
                    _savedColumnsArmy1[i] = currentFightersArmy1[i];
                    _savedColumnsArmy2[i] = currentFightersArmy2[i];
                }
                _savedBackupArmy1 = new System.Collections.Generic.List<IUnit>(army1BackupQueue);
                _savedBackupArmy2 = new System.Collections.Generic.List<IUnit>(army2BackupQueue);
            }
            else
            {
                _savedColumnsArmy1[0] = currentFighter1;
                _savedColumnsArmy2[0] = currentFighter2;
            }

            currentFormation = newFormation;
            SetFormationStrategy(newFormation);

            if (newFormation == FormationType.ThreeColumns)
            {
                InitializeThreeColumns();
            }
            else if (newFormation == FormationType.OneColumn)
            {
                currentFighter1 = _savedColumnsArmy1[0];
                currentFighter2 = _savedColumnsArmy2[0];

                if (currentFighter1?.IsAlive != true)
                    currentFighter1 = army1.GetNextFighterInBattleOrder();
                if (currentFighter2?.IsAlive != true)
                    currentFighter2 = army2.GetNextFighterInBattleOrder();
            }
            else
            {
                _currentStrategy?.Initialize(this);
            }

            needNewRoundHeader = true;
        }

        /// <summary>
        /// Восстанавливает и переинициализирует трёхколонное построение
        /// </summary>
        public void ReinitializeThreeColumns()
        {
            // Сохраняем текущий порядок колонн и резерва перед перестроением
            for (int i = 0; i < 3; i++)
            {
                _savedColumnsArmy1[i] = currentFightersArmy1[i];
                _savedColumnsArmy2[i] = currentFightersArmy2[i];
            }
            _savedBackupArmy1 = new List<IUnit>(army1BackupQueue);
            _savedBackupArmy2 = new List<IUnit>(army2BackupQueue);

            army1BackupQueue.Clear();
            army2BackupQueue.Clear();

            for (int i = 0; i < 3; i++)
            {
                currentFightersArmy1[i] = null;
                currentFightersArmy2[i] = null;
            }

            var alive1 = army1.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();
            var alive2 = army2.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();

            int pairsToTake = Math.Min(3, Math.Min(alive1.Count, alive2.Count));

            for (int i = 0; i < pairsToTake; i++)
            {
                currentFightersArmy1[i] = alive1[i];
                currentFightersArmy2[i] = alive2[i];
            }

            army1BackupQueue.AddRange(alive1.Skip(pairsToTake));
            army2BackupQueue.AddRange(alive2.Skip(pairsToTake));
        }

        // Метод для восстановления текущих бойцов при продолжении битвы после смены формации
        public void SetCurrentFightersForContinuation()
        {
            currentFighter1 = SelectFighterForArmy(army1);
            currentFighter2 = SelectFighterForArmy(army2);
        }

        /// <summary>
        /// Выбирает текущего бойца для армии в порядке боя
        /// </summary>
        private IUnit? SelectFighterForArmy(IArmy army)
        {
            if (army.CurrentFighterIndex < army.AliveFightersInBattleOrder.Count)
            {
                return army.AliveFightersInBattleOrder[army.CurrentFighterIndex];
            }
            return null;
        }

        // Метод для очистки сохранённых данных колонн и резерва при переходе на другую формацию
        public void ClearSavedColumns()
        {
            for (int i = 0; i < 3; i++)
            {
                _savedColumnsArmy1[i] = null;
                _savedColumnsArmy2[i] = null;
            }
            _savedBackupArmy1.Clear();
            _savedBackupArmy2.Clear();
        }

        // Метод для инициализации трёхколонного режима боя (может быть вызван при выборе формации)
        public void InitializeThreeColumns()
        {
            InitializeThreeColumnBattle();
        }
    }
}