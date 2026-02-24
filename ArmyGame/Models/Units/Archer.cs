using System;

namespace ArmyBattle.Models
{
    // Класс лучника
    public class Archer : Unit
    {
        public Archer(int fighterNumber) 
            : base(
                "Лучник",
                10,
                2,
                18,
                25,
                "Лучник"
              )
        {
            FighterNumber = fighterNumber;
            // Лучник имеет способность стрелять на дальность
            // Обычная атака луком: 10
            // Дальний выстрел: 12 урона на дальность 5
            SpecialAbility = new SpecialAbility("Выстрел стрелы", 5, 12);
        }
    }
}
