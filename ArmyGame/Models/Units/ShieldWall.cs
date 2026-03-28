using System;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Гуляй-город: движущаяся стена со щитами.
    /// Не атакует, имеет огромную защиту, не может быть скопирован или вылечен.
    /// Просто стоит и блокирует атаки.
    /// </summary>
    public class ShieldWall : Unit
    {
        public ShieldWall(int fighterNumber)
            : base(
                "Гуляй-город",
                0,  // Нет атаки
                50, // Огромная защита
                100, // Больше HP
                60,  // Стоимость
                "Стена"
              )
        {
            FighterNumber = fighterNumber;
        }

        // Переопределяем атаку: не атакует вообще
        public override void AttackUnit(IUnit target)
        {
            // Гуляй-город не атакует, просто стоит
            // Ничего не делаем
        }

        // Переопределяем получение урона: учитываем огромную защиту
        public override void TakeDamage(int damage, string attackerName)
        {
            // Защита уменьшает урон значительно
            int actualDamage = Math.Max(0, damage - Defence * 2); // Удвоенная защита
            Health -= actualDamage;
        }

        // Запрещаем копирование: переопределяем, чтобы не быть целью для мага
        public override bool CanBeCloned()
        {
            return false;
        }

        // Запрещаем лечение: переопределяем, чтобы не быть целью для лекаря
        public override bool CanBeHealed()
        {
            return false;
        }
    }
}