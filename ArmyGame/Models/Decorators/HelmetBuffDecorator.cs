// HelmetBuffDecorator.cs
namespace ArmyBattle.Models.Decorators
{
    /// <summary>
    /// Бафф "Шлем" - +8 к защите
    /// </summary>
    public class HelmetBuffDecorator : BuffDecorator
    {
        public HelmetBuffDecorator(IUnit unit) : base(unit) { }

        public override int EffectiveDefence => _unit.EffectiveDefence + 8;
        
        public override string GetDisplayName(string prefix)
        {
            return $"{_unit.GetDisplayName(prefix)}";
        }
    }
}