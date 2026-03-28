using System;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Гуляй город - движущаяся стена щитов
    /// Юнит с огромной защитой, но без атаки
    /// Не может быть скопирован (Wizard) или залечен (Healer)
    /// </summary>
    public class ShieldWall : Unit
    {
        public ShieldWall(int fighterNumber) 
            : base(
                "Гуляй город",     // name
                0,                 // attack - нет атаки вообще
                50,                // defence - очень высокая защита
                70,                // health - большое здоровье
                55,                // cost - дорогой юнит
                "Стена"            // powerlevel
              )
        {
            FighterNumber = fighterNumber;
            // Гуляй город не имеет специальных способностей
            // Его нельзя копировать (Wizard) или лечить (Healer)
            SpecialAbility = null;
        }

        /// <summary>
        /// Гуляй город не идеален - не наносит урон своей атакой, но
        /// когда его атакуют, противники получают очень мало урона из-за высокой защиты.
        /// Гуляй город не атакует вообще - он только защищает
        /// </summary>
        public override void AttackUnit(IUnit target)
        {
            // Гуляй город не наносит урон - он только защищает
            // Никакого урона противнику
        }
    }
}

