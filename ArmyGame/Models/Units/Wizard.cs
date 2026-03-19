using System;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Маг: может с небольшой вероятностью клонировать случайного союзника (только слабого бойца или лучника).
    /// </summary>
    public class Wizard : Unit
    {
        public Wizard(int fighterNumber)
            : base(
                "Маг",    // name
                7,         // attack
                4,         // defence
                22,        // health
                30,        // cost
                "Маг"     // powerlevel
              )
        {
            FighterNumber = fighterNumber;
            SpecialAbility = new CloneAbility("Клонирование", 1, 15);
        }
    }
}
