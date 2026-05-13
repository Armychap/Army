using System.Collections.Generic;
using ArmyBattle.Game.Formations;

namespace ArmyBattle.Game
{
    /// <summary>
    /// Снимок состояния битвы для паттерна Memento
    /// </summary>
    public class BattleMemento
    {
        // Основные параметры битвы
        public int Round { get; set; }
        public int MoveCount { get; set; }
        public int AttackTurn { get; set; }
        public bool FirstAttackerIsArmy1 { get; set; }
        public bool NeedNewRoundHeader { get; set; }
        public FormationType CurrentFormation { get; set; }
        
        // Снимки состояния всех юнитов
        public List<UnitSnapshot> Army1UnitSnapshots { get; set; } = new();
        public List<UnitSnapshot> Army2UnitSnapshots { get; set; } = new();
        
        // Порядок боя (номера бойцов)
        public List<int> Army1AliveOrder { get; set; } = new();
        public List<int> Army2AliveOrder { get; set; } = new();
        
        // Текущие бойцы в 1-колонном режиме
        public int? CurrentFighter1Number { get; set; }
        public int? CurrentFighter2Number { get; set; }
        
        // Для трёх колонн
        public List<int?> ColumnsArmy1 { get; set; } = new();
        public List<int?> ColumnsArmy2 { get; set; } = new();
        public List<int> BackupQueueArmy1 { get; set; } = new();
        public List<int> BackupQueueArmy2 { get; set; } = new();
        
        // Текущий индекс атаки
        public int AttackTurnValue { get; set; }
        
        // Счётчики стагнации
        public int NoLethalActions { get; set; }
        public int NoHealthChangeCount { get; set; }
    }
    
    /// <summary>
    /// Снимок состояния одного юнита
    /// </summary>
    public class UnitSnapshot
    {
        public int FighterNumber { get; set; }
        public int Health { get; set; }
        public bool IsAlive { get; set; }
        public List<string> AppliedBuffs { get; set; } = new();
        public int Attack { get; set; }
        public int Defence { get; set; }
    }
}