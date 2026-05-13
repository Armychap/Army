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
        
        public MakeMoveCommand(BattleEngine battle)
        {
            _battle = battle;
        }
        
        public void Execute()
        {
            if (!_moveExecuted)
            {
                // ПЕРВЫЙ РАЗ: сохраняем состояние ДО, выполняем ход, сохраняем ПОСЛЕ
                Console.WriteLine(); // Отступ перед выводом хода
                _beforeState = _battle.CreateMemento();
                _battle.DoSingleMove();
                _afterState = _battle.CreateMemento();
                _moveExecuted = true;
            }
            else
            {
                // REDO: заново выполняем ход (чтобы показать его в консоли)
                Console.WriteLine(); // Отступ перед выводом хода
                _battle.RestoreMemento(_beforeState);      // Возвращаемся к состоянию ДО
                _battle.DoSingleMove();                    // Заново выполняем ход (с выводом)
                // _afterState уже есть, не перезаписываем
                
                // Можно также обновить _afterState новым состоянием
                // _afterState = _battle.CreateMemento();
            }
        }
        
        public void Undo()
        {
            if (_beforeState != null && CanUndo)
            {
                _battle.RestoreMemento(_beforeState);
                ConsoleMenu.ShowMessage("Ход отменён");
            }
        }
    }
}