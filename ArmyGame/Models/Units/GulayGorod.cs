using ArmyBattle.Models.Interfaces;

namespace ArmyBattle.Models
{
    public class GulayGorod : IUnit
    {
        public GulayGorod(int fighterNumber)
        {
            FighterNumber = fighterNumber;
        }

        // Свойства IUnit
        public string Name { get; set; } = "Гуляй город";
        public int Attack { get; set; } = 0;  // Не может атаковать
        public int Defence { get; set; } = 50;  // Высокая защита
        public int EffectiveAttack => Attack;
        public int EffectiveDefence => Defence;
        public int Health { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public int Cost { get; set; } = 55;  // Высокая стоимость
        public string PowerLevel { get; set; } = "Гуляй город";
        public int DamageDealt { get; set; } = 0;
        public int FighterNumber { get; set; }
        public ISpecialAbility? SpecialAbility { get; set; } = null;
        public IArmy? Army { get; set; }
        public bool IsAlive => Health > 0;
        public List<IUnitObserver> Observers { get; } = new();

        // Реализация методов IUnit
        public void AttachObserver(IUnitObserver observer)
        {
            if (!Observers.Contains(observer))
                Observers.Add(observer);
        }

        public void DetachObserver(IUnitObserver observer)
        {
            Observers.Remove(observer);
        }

        public void ClearObservers()
        {
            Observers.Clear();
        }

        public void TakeDamage(int damage, string attackerName)
        {
            // АДАПТЕР: Специальная механика урона для Гуляй-города
            // Блокирует половину входящего урона, но минимум 1 урон всегда проходит
            // Это отличает его от обычных юнитов (которые используют EffectiveDefence)
            int reducedDamage = Math.Max(1, damage - (Defence / 2));
            
            Health -= reducedDamage;
            int newHealth = Health;
            if (Health < 0) newHealth = 0;

            foreach (var observer in Observers)
            {
                observer.OnDamageTaken(this, reducedDamage, attackerName, newHealth);
            }

            if (!IsAlive)
            {
                foreach (var observer in Observers)
                {
                    observer.OnDeath(this, attackerName);
                }
            }
        }

        public void AttackUnit(IUnit target)
        {
            // АДАПТЕР: Гуляй-город - пассивная оборона
            // Это сооружение не может атаковать, только защищать
            // Используется только в защитных формациях (Wall formation)
        }

        public bool CanUseSpecialAbility(IUnit? target)
        {
            // АДАПТЕР: Гуляй-город не имеет специальных способностей
            return false;
        }

        public void UseSpecialAbility(IUnit? target)
        {
            // АДАПТЕР: Нет реализации - метод не используется
            // Оставлен для совместимости с интерфейсом IUnit
        }

        public string GetDisplayName(string prefix)
        {
            return $"{prefix}{Name}";
        }

        public bool CanBeCloned()
        {
            // АДАПТЕР: Гуляй-город не может быть клонирован
            // Это не просто боец, а стратегическое сооружение
            return false;
        }

        public bool CanBeHealed()
        {
            // АДАПТЕР: Гуляй-город не может быть исцелён
            // Его урон необратим (как реальное укрепление)
            return false;
        }
    }
}

