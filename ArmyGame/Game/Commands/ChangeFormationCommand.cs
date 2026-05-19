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
        
        // Ссылка на движок битвы для выполнения изменений
        private readonly BattleEngine _battle;
        // Новое построение, которое нужно установить
        private readonly FormationType _newFormation;
        // Старое построение для возможности отката
        private FormationType _oldFormation;
        // Флаг, указывающий, выполнялась ли команда ранее
        private bool _executed;
        
        
        // Конструктор без явного указания построения - запрашивает у пользователя
        public ChangeFormationCommand(BattleEngine battle) : this(battle, AskFormationType())
        {
        }
        
        // Основной конструктор с указанием конкретного построения
        public ChangeFormationCommand(BattleEngine battle, FormationType newFormation)
        {
            _battle = battle;
            _newFormation = newFormation;
        }
        
        // Выполнение команды смены построения
        public void Execute()
        {
            // При первом выполнении сохраняем текущее состояние перед изменением
            if (!_executed)
            {
                _oldFormation = _battle.GetCurrentFormation();
                _battle.ReinitializeFormation(_newFormation);
                _executed = true;
                ConsoleMenu.ShowMessage($"Построение изменено на: {GetFormationName(_newFormation)}");
            }
            else
            {
                // При повторном выполнении (Redo) применяем новое построение без сохранения старого
                _battle.ReinitializeFormation(_newFormation);
                ConsoleMenu.ShowMessage($"Построение повторено: {GetFormationName(_newFormation)}");
            }
        }
        
        // Откат команды - возвращает предыдущее построение
        public void Undo()
        {
            if (CanUndo)
            {
                _battle.ReinitializeFormation(_oldFormation);
                ConsoleMenu.ShowMessage($"Построение восстановлено: {GetFormationName(_oldFormation)}");
            }
        }
        
        // Запрашивает у пользователя тип построения через консоль
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
            // При некорректном вводе по умолчанию выбираем одну колонну
            return input switch
            {
                "1" => FormationType.OneColumn,
                "2" => FormationType.ThreeColumns,
                "3" => FormationType.Wall,
                _ => FormationType.OneColumn
            };
        }
        
        // Возвращает строковое представление типа построения для вывода пользователю
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