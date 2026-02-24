using System;

namespace ArmyBattle.Models
{
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
        
        // Получение отображаемого имени бойца
        public string GetDisplayName(string prefix)
        {
            return $"{prefix} {FighterNumber}";
        }
    }
}