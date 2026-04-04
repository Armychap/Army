using System;
using System.Collections.Generic;
using System.Linq;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Слабый боец
    /// </summary>
    public class WeakFighter : Unit, ICanBeHealed
    {
        // Список бафов
        public List<Buff> Buffs { get; } = new List<Buff>();

        public WeakFighter(int fighterNumber) 
            : base(
                "Слабый боец",
                10, // атака
                8, // защита
                25, // здоровье
                15, // стоимость
                "Слабый"
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
