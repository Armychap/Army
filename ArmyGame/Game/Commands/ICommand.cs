namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Интерфейс команды для паттерна Command
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Название команды (для отображения в меню Undo/Redo)
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Выполнить команду
        /// </summary>
        void Execute();
        
        /// <summary>
        /// Отменить выполнение команды
        /// </summary>
        void Undo();
        
        /// <summary>
        /// Можно ли отменить эту команду
        /// </summary>
        bool CanUndo { get; }
    }
}