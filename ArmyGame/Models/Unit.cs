using System;

namespace ArmyBattle.Models
{
    // Класс для специального умения
    public class SpecialAbility
    {
        public string Name { get; set; }
        public int Range { get; set; }  // Дальность действия
        public int Power { get; set; }  // Сила способности
        
        public SpecialAbility(string name, int range, int power)
        {
            Name = name;
            Range = range;
            Power = power;
        }
    }
    
    // Основной класс для всех боевых единиц
    public abstract class Unit
    {
        // Название юнита для отображения
        public string Name { get; set; }
        
        // Сила атаки
        public int Attack { get; set; }
        
        // Защита от урона
        public int Defence { get; set; }
        
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
        public SpecialAbility SpecialAbility { get; set; }

        // Конструктор для инициализации характеристик
        protected Unit(string name, int attack, int defence, int health, 
                      int cost, string powerLevel)
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
        }

        // Проверка, жив ли юнит
        public bool IsAlive
        {
            get { return Health > 0; }
        }

        // Метод получения урона
        public virtual void TakeDamage(int damage, string attackerName)
        {
            // Расчет урона с учетом защиты
            int actualDamage = Math.Max(1, damage - Defence);
            Health -= actualDamage;
        }

        // Метод атаки другого юнита
        public virtual void AttackUnit(Unit target)
        {
            target.TakeDamage(Attack, Name);
            DamageDealt += Math.Max(1, Attack - target.Defence);
        }
        
        // Проверка наличия специального умения в пределах дальности
        public bool CanUseSpecialAbility(Unit target)
        {
            if (SpecialAbility == null || !IsAlive)
                return false;
            
            // Вычисляем дальность между юнитами (для упрощения - фиксированная дистанция 1)
            // В реальной игре здесь может быть система координат
            int distance = 1;
            return distance <= SpecialAbility.Range;
        }
        
        // Использование специального умения
        public virtual void UseSpecialAbility(Unit target)
        {
            if (CanUseSpecialAbility(target))
            {
                target.TakeDamage(SpecialAbility.Power, Name);
                DamageDealt += Math.Max(1, SpecialAbility.Power - target.Defence);
            }
        }
        
        // Получение отображаемого имени бойца
        public string GetDisplayName(string prefix)
        {
            return $"{prefix} {FighterNumber}";
        }
    }
}