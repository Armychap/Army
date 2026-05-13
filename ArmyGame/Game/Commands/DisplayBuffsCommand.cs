using ArmyBattle.Models;

namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для отображения баффов на юнитах
    /// </summary>
    public class DisplayBuffsCommand : ICommand
    {
        public string Name => "Просмотр баффов";
        public bool CanUndo => false; // Просмотр не меняет состояние
        
        private readonly IArmy _army1;
        private readonly IArmy _army2;
        
        public DisplayBuffsCommand(IArmy army1, IArmy army2)
        {
            _army1 = army1;
            _army2 = army2;
        }
        
        public void Execute()
        {
            DisplayBuffs(_army1, _army2);
        }
        
        public void Undo()
        {
            // Просмотр не требует отмены
        }
        
        private static void DisplayBuffs(IArmy army1, IArmy army2)
        {
            Console.WriteLine("\n=== БАФФЫ ===");
            
            // Армия 1
            Console.WriteLine($"\n{army1.Name}:");
            foreach (var unit in army1.Units)
            {
                if (!unit.IsAlive) continue;
                
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
            
            // Армия 2
            Console.WriteLine($"\n{army2.Name}:");
            foreach (var unit in army2.Units)
            {
                if (!unit.IsAlive) continue;
                
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
        
        private static List<string> GetBuffNames(IUnit unit)
        {
            var buffs = new List<string>();
            var current = unit;
            
            while (current is Models.Decorators.BuffDecorator decorator)
            {
                if (decorator is Models.Decorators.HorseBuffDecorator) buffs.Add("Конь");
                else if (decorator is Models.Decorators.ShieldBuffDecorator) buffs.Add("Щит");
                else if (decorator is Models.Decorators.HelmetBuffDecorator) buffs.Add("Шлем");
                else if (decorator is Models.Decorators.SpearBuffDecorator) buffs.Add("Копьё");
                
                current = decorator.GetInnerUnit();
            }
            
            return buffs;
        }
    }
}
