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
                "Маг",  
                7,       
                4,       
                22,      
                30,       
                "Маг"     
              )
        {
            FighterNumber = fighterNumber;
            // 30% шанс клонировать союзника при срабатывании способности
            // Дальность действия мага - 5
            SpecialAbility = new CloneAbility("Клонирование", 5, 30);
        }
    }
}
