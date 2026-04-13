using System;
using System.Collections.Generic;
using ArmyBattle.Models.Interfaces;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Специальное умение, которым может обладать юнит.
    /// </summary>
    public class SpecialAbility : ISpecialAbility
    {
        public string Name { get; set; }
        public int Range { get; set; }  // Дальность действия
        public int Power { get; set; }  // Сила способности

        public IUnit? LastHealed { get; private set; }  // Для лечения

        private Random random = new Random();

        public SpecialAbility(string name, int range, int power)
        {
            Name = name;
            Range = range;
            Power = power;
            LastHealed = null;
        }

        // Выполняет способность: урон или лечение в зависимости от типа юнита
        public void Execute(IUnit? user, IUnit? target)
        {
            var rootUser = user?.GetRootUnit();

            if (rootUser is Healer && user?.Army != null)
            {
                // Лечение: выбрать случайного союзника, который может быть вылечен (не себя)
                var allies = user?.Army.Units
                    .Where(u => u.IsAlive && u != user && u.CanBeHealed() && !u.Is<StrongFighter>())
                    .ToList();

                if (allies?.Count == 0)
                {
                    LastHealed = null;
                    return;
                }

                var chosen = allies?[random.Next(allies.Count)];
                LastHealed = chosen;

                // Восстанавливаем здоровье до первоначального состояния
                if (chosen != null)
                {
                    chosen.Health = chosen.MaxHealth;
                }
            }
            else if (rootUser is Archer)
            {
                // Для лучника урон с учетом защиты, но минимум 1
                if (target == null || user == null) return;
                target.TakeDamage(Power, user.Name);
                user.DamageDealt += Math.Max(1, Power - target.Defence);
            }
            else
            {
                // Урон по умолчанию
                if (user == null || target == null) return;
                target.TakeDamage(Power, user.Name);
                user.DamageDealt += Math.Max(1, Power - target.Defence);
            }
        }
    }

    /// <summary>
    /// Базовая реализация IUnit. Позволяет соблюдать Liskov Substitution:
    /// любой подкласс может использоваться вместо IUnit.
    /// можно наследоваться и расширять чужими классами.
    /// SRP: отвечает исключительно за поведение отдельного юнита
    /// </summary>
    public abstract class Unit : IUnit
    {
        // Название юнита для отображения
        public string Name { get; set; }

        // Сила атаки
        public int Attack { get; set; }

        // Защита от урона
        public int Defence { get; set; }

        // Эффективная атака с учетом бафов
        public virtual int EffectiveAttack => Attack;

        // Эффективная защита с учетом бафов
        public virtual int EffectiveDefence => Defence;

        // Текущее здоровье
        public int Health { get; set; }

        // Максимальный запас здоровья
        public int MaxHealth { get; set; }

        // Стоимость найма
        public int Cost { get; set; }

        // Уровень силы (слабый, средний, сильный)
        public string PowerLevel { get; set; }

        // Счетчик нанесенного урона
        public int DamageDealt { get; set; }

        // Порядковый номер бойца в армии
        public int FighterNumber { get; set; }

        // Специальная способность (например, для лучников)
        public ISpecialAbility? SpecialAbility { get; set; }

        // Армия, к которой принадлежит юнит
        public IArmy? Army { get; set; }
        /// <summary>
        /// Список наблюдателей
        /// </summary>
        public List<IUnitObserver> Observers { get; } = new List<IUnitObserver>();

        public bool IsAlive => Health > 0;
        // Конструктор для инициализации характеристик
        protected Unit(string name, int attack, int defence, int health,
                      int cost, string powerLevel, IArmy? army = null)
        {
            Name = name;
            Attack = attack;
            Defence = defence;
            Health = health;
            MaxHealth = health;
            Cost = cost;
            PowerLevel = powerLevel;
            DamageDealt = 0;
            FighterNumber = 0;
            SpecialAbility = null;
            Army = army;
        }


        /// <summary>
        /// Подписать наблюдателя
        /// </summary>
        public void AttachObserver(IUnitObserver observer)
        {
            if (!Observers.Contains(observer))
                Observers.Add(observer);
        }

        /// <summary>
        /// Отписать наблюдателя
        /// </summary>
        public void DetachObserver(IUnitObserver observer)
        {
            Observers.Remove(observer);
        }

        /// <summary>
        /// Очистить всех наблюдателей
        /// </summary>
        public void ClearObservers()
        {
            Observers.Clear();
        }

        /// <summary>
        /// Уведомить наблюдателей о получении урона
        /// </summary>
        protected void NotifyDamageTaken(int damage, string attackerName, int newHealth)
        {
            foreach (var observer in Observers)
            {
                observer.OnDamageTaken(this, damage, attackerName, newHealth);
            }
        }

        /// <summary>
        /// Уведомить наблюдателей о смерти
        /// </summary>
        protected void NotifyDeath(string killerName)
        {
            foreach (var observer in Observers)
            {
                observer.OnDeath(this, killerName);
            }
        }

        /// <summary>
        /// Уведомить наблюдателей о лечении
        /// </summary>
        protected void NotifyHealed(int amount, int newHealth)
        {
            foreach (var observer in Observers)
            {
                observer.OnHealed(this, amount, newHealth);
            }
        }

        // Метод получения урона
        public virtual void TakeDamage(int damage, string attackerName)
        {
            // Расчет урона с учетом защиты
            int actualDamage = Math.Max(1, damage - EffectiveDefence);
            int previousHealth = Health;

            Health -= actualDamage;
            NotifyDamageTaken(actualDamage, attackerName, Health);

            if (previousHealth > 0 && Health <= 0)
            {
                NotifyDeath(attackerName);
            }
        }

        // Перегрузка для совместимости
        public virtual void TakeDamage(int damage)
        {
            TakeDamage(damage, "Unknown");
        }

        // Атаковать цель через интерфейс
        public virtual void AttackUnit(IUnit target)
        {
            target.TakeDamage(EffectiveAttack, Name);
            DamageDealt += Math.Max(1, EffectiveAttack - target.EffectiveDefence);
        }

        // Может ли юнит быть скопирован магом
        public virtual bool CanBeCloned()
        {
            return true;
        }

        // Может ли юнит быть вылечен лекарем
        public virtual bool CanBeHealed()
        {
            return true;
        }

        // Проверка наличия специального умения в пределах дальности
        public bool CanUseSpecialAbility(IUnit? target)
        {
            if (SpecialAbility == null || !IsAlive)
                return false;
            int distance = 1;
            return distance <= SpecialAbility.Range;
        }

        // Использование специального умения через интерфейс
        public virtual void UseSpecialAbility(IUnit? target)
        {
            if (CanUseSpecialAbility(target))
            {
                SpecialAbility?.Execute(this, target);
            }
        }

        // Получение отображаемого имени бойца
        public string GetDisplayName(string prefix)
        {
            return $"{prefix} {FighterNumber}";
        }
    }
}