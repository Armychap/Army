using System;
using ArmyBattle.Models;

namespace ArmyBattle.UI
{
    /// <summary>
    /// Интерфейс для отображения боевых действий
    /// </summary>
    public interface IBattleView
    {
        /// <summary>
        /// Отображает начало битвы с基本信息 армий
        /// </summary>
        void DisplayStart(string army1Name, string army2Name, int totalCost);
        
        /// <summary>
        /// Отображает информацию о текущем раунде и сражающихся бойцах
        /// </summary>
        void DisplayRound(int round, IUnit? fighter1, IUnit? fighter2, 
                         IArmy army1, IArmy army2);
        
        /// <summary>
        /// Отображает атаку одного бойца на другого с указанием урона
        /// </summary>
        void DisplayAttack(IUnit attacker, IUnit defender, 
                          IArmy attackingArmy, IArmy defendingArmy, int damage);
        
        /// <summary>
        /// Отображает смерть бойца с указанием убийцы
        /// </summary>
        void DisplayDeath(IUnit killer, IUnit victim, 
                         IArmy killingArmy, IArmy victimArmy);
        
        /// <summary>
        /// Отображает применение специальной способности бойца
        /// </summary>
        void DisplaySpecialAbility(IUnit user, IUnit? target, 
                                  IArmy userArmy, IArmy targetArmy, 
                                  string abilityName);
        
        /// <summary>
        /// Отображает наложение баффа на бойца с указанием изменения характеристик
        /// </summary>
        void DisplayBuff(IUnit unit, string buffName, int attack, int defence);
        
        /// <summary>
        /// Отображает ситуацию патовой ситуации с указанием причины
        /// </summary>
        void DisplayStalemate(string reason);
        
        /// <summary>
        /// Отображает победителя битвы с возможностью указания цвета
        /// </summary>
        void DisplayWinner(string? winnerName, ConsoleColor? color = null);
        
        /// <summary>
        /// Отображает полную статистику прошедшей битвы
        /// </summary>
        void DisplayStatistics(int totalMoves, int army1Survivors, int army2Survivors,
                              int army1Added, int army2Added, int army1Buffs, int army2Buffs);
        
        /// <summary>
        /// Очищает экран консоли
        /// </summary>
        void ClearScreen();
        
        /// <summary>
        /// Ожидает нажатия любой клавиши с возможностью вывода сообщения
        /// </summary>
        void WaitForKey(string message = "Нажмите любую клавишу...");
    }
}