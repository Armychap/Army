// BuffDecorator.cs
using ArmyBattle.Models;
using ArmyBattle.Models.Interfaces;

namespace ArmyBattle.Models.Decorators
{
    /// <summary>
    /// Базовый абстрактный класс для всех баффов-декораторов
    /// </summary>
    public abstract class BuffDecorator : IUnit
    {
        protected IUnit _unit;

        protected BuffDecorator(IUnit unit)
        {
            _unit = unit;
            
            // Проксируем армию и номер бойца
            Army = unit.Army;
            FighterNumber = unit.FighterNumber;
            Name = unit.Name;
        }

        // Проксируем все свойства
        public virtual string Name { get; set; }
        public virtual int Attack { get => _unit.Attack; set => _unit.Attack = value; }
        public virtual int Defence { get => _unit.Defence; set => _unit.Defence = value; }
        public virtual int EffectiveAttack => _unit.EffectiveAttack;
        public virtual int EffectiveDefence => _unit.EffectiveDefence;
        public virtual int Health { get => _unit.Health; set => _unit.Health = value; }
        public virtual int MaxHealth { get => _unit.MaxHealth; set => _unit.MaxHealth = value; }
        public virtual int Cost { get => _unit.Cost; set => _unit.Cost = value; }
        public virtual string PowerLevel { get => _unit.PowerLevel; set => _unit.PowerLevel = value; }
        public virtual int DamageDealt { get => _unit.DamageDealt; set => _unit.DamageDealt = value; }
        public virtual int FighterNumber { get; set; }
        public virtual ISpecialAbility? SpecialAbility { get => _unit.SpecialAbility; set => _unit.SpecialAbility = value; }
        public virtual IArmy? Army { get; set; }
        public virtual bool IsAlive => _unit.IsAlive;
        public virtual List<IUnitObserver> Observers => _unit.Observers;
        public IUnit GetInnerUnit() => _unit;

        // Проксируем все методы
        public virtual void AttachObserver(IUnitObserver observer) => _unit.AttachObserver(observer);
        public virtual void DetachObserver(IUnitObserver observer) => _unit.DetachObserver(observer);
        public virtual void ClearObservers() => _unit.ClearObservers();
        public virtual void TakeDamage(int damage, string attackerName) => _unit.TakeDamage(damage, attackerName);
        public virtual void AttackUnit(IUnit target) => _unit.AttackUnit(target);
        public virtual bool CanUseSpecialAbility(IUnit? target) => _unit.CanUseSpecialAbility(target);
        public virtual void UseSpecialAbility(IUnit? target) => _unit.UseSpecialAbility(target);
        public virtual string GetDisplayName(string prefix) => _unit.GetDisplayName(prefix);
        public virtual bool CanBeCloned() => _unit.CanBeCloned();
        public virtual bool CanBeHealed() => _unit.CanBeHealed();
    }
}