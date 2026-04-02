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
                "Гуляй город",     
                0,                 // атака - нет атаки вообще
                50,                // защита 
                70,                // здоровье
                55,                // стоимость
                "Гуляй город"            // уровень силы
              )
        {
            FighterNumber = fighterNumber;
            // Гуляй поле не имеет специальных способностей
            // Его нельзя копировать (Wizard) или лечить (Healer)
            SpecialAbility = null;
        }

        /// <summary>
        /// Гуляй город не атакует вообще - он только защищает
        /// </summary>
        public override void AttackUnit(IUnit target)
        {
            // Гуляй город не наносит урон - он только защищает
            // Никакого урона противнику
        }
    }
}

