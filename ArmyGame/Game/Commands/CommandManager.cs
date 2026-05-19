using System.Collections.Generic;

namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Менеджер для управления командами с поддержкой Undo/Redo
    /// </summary>
    public class CommandManager
    {
        // Стек выполненных команд для отката (Undo)
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        // Стек отменённых команд для повтора (Redo)
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
            // Выполняем саму команду
            command.Execute();
            // Помещаем выполненную команду в стек отмены
            _undoStack.Push(command);
            // При новой команде стек повтора очищается, так как история действий меняется
            _redoStack.Clear();
        }

        // Отменяет последнюю выполненную команду
        public bool Undo()
        {
            // Нечего отменять
            if (_undoStack.Count == 0) return false;

            var command = _undoStack.Pop();

            // Если команду нельзя отменить, возвращаем её обратно в стек
            if (!command.CanUndo)
            {
                _undoStack.Push(command);
                return false;
            }

            // Выполняем откат команды
            command.Undo();
            // Перемещаем отменённую команду в стек повтора
            _redoStack.Push(command);
            return true;
        }

        // Повторяет последнюю отменённую команду
        public bool Redo()
        {
            // Нечего повторять
            if (_redoStack.Count == 0) return false;

            var command = _redoStack.Pop();
            // Повторно выполняем команду
            command.Execute();
            // Возвращаем команду обратно в стек отмены
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
            // Peek смотрит верхний элемент без извлечения
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