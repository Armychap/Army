using System;
using System.Collections.Generic;
using System.Linq;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Сильный боец
    /// </summary>
    public class StrongFighter : Unit
    {
        // Список бафов
        public List<Buff> Buffs { get; } = new List<Buff>();

        public StrongFighter(int fighterNumber) 
            : base(
                "Сильный боец",
                20, // атака
                15, // защита
                60, // здоровье
                40, // стоимость
                "Сильный"
              )
        {
            FighterNumber = fighterNumber;
        }

        // Эффективная атака с учетом бафов
        public override int EffectiveAttack => Attack + Buffs.Sum(b => b.AttackBonus);

        // Эффективная защита с учетом бафов
        public override int EffectiveDefence => Defence + Buffs.Sum(b => b.DefenceBonus);
    }
}
