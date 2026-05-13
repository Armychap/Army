using ArmyBattle.Game.Formations;
using ArmyBattle.UI;

namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для смены боевого построения
    /// </summary>
    public class ChangeFormationCommand : ICommand
    {
        public string Name => "Сменить построение";
        public bool CanUndo => true;
        
        private readonly BattleEngine _battle;
        private readonly FormationType _newFormation;
        private FormationType _oldFormation;
        private bool _executed;
        
        public ChangeFormationCommand(BattleEngine battle) : this(battle, AskFormationType())
        {
        }
        
        public ChangeFormationCommand(BattleEngine battle, FormationType newFormation)
        {
            _battle = battle;
            _newFormation = newFormation;
        }
        
        public void Execute()
        {
            if (!_executed)
            {
                _oldFormation = _battle.GetCurrentFormation();
                _battle.ReinitializeFormation(_newFormation);
                _executed = true;
                ConsoleMenu.ShowMessage($"Построение изменено на: {GetFormationName(_newFormation)}");
            }
            else
            {
                // При Redo - применяем новое построение
                _battle.ReinitializeFormation(_newFormation);
                ConsoleMenu.ShowMessage($"Построение повторено: {GetFormationName(_newFormation)}");
            }
        }
        
        public void Undo()
        {
            if (CanUndo)
            {
                _battle.ReinitializeFormation(_oldFormation);
                ConsoleMenu.ShowMessage($"Построение восстановлено: {GetFormationName(_oldFormation)}");
            }
        }
        
        private static FormationType AskFormationType()
        {
            ConsoleMenu.ClearConsole();
            ConsoleMenu.PrintHeader("СМЕНА БОЕВОГО ПОСТРОЕНИЯ");
            
            Console.WriteLine("Выберите тип построения:");
            Console.WriteLine("1. Одна колонна");
            Console.WriteLine("2. Три колонны");
            Console.WriteLine("3. Стенка");
            Console.Write("Ваш выбор: ");
            
            string? input = Console.ReadLine();
            return input switch
            {
                "1" => FormationType.OneColumn,
                "2" => FormationType.ThreeColumns,
                "3" => FormationType.Wall,
                _ => FormationType.OneColumn
            };
        }
        
        private static string GetFormationName(FormationType formation)
        {
            return formation switch
            {
                FormationType.OneColumn => "Одна колонна",
                FormationType.ThreeColumns => "Три колонны",
                FormationType.Wall => "Стенка",
                _ => "Неизвестно"
            };
        }
    }
}