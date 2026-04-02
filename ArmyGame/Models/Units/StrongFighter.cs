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
                20, // атака
                15, // защита
                60, // здоровье
                40, // стоимость
                "Сильный"
              )
        {
            FighterNumber = fighterNumber;
        }
    }
}
