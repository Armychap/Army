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
                "Лучник", 
                5, //атака
                3, //защита
                18, // здоровье (макс. здоровье)
                25, //стоимость
                "Лучник" //уровень силы (выводится в скобках с юнитом)
              )
        {
            FighterNumber = fighterNumber;
            // использует стрелы
            SpecialAbility = new SpecialAbility("Выстрел стрелы", 5, 12);
        }
    }
}



