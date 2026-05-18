using System;
using System.Collections.Generic;
using System.Linq;
using ArmyBattle.Models;
using ArmyBattle.Game.Formations;

namespace ArmyBattle.Game
{
    /// <summary>
    /// Отвечает за управление формациями (одна колонна, три колонны, стенка)
    /// </summary>
    public partial class BattleEngine
    {
        // Текущая выбранная стратегия построения
        private IFormationStrategy? _currentStrategy;
        
        // Текущие бойцы в каждой из трёх колонн (для режима трёх колонн)
        private IUnit?[] currentFightersArmy1 = new IUnit?[3];
        private IUnit?[] currentFightersArmy2 = new IUnit?[3];

        // Резервные очереди для замены погибших в колоннах
        private List<IUnit> army1BackupQueue = new();
        private List<IUnit> army2BackupQueue = new();

        // Сохранённые данные для перестроения формации
        private IUnit?[] _savedColumnsArmy1 = new IUnit?[3];
        private IUnit?[] _savedColumnsArmy2 = new IUnit?[3];
        private List<IUnit> _savedBackupArmy1 = new();
        private List<IUnit> _savedBackupArmy2 = new();

        /// <summary>
        /// Получить текущую стратегию
        /// </summary>
        public IFormationStrategy? GetCurrentStrategy() => _currentStrategy;

        /// <summary>
        /// Получить первую армию
        /// </summary>
        public IArmy GetArmy1() => army1;
        
        /// <summary>
        /// Получить вторую армию
        /// </summary>
        public IArmy GetArmy2() => army2;
        
        /// <summary>
        /// Получить генератор случайных чисел
        /// </summary>
        public Random GetRandom() => random;

        /// <summary>
        /// Проверяет, есть ли активная пара в колоннах
        /// </summary>
        public bool HasActiveColumnPair()
        {
            for (int col = 0; col < 3; col++)
            {
                if (currentFightersArmy1[col]?.IsAlive == true && currentFightersArmy2[col]?.IsAlive == true)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Получить бойца в конкретной колонне
        /// </summary>
        public IUnit? GetCurrentFighterInColumn(int column, bool isArmy1)
        {
            return isArmy1 ? currentFightersArmy1[column] : currentFightersArmy2[column];
        }

        /// <summary>
        /// Обновить бойца в конкретной колонне
        /// </summary>
        public void UpdateCurrentFighterInColumn(int column, bool isArmy1, IUnit? fighter)
        {
            if (isArmy1)
                currentFightersArmy1[column] = fighter;
            else
                currentFightersArmy2[column] = fighter;
        }

        /// <summary>
        /// Получить резерв первой армии
        /// </summary>
        public List<IUnit> GetArmy1BackupQueue() => army1BackupQueue;
        
        /// <summary>
        /// Получить резерв второй армии
        /// </summary>
        public List<IUnit> GetArmy2BackupQueue() => army2BackupQueue;

        /// <summary>
        /// Получить сохранённых бойцов для армии
        /// </summary>
        public List<IUnit> GetSavedFightersForArmy(bool isArmy1)
        {
            var savedColumns = isArmy1 ? _savedColumnsArmy1 : _savedColumnsArmy2;
            var savedBackup = isArmy1 ? _savedBackupArmy1 : _savedBackupArmy2;
            var fighters = savedColumns.Where(u => u?.IsAlive == true).Concat(savedBackup.Where(u => u.IsAlive)).ToList();
            return fighters;
        }

        /// <summary>
        /// Маркер необходимости перестройки пар (для стратегии стенки)
        /// </summary>
        public void SetNeedRebuildPairs(bool value)
        {
            // Реализуется через стратегию
        }
    }
}