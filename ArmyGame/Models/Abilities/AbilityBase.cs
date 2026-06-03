using System;
using System.Linq;
using ArmyBattle.Models.Interfaces;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Базовый абстрактный класс для специальных способностей.
    /// SRP: каждый подкласс отвечает за ОДИН тип способности.
    /// </summary>
    public abstract class AbilityBase : ISpecialAbility
    {
        public string Name { get; set; }
        public int Range { get; set; }
        public int Power { get; set; }

        protected AbilityBase(string name, int range, int power)
        {
            Name = name;
            Range = range;
            Power = power;
        }

        /// <summary>
        /// Выполняет способность (переопределяется в подклассах)
        /// </summary>
        public abstract void Execute(IUnit user, IUnit? target);
    }

    /// <summary>
    /// Способность стрельбы из лука (урон по цели).
    /// </summary>
    public class ArcherShootAbility : AbilityBase
    {
        public ArcherShootAbility(string name, int range, int power) 
            : base(name, range, power)
        {
        }

        public override void Execute(IUnit user, IUnit? target)
        {
            if (target == null) return;

            // Урон с учетом защиты, но минимум 1
            target.TakeDamage(Power, user.Name);
            user.DamageDealt += Math.Max(1, Power - target.Defence);
        }
    }

    /// <summary>
    /// Способность лечения (лекарь лечит союзника).
    /// </summary>
    public class HealerHealAbility : AbilityBase
    {
        private static readonly Random _random = new Random();
        public IUnit? LastHealed { get; private set; }

        public HealerHealAbility(string name, int range, int power) 
            : base(name, range, power)
        {
        }

        public override void Execute(IUnit user, IUnit? target)
        {
            if (user?.Army == null)
                return;

            LastHealed = null;

            // Выбираем случайного союзника, который может быть вылечен (не себя)
            var allies = user.Army.Units
                .Where(u => u.IsAlive && u != user && u.CanBeHealed() && !u.Is<StrongFighter>())
                .ToList();

            if (allies.Count == 0)
                return;

            var chosen = allies[_random.Next(allies.Count)];
            LastHealed = chosen;

            // Восстанавливаем здоровье до первоначального состояния
            chosen.Health = chosen.MaxHealth;
        }
    }

    /// <summary>
    /// Способность нанесения урона по умолчанию.
    /// </summary>
    public class DefaultDamageAbility : AbilityBase
    {
        public DefaultDamageAbility(string name, int range, int power) 
            : base(name, range, power)
        {
        }

        public override void Execute(IUnit user, IUnit? target)
        {
            if (target == null) return;

            target.TakeDamage(Power, user.Name);
            user.DamageDealt += Math.Max(1, Power - target.Defence);
        }
    }
}
