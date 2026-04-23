// ShieldBuffDecorator.cs
namespace ArmyBattle.Models.Decorators
{
    /// <summary>
    /// Бафф "Щит" - +10 к защите
    /// </summary>
    public class ShieldBuffDecorator : BuffDecorator
    {
        public ShieldBuffDecorator(IUnit unit) : base(unit) { }

        public override int EffectiveDefence => _unit.EffectiveDefence + 10;
        
        public override string GetDisplayName(string prefix)
        {
            return $"{_unit.GetDisplayName(prefix)}";
        }
    }
}