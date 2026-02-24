using System;

namespace ArmyBattle.Models
{
    // Класс сильного бойца
    public class StrongFighter : Unit
    {
        public StrongFighter(int fighterNumber) 
            : base(
                "Сильный боец",
                18,
                10,
                60,
                40,
                "Сильный"
              )
        {
            FighterNumber = fighterNumber;
        }
    }
}
