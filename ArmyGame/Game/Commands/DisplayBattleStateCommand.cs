namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для отображения состояния битвы (порядка боя)
    /// </summary>
    public class DisplayBattleStateCommand : ICommand
    {
        public string Name => "Просмотр состояния";
        public bool CanUndo => false; // Просмотр не меняет состояние
        
        private readonly BattleEngine _battle;
        
        public DisplayBattleStateCommand(BattleEngine battle)
        {
            _battle = battle;
        }
        
        public void Execute()
        {
            _battle.DisplayBattleOrder();
        }
        
        public void Undo()
        {
            // Просмотр не требует отмены
        }
    }
}
