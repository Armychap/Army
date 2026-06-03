using System;
using System.Linq;
using System.Collections.Generic;
using ArmyBattle.Models;
using ArmyBattle.Models.Decorators;

namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        /// <summary>
        /// Отображает заголовок раунда (делегирует работу сервису отображения)
        /// </summary>
        private void DisplayRoundHeader()
        {
            if (_currentStrategy != null)
            {
                _currentStrategy.DisplayRoundHeader(this, round);
            }
            else
            {
                _displayService.DisplayRoundHeader(round, currentFormation, currentFighter1, currentFighter2,
                    currentFightersArmy1, currentFightersArmy2, army1, army2);
            }
        }

        /// <summary>
        /// Отображает порядок боя (делегирует работу сервису отображения)
        /// </summary>
        public void DisplayBattleOrder()
        {
            if (_currentStrategy != null)
            {
                _currentStrategy.DisplayBattleOrder(this);
            }
            else
            {
                _displayService.DisplayBattleOrder(currentFormation, currentFightersArmy1, currentFightersArmy2,
                    army1BackupQueue, army2BackupQueue, army1, army2);
            }
        }

        /// <summary>
        /// Отображает текущее здоровье и баффы обоих бойцов (делегирует работу сервису отображения)
        /// </summary>
        private void DisplayHealthInfo()
        {
            _displayService.DisplayHealthInfo(currentFighter1, currentFighter2, army1, army2);
        }
    }
}