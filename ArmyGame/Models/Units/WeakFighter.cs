using System;

namespace ArmyBattle.Models
{
    // Класс слабого бойца
    public class WeakFighter : Unit
    {
        public WeakFighter(int fighterNumber) 
            : base(
                "Слабый боец",
                8,
                3,
                25,
                15,
                "Слабый"
              )
        {
            FighterNumber = fighterNumber;
        }
    }
}
