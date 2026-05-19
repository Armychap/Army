namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для отображения состояния битвы (порядка боя)
    /// </summary>
    public class DisplayBattleStateCommand : ICommand
    {
        /// <summary>
        /// Название команды, отображаемое в меню
        /// </summary>
        public string Name => "Просмотр состояния";
        
        /// <summary>
        /// Можно ли отменить команду. Просмотр не меняет состояние, поэтому false
        /// </summary>
        public bool CanUndo => false;
        
        /// <summary>
        /// Движок битвы для получения порядка ходов
        /// </summary>
        private readonly BattleEngine _battle;
        
        /// <summary>
        /// Конструктор команды просмотра состояния
        /// </summary>
        public DisplayBattleStateCommand(BattleEngine battle)
        {
            _battle = battle;
        }
        
        /// <summary>
        /// Выполняет команду: выводит текущий порядок боя
        /// </summary>
        public void Execute()
        {
            _battle.DisplayBattleOrder();
        }
        
        /// <summary>
        /// Отмена команды. Не требуется, так как команда только отображает информацию
        /// </summary>
        public void Undo()
        {
        }
    }
}