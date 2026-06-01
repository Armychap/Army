using ArmyBattle.Models;

namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для отображения баффов на юнитах
    /// </summary>
    public class DisplayBuffsCommand : ICommand
    {
        /// <summary>
        /// Название команды, отображаемое в меню
        /// </summary>
        public string Name => "Просмотр баффов";
        
        /// <summary>
        /// Можно ли отменить команду. Просмотр не меняет состояние, поэтому false
        /// </summary>
        public bool CanUndo => false;
        
        /// <summary>
        /// Первая армия для отображения её баффов
        /// </summary>
        private readonly IArmy _army1;
        
        /// <summary>
        /// Вторая армия для отображения её баффов
        /// </summary>
        private readonly IArmy _army2;
        
        /// <summary>
        /// Конструктор команды просмотра баффов
        /// </summary>
        public DisplayBuffsCommand(IArmy army1, IArmy army2)
        {
            _army1 = army1;
            _army2 = army2;
        }
        
        /// <summary>
        /// Выполняет команду: выводит список всех баффов у живых юнитов
        /// </summary>
        public void Execute()
        {
            DisplayBuffs(_army1, _army2);
        }
        
        /// <summary>
        /// Отмена команды. Не требуется, так как команда только отображает информацию
        /// </summary>
        public void Undo()
        {
        }
        
        /// <summary>
        /// Отображает баффы для обеих армий
        /// </summary>
        private static void DisplayBuffs(IArmy army1, IArmy army2)
        {
            Console.WriteLine("\nБафы");
            
            // Вывод баффов для первой армии
            Console.WriteLine($"\n{army1.Name}:");
            foreach (var unit in army1.Units)
            {
                if (!unit.IsAlive) continue; // Пропускаем погибших бойцов
                
                Console.Write($"  {unit.GetDisplayName(army1.Name)} - ");
                var buffs = GetBuffNames(unit);
                if (buffs.Count > 0)
                {
                    Console.WriteLine(string.Join(", ", buffs));
                }
                else
                {
                    Console.WriteLine("без баффов");
                }
            }
            
            // Вывод баффов для второй армии
            Console.WriteLine($"\n{army2.Name}:");
            foreach (var unit in army2.Units)
            {
                if (!unit.IsAlive) continue; // Пропускаем погибших бойцов
                
                Console.Write($"  {unit.GetDisplayName(army2.Name)} - ");
                var buffs = GetBuffNames(unit);
                if (buffs.Count > 0)
                {
                    Console.WriteLine(string.Join(", ", buffs));
                }
                else
                {
                    Console.WriteLine("без баффов");
                }
            }
            
            Console.WriteLine();
        }
        
        /// <summary>
        /// Получает список названий всех баффов, навешенных на юнита
        /// </summary>
        private static List<string> GetBuffNames(IUnit unit)
        {
            var buffs = new List<string>();
            var current = unit;
            
            // Проходим по цепочке декораторов, пока не дойдём до базового юнита
            while (current is Models.Decorators.BuffDecorator decorator)
            {
                if (decorator is Models.Decorators.HorseBuffDecorator) buffs.Add("Конь");
                else if (decorator is Models.Decorators.ShieldBuffDecorator) buffs.Add("Щит");
                else if (decorator is Models.Decorators.HelmetBuffDecorator) buffs.Add("Шлем");
                else if (decorator is Models.Decorators.SpearBuffDecorator) buffs.Add("Копьё");
                
                current = decorator.GetInnerUnit(); // Переходим к следующему слою
            }
            
            return buffs;
        }
    }
}