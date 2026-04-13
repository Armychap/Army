using System.Collections.Generic;

namespace ShoppingCartWithUndo
{
    // Менеджер команд - отвечает за выполнение команд и управление Undo/Redo.
    // Использует паттерн Command для инкапсуляции операций.
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
        private readonly ILogger _logger;

        public CommandManager(ILogger logger)
        {
            _logger = logger;
        }

        // Выполнить команду и добавить в стек Undo
        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear(); // Очистить Redo при новом действии
            _logger.LogCommand("Выполнено", command);
        }

        // Отменить последнюю команду
        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
                _logger.LogCommand("Отменено", command);
            }
            else
            {
                _logger.Log("Нет действий для отмены");
            }
        }

        // Повторить отмененную команду
        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
                _logger.LogCommand("Повторено", command);
            }
            else
            {
                _logger.Log("Нет действий для повтора");
            }
        }

        public bool CanUndo() => _undoStack.Count > 0;
        public bool CanRedo() => _redoStack.Count > 0;
    }
}