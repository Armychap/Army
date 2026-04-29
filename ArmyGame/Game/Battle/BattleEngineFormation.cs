using System;
using System.Collections.Generic;
using ArmyBattle.Models;
using ArmyBattle.Game.Formations;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        private IFormationStrategy? _currentStrategy;
        private IUnit?[] currentFightersArmy1 = new IUnit?[3];
        private IUnit?[] currentFightersArmy2 = new IUnit?[3];

        private List<IUnit> army1BackupQueue = new();
        private List<IUnit> army2BackupQueue = new();

        private IUnit?[] _savedColumnsArmy1 = new IUnit?[3];
        private IUnit?[] _savedColumnsArmy2 = new IUnit?[3];
        private List<IUnit> _savedBackupArmy1 = new();
        private List<IUnit> _savedBackupArmy2 = new();

        public IFormationStrategy? GetCurrentStrategy() => _currentStrategy;

        // Методы доступа
        public IArmy GetArmy1() => army1;
        public IArmy GetArmy2() => army2;
        public Random GetRandom() => random;

        public bool HasActiveColumnPair()
        {
            for (int col = 0; col < 3; col++)
            {
                if (currentFightersArmy1[col]?.IsAlive == true && currentFightersArmy2[col]?.IsAlive == true)
                    return true;
            }
            return false;
        }

        public IUnit? GetCurrentFighterInColumn(int column, bool isArmy1)
        {
            return isArmy1 ? currentFightersArmy1[column] : currentFightersArmy2[column];
        }

        public void UpdateCurrentFighterInColumn(int column, bool isArmy1, IUnit? fighter)
        {
            if (isArmy1)
                currentFightersArmy1[column] = fighter;
            else
                currentFightersArmy2[column] = fighter;
        }

        public List<IUnit> GetArmy1BackupQueue() => army1BackupQueue;
        public List<IUnit> GetArmy2BackupQueue() => army2BackupQueue;

        public List<IUnit> GetSavedFightersForArmy(bool isArmy1)
        {
            var savedColumns = isArmy1 ? _savedColumnsArmy1 : _savedColumnsArmy2;
            var savedBackup = isArmy1 ? _savedBackupArmy1 : _savedBackupArmy2;
            var fighters = savedColumns.Where(u => u?.IsAlive == true).Concat(savedBackup.Where(u => u.IsAlive)).ToList();
            return fighters;
        }

        public void SetNeedRebuildPairs(bool value)
        {
            // Для WallStrategy - будет использоваться через стратегию
        }
    }
}