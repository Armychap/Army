// HorseBuffDecorator.cs
using ArmyBattle.Models;

namespace ArmyBattle.Models.Decorators
{
    /// <summary>
    /// Бафф "Конь" - +5 к атаке и защите
    /// </summary>
    public class HorseBuffDecorator : BuffDecorator
    {
        public HorseBuffDecorator(IUnit unit) : base(unit) { }

        public override int EffectiveAttack => _unit.EffectiveAttack + 5;
        public override int EffectiveDefence => _unit.EffectiveDefence + 5;
        
        public override string GetDisplayName(string prefix)
        {
            return $"{_unit.GetDisplayName(prefix)}";
        }
    }
}