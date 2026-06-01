using ArmyBattle.Models.Interfaces;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Объектный адаптер для Гуляй-город.
    /// 
    /// Этот класс не наследуется от ShieldWall, а оборачивает его внутри.
    /// Он реализует IUnitAdapter и проксирует все вызовы к внутреннему ShieldWall.
    /// Таким образом адаптер может добавляться в те же коллекции IUnit,
    /// но при этом возвращает оригинальный юнит через GetInnerUnit().
    /// </summary>
    public class ShieldWallAdapter : IUnitAdapter
    {
        // Внутренний оригинальный объект ShieldWall, который хранит основную логику
        private readonly ShieldWall _shieldWall;

        public ShieldWallAdapter(int fighterNumber)
        {
            _shieldWall = new ShieldWall(fighterNumber);
            _shieldWall.Name = "Гуляй город";
            _shieldWall.MaxHealth = 100;
            _shieldWall.Health = 100;
            _shieldWall.Cost = 55;
            _shieldWall.PowerLevel = "Гуляй город";
        }

        // Все свойства проксируются на внутренний ShieldWall.
        // Это позволяет использовать адаптер там же, где используется обычный IUnit.
        public string Name { get => _shieldWall.Name; set => _shieldWall.Name = value; }
        public int Attack { get => _shieldWall.Attack; set => _shieldWall.Attack = value; }
        public int Defence { get => _shieldWall.Defence; set => _shieldWall.Defence = value; }
        public int EffectiveAttack => _shieldWall.EffectiveAttack;
        public int EffectiveDefence => _shieldWall.EffectiveDefence;
        public int Health { get => _shieldWall.Health; set => _shieldWall.Health = value; }
        public int MaxHealth { get => _shieldWall.MaxHealth; set => _shieldWall.MaxHealth = value; }
        public int Cost { get => _shieldWall.Cost; set => _shieldWall.Cost = value; }
        public string PowerLevel { get => _shieldWall.PowerLevel; set => _shieldWall.PowerLevel = value; }
        public int DamageDealt { get => _shieldWall.DamageDealt; set => _shieldWall.DamageDealt = value; }
        public int FighterNumber { get => _shieldWall.FighterNumber; set => _shieldWall.FighterNumber = value; }
        public ISpecialAbility? SpecialAbility { get => _shieldWall.SpecialAbility; set => _shieldWall.SpecialAbility = value; }
        public IArmy? Army { get => _shieldWall.Army; set => _shieldWall.Army = value; }
        public bool IsAlive => _shieldWall.IsAlive;
        public List<IUnitObserver> Observers => _shieldWall.Observers;

        /// <summary>
        /// Возвращает внутренний оригинальный ShieldWall.
        /// Это важно для механизма разворачивания прокси в GetRootUnit().
        /// </summary>
        public IUnit GetInnerUnit() => _shieldWall;

        public void AttachObserver(IUnitObserver observer) => _shieldWall.AttachObserver(observer);
        public void DetachObserver(IUnitObserver observer) => _shieldWall.DetachObserver(observer);
        public void ClearObservers() => _shieldWall.ClearObservers();
        public void TakeDamage(int damage, string attackerName) => _shieldWall.TakeDamage(damage, attackerName);
        public void AttackUnit(IUnit target) => _shieldWall.AttackUnit(target);
        public bool CanUseSpecialAbility(IUnit? target) => _shieldWall.CanUseSpecialAbility(target);
        public void UseSpecialAbility(IUnit? target) => _shieldWall.UseSpecialAbility(target);
        public string GetDisplayName(string prefix) => _shieldWall.GetDisplayName(prefix);
        public bool CanBeCloned() => _shieldWall.CanBeCloned();
        public bool CanBeHealed() => _shieldWall.CanBeHealed();
    }
}
