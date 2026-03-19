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
                10,
                8,
                25,
                15,
                "Слабый"
              )
        {
            FighterNumber = fighterNumber;
        }
    }
}
