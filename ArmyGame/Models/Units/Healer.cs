using System;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Лекарь
    /// </summary>
    public class Healer : Unit, ICanBeHealed
    {
        public Healer(int fighterNumber) 
            : base(
                "Лекарь", //name
                3, //attack
                2, //defence
                15, // health (maxhealth)
                20, //cost
                "Лекарь" //powerlevel
              )
        {
            FighterNumber = fighterNumber;
            // лекарь имеет способность лечения
            SpecialAbility = new SpecialAbility("Лечение", 5, 10);
        }
    }
}