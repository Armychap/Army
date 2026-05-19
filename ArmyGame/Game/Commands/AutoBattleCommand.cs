namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для автоматического прохождения битвы до конца
    /// </summary>
    public class AutoBattleCommand : ICommand
    {
        public string Name => "Автобой до конца";
        public bool CanUndo => false; // Автобой нельзя отменить
        
        // Движок битвы, над которым выполняется автоматическое сражение
        private readonly BattleEngine _battle;
        // Флаг выполнения команды, чтобы не запустить автобой повторно
        private bool _executed;
        
        public AutoBattleCommand(BattleEngine battle)
        {
            _battle = battle;
        }
        
        
        // Запускает автоматическое выполнение ходов до завершения битвы
        public void Execute()
        {
            // Защита от повторного выполнения
            if (_executed) return;
            
            // Выполняем одиночные ходы, пока битва не закончится
            while (_battle.DoSingleMove())
            {
                // Небольшая задержка для визуального восприятия ходов
                System.Threading.Thread.Sleep(400);
            }
            _executed = true;
        }
        
        // Отмена не поддерживается, метод пуст
        public void Undo()
        {
        }
    }
}