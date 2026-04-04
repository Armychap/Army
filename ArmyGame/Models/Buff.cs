using System;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Баф - оборудование, которое дает бонусы к атаке и защите
    /// </summary>
    public class Buff
    {
        public string Name { get; }
        public int AttackBonus { get; }
        public int DefenceBonus { get; }

        public Buff(string name, int attackBonus, int defenceBonus)
        {
            Name = name;
            AttackBonus = attackBonus;
            DefenceBonus = defenceBonus;
        }
    }

    /// <summary>
    /// Статические бафы
    /// </summary>
    public static class Buffs
    {
        public static readonly Buff Horse = new Buff("Конь", 5, 5); // защита и атака
        public static readonly Buff Shield = new Buff("Щит", 0, 10); // только защита 
        public static readonly Buff Helmet = new Buff("Шлем", 0, 8); // только защита
        public static readonly Buff Spear = new Buff("Копье", 10, 0); // только атака
    }
}