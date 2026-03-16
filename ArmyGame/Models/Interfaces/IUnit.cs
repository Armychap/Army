using System;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Абстракция для бойца; позволяет работать с любыми реализациями
    /// (ISP) интерфейс содержит только необходимые члены, клиентам не приходится зависеть от лишнего.</para>
    /// (DIP) остальные компоненты зависят от этой абстракции, а не от конкретных классов.</para>
    /// </summary>
    public interface IUnit
    {
        //Название бойца
        string Name { get; set; }
        
        //Показатель атаки - определяет наносимый урон
        int Attack { get; set; }
        
        //Показатель защиты - уменьшает получаемый урон
        int Defence { get; set; }
        
        //Текущее здоровье бойца
        int Health { get; set; }
        
        //Максимальное здоровье бойца
        int MaxHealth { get; set; }
        
        //Стоимость бойца в условных единицах
        int Cost { get; set; }
        
        //Уровень мощности (например: "Слабый", "Обычный", "Сильный")
        string PowerLevel { get; set; }
        
        //Количество урона нанесено этим бойцом в течение боя
        int DamageDealt { get; set; }
        
        //Номер бойца в армии для идентификации
        int FighterNumber { get; set; }
        
        //Специальная способность бойца
        ISpecialAbility SpecialAbility { get; set; }

        //Армия, к которой принадлежит боец
        IArmy Army { get; set; }

        /// Проверить живой ли боец
        bool IsAlive { get; }

        /// Получить урон от атакующего
        void TakeDamage(int damage, string attackerName);
        
        /// Атаковать цель
        void AttackUnit(IUnit target);
        
        /// Проверить может ли боец использовать специальную способность против цели
        bool CanUseSpecialAbility(IUnit target);
        
        /// Использовать специальную способность против цели
        void UseSpecialAbility(IUnit target);
        
        /// Получить отображаемое имя бойца с префиксом
        string GetDisplayName(string prefix);
    }
}