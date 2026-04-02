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
                "Лекарь", 
                3, //атака
                2, //защита
                15, // здоровье (макс. здоровье)
                20, //стоимость
                "Лекарь" //уровень силы (выводится в скобках с юнитом)
              )
        {
            FighterNumber = fighterNumber;
            // способность лечения
            SpecialAbility = new SpecialAbility("Лечение", 5, 10);
        }
    }
}