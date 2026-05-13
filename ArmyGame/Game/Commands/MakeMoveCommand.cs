using ArmyBattle.Models;
using ArmyBattle.UI;

namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для выполнения одного хода в битве
    /// </summary>
    public class MakeMoveCommand : ICommand
    {
        public string Name => "Сделать ход";
        public bool CanUndo => true;
        
        private readonly BattleEngine _battle;
        private BattleMemento? _beforeState;
        private BattleMemento? _afterState;
        private bool _moveExecuted;
        private bool _moveResult;
        
        public MakeMoveCommand(BattleEngine battle)
        {
            _battle = battle;
        }
        
        public void Execute()
        {
            if (!_moveExecuted)
            {
                // Сохраняем состояние ДО выполнения
                _beforeState = _battle.CreateMemento();
                _moveResult = _battle.DoSingleMove();
                _afterState = _battle.CreateMemento();
                _moveExecuted = true;
            }
            else
            {
                // При Redo восстанавливаем состояние ПОСЛЕ
                if (_afterState != null)
                {
                    _battle.RestoreMemento(_afterState);
                    ConsoleMenu.ShowMessage("Ход повторен");
                }
            }
        }
        
        public void Undo()
        {
            if (_beforeState != null && CanUndo)
            {
                _battle.RestoreMemento(_beforeState);
            }
        }
    }
}