using System;
using ArmyBattle.Models;
using ArmyBattle.Game.Formations;


namespace ArmyBattle.Game
{
    /// <summary>
    /// Управляет состоянием битвы: раунды, ходы, текущие бойцы
    /// </summary>
    public partial class BattleEngine
    {
        // Счётчик раундов
        private int round;
        // Общий счётчик ходов в битве
        private int moveCount;
        // Чей ход: 0 - первый, 1 - второй
        private int attackTurn;
        // Флаг: кто начинает атаку (true = первая армия)
        private bool firstAttackerIsArmy1;
        // Нужен ли новый заголовок раунда
        private bool needNewRoundHeader;
        // Инициализирована ли битва
        private bool battleInitialized;
        // Текущие бойцы в строю (одноколонный режим)
        private IUnit? currentFighter1;
        private IUnit? currentFighter2;
        // Флаг: нужно ли показать новую пару
        private bool _needDisplayPair = true;
        // История показанных пар
        private IUnit? _lastDisplayedFighter1;
        private IUnit? _lastDisplayedFighter2;

        public int Round => round;
        public int MoveCount => moveCount;
        public int AttackTurn => attackTurn;
        public bool FirstAttackerIsArmy1 => firstAttackerIsArmy1;
        public bool NeedNewRoundHeader => needNewRoundHeader;

        /// <summary>
        /// Проверяет, активна ли боевая фаза
        /// </summary>
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

        /// <summary>
        /// Устанавливает состояние битвы из сохранённых данных
        /// </summary>
        public void SetBattleState(int currentRound, int attackTurn, bool firstAttackerIsArmy1, bool needNewRoundHeader)
        {
            this.round = currentRound;
            this.attackTurn = attackTurn;
            this.firstAttackerIsArmy1 = firstAttackerIsArmy1;
            this.needNewRoundHeader = needNewRoundHeader;
            battleInitialized = true;
        }

        /// <summary>
        /// Устанавливает счётчик ходов
        /// </summary>
        public void SetMoveCount(int count)
        {
            this.moveCount = count;
        }

        /// <summary>
        /// Устанавливает флаг инициализации битвы
        /// </summary>
        public void SetBattleInitialized(bool initialized)
        {
            battleInitialized = initialized;
        }

        /// <summary>
        /// Восстанавливает состояние битвы из сохранённых данных
        /// </summary>
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

        /// <summary>
        /// Получить первого текущего бойца
        /// </summary>
        public IUnit? GetCurrentFighter1() => currentFighter1;
        /// <summary>
        /// Получить второго текущего бойца
        /// </summary>
        public IUnit? GetCurrentFighter2() => currentFighter2;
        /// <summary>
        /// Установить первого текущего бойца
        /// </summary>
        public void SetCurrentFighter1(IUnit? fighter) => currentFighter1 = fighter;
        /// <summary>
        /// Установить второго текущего бойца
        /// </summary>
        public void SetCurrentFighter2(IUnit? fighter) => currentFighter2 = fighter;
        
        /// <summary>
        /// Получить ссылку на первого текущего бойца (для изменения по ссылке)
        /// </summary>
        public ref IUnit? GetCurrentFighter1Ref() => ref currentFighter1;
        /// <summary>
        /// Получить ссылку на второго текущего бойца (для изменения по ссылке)
        /// </summary>
        public ref IUnit? GetCurrentFighter2Ref() => ref currentFighter2;
    }
}