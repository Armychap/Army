using ArmyBattle.Models;
using ArmyBattle.UI;

namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для выполнения одного хода в битве
    /// </summary>
    public class MakeMoveCommand : ICommand
    {
        /// <summary>
        /// Название команды, отображаемое в меню
        /// </summary>
        public string Name => "Сделать ход";
        
        /// <summary>
        /// Можно ли отменить команду. Ход можно откатить
        /// </summary>
        public bool CanUndo => true;
        
        /// <summary>
        /// Движок битвы, над которым выполняется ход
        /// </summary>
        private readonly BattleEngine _battle;
        
        /// <summary>
        /// Состояние битвы до выполнения хода
        /// </summary>
        private BattleMemento? _beforeState;
        
        /// <summary>
        /// Состояние битвы после выполнения хода
        /// </summary>
        private BattleMemento? _afterState;
        
        /// <summary>
        /// Флаг, указывающий, был ли ход уже выполнен
        /// </summary>
        private bool _moveExecuted;
        
        /// <summary>
        /// Конструктор команды выполнения хода
        /// </summary>
        public MakeMoveCommand(BattleEngine battle)
        {
            _battle = battle;
        }
        
        /// <summary>
        /// Выполняет ход с сохранением состояния для отмены
        /// </summary>
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
        
        /// <summary>
        /// Отменяет выполненный ход, возвращая битву в состояние до хода
        /// </summary>
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