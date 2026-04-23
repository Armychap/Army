// SpearBuffDecorator.cs
namespace ArmyBattle.Models.Decorators
{
    /// <summary>
    /// Бафф "Копье" - +10 к атаке
    /// </summary>
    public class SpearBuffDecorator : BuffDecorator
    {
        public SpearBuffDecorator(IUnit unit) : base(unit) { }

        public override int EffectiveAttack => _unit.EffectiveAttack + 10;
        
        public override string GetDisplayName(string prefix)
        {
            return $"{_unit.GetDisplayName(prefix)}";
        }
    }
}