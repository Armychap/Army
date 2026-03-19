using System;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Лучник
    /// </summary>
    public class Archer : Unit, ICanBeHealed
    {
        public Archer(int fighterNumber) 
            : base(
                "Лучник", //name
                5, //attack
                3, //defence
                18, // health (maxhealth)
                25, //cost
                "Лучник" //powerlevel (выводится в скобках с юнитом)
              )
        {
            FighterNumber = fighterNumber;
            // лучник имеет дальнюю способность
            SpecialAbility = new SpecialAbility("Выстрел стрелы", 5, 12);
        }
    }
}



