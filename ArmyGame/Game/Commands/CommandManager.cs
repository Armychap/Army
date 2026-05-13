using System.Collections.Generic;

namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Менеджер для управления командами с поддержкой Undo/Redo
    /// </summary>
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
        
        /// <summary>
        /// Количество доступных для отмены команд
        /// </summary>
        public int UndoCount => _undoStack.Count;
        
        /// <summary>
        /// Количество доступных для повтора команд
        /// </summary>
        public int RedoCount => _redoStack.Count;
        
        /// <summary>
        /// Выполнить команду и добавить её в историю
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }
        
        /// <summary>
        /// Отменить последнюю команду
        /// </summary>
        /// <returns>true если отмена выполнена, false если нечего отменять</returns>
        public bool Undo()
        {
            if (_undoStack.Count == 0) return false;
            
            var command = _undoStack.Pop();
            
            // Проверяем, можно ли отменить команду
            if (!command.CanUndo)
            {
                // Вернуть команду обратно в стек, если отмена невозможна
                _undoStack.Push(command);
                return false;
            }
            
            command.Undo();
            _redoStack.Push(command);
            return true;
        }
        
        /// <summary>
        /// Повторить отменённую команду
        /// </summary>
        /// <returns>true если повтор выполнен, false если нечего повторять</returns>
        public bool Redo()
        {
            if (_redoStack.Count == 0) return false;
            
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            return true;
        }
        
        /// <summary>
        /// Очистить всю историю команд
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
        
        /// <summary>
        /// Получить название последней команды для отмены
        /// </summary>
        public string GetUndoName()
        {
            return _undoStack.Count > 0 ? _undoStack.Peek().Name : "";
        }
        
        /// <summary>
        /// Получить название последней команды для повтора
        /// </summary>
        public string GetRedoName()
        {
            return _redoStack.Count > 0 ? _redoStack.Peek().Name : "";
        }
    }
}