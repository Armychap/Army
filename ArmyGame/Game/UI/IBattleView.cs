using System;
using ArmyBattle.Models;

namespace ArmyBattle.UI
{
    /// <summary>
    /// Интерфейс для отображения боевых действий
    /// </summary>
    public interface IBattleView
    {
        void DisplayStart(string army1Name, string army2Name, int totalCost);
        
        void DisplayRound(int round, IUnit? fighter1, IUnit? fighter2, 
                         IArmy army1, IArmy army2);
        
        void DisplayAttack(IUnit attacker, IUnit defender, 
                          IArmy attackingArmy, IArmy defendingArmy, int damage);
        
        void DisplayDeath(IUnit killer, IUnit victim, 
                         IArmy killingArmy, IArmy victimArmy);
        
        void DisplaySpecialAbility(IUnit user, IUnit? target, 
                                  IArmy userArmy, IArmy targetArmy, 
                                  string abilityName);
        
        void DisplayBuff(IUnit unit, string buffName, int attack, int defence);
        
        void DisplayStalemate(string reason);
        
        void DisplayWinner(string? winnerName, ConsoleColor? color = null);
        
        void DisplayStatistics(int totalMoves, int army1Survivors, int army2Survivors,
                              int army1Added, int army2Added, int army1Buffs, int army2Buffs);
        
        void ClearScreen();
        
        void WaitForKey(string message = "Нажмите любую клавишу...");
    }
}
