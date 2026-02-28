using System;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Сильный боец
    /// </summary>
    public class StrongFighter : Unit
    {
        public StrongFighter(int fighterNumber) 
            : base(
                "Сильный боец",
                20,
                15,
                60,
                40,
                "Сильный"
              )
        {
            FighterNumber = fighterNumber;
        }
    }
}
