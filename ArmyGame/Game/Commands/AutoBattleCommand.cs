namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для автоматического прохождения битвы до конца
    /// </summary>
    public class AutoBattleCommand : ICommand
    {
        public string Name => "Автобой до конца";
        public bool CanUndo => false; // Автобой нельзя отменить
        
        private readonly BattleEngine _battle;
        private bool _executed;
        
        public AutoBattleCommand(BattleEngine battle)
        {
            _battle = battle;
        }
        
        public void Execute()
        {
            if (_executed) return;
            
            while (_battle.DoSingleMove())
            {
                System.Threading.Thread.Sleep(400);
            }
            _executed = true;
        }
        
        public void Undo()
        {
            // Nothing - CanUndo = false
        }
    }
}