using System;
using ArmyBattle.Models;
using ArmyBattle.Game.Formations;


namespace ArmyBattle.Game
{
    public partial class BattleEngine
    {
        private int round;
        private int moveCount;
        private int attackTurn;
        private bool firstAttackerIsArmy1;
        private bool needNewRoundHeader;
        private bool battleInitialized;
        private IUnit? currentFighter1;
        private IUnit? currentFighter2;
        private bool _needDisplayPair = true;
        private IUnit? _lastDisplayedFighter1;
        private IUnit? _lastDisplayedFighter2;

        public int Round => round;
        public int MoveCount => moveCount;
        public int AttackTurn => attackTurn;
        public bool FirstAttackerIsArmy1 => firstAttackerIsArmy1;
        public bool NeedNewRoundHeader => needNewRoundHeader;

        public bool IsCombatActive
        {
            get
            {
                if (_currentStrategy != null)
                    return _currentStrategy.IsCombatActive(this);

                if (currentFormation == FormationType.OneColumn)
                    return army1.HasAliveUnits() && army2.HasAliveUnits();
                else
                    return HasActiveColumnPair();
            }
        }

        public void SetBattleState(int currentRound, int attackTurn, bool firstAttackerIsArmy1, bool needNewRoundHeader)
        {
            this.round = currentRound;
            this.attackTurn = attackTurn;
            this.firstAttackerIsArmy1 = firstAttackerIsArmy1;
            this.needNewRoundHeader = needNewRoundHeader;
            battleInitialized = true;
        }

        public void SetMoveCount(int count)
        {
            this.moveCount = count;
        }

        public void SetBattleInitialized(bool initialized)
        {
            battleInitialized = initialized;
        }

        public void RestoreFromSave(FormationType formation, int currentRound, int attackTurn, bool firstAttackerIsArmy1, bool needNewRoundHeader, int moveCount)
        {
            this.round = currentRound;
            this.attackTurn = attackTurn;
            this.firstAttackerIsArmy1 = firstAttackerIsArmy1;
            this.needNewRoundHeader = needNewRoundHeader;
            this.moveCount = moveCount;

            currentFormation = formation;
            SetFormationStrategy(formation);

            if (_currentStrategy is OneColumnStrategy)
            {
                SetCurrentFightersForContinuation();
            }
            else if (_currentStrategy is ThreeColumnsStrategy)
            {
                InitializeThreeColumns();
            }
            else if (_currentStrategy is WallStrategy)
            {
                _currentStrategy.Reinitialize(this);
            }

            battleInitialized = true;
            _needDisplayPair = true;
        }

        public IUnit? GetCurrentFighter1() => currentFighter1;
        public IUnit? GetCurrentFighter2() => currentFighter2;
        public void SetCurrentFighter1(IUnit? fighter) => currentFighter1 = fighter;
        public void SetCurrentFighter2(IUnit? fighter) => currentFighter2 = fighter;
        
        public ref IUnit? GetCurrentFighter1Ref() => ref currentFighter1;
        public ref IUnit? GetCurrentFighter2Ref() => ref currentFighter2;
    }
}