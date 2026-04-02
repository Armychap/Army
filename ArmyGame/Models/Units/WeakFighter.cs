using System;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Слабый боец
    /// </summary>
    public class WeakFighter : Unit, ICanBeHealed
    {
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
    }
}
