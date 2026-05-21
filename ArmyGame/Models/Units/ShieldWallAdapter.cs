using ArmyBattle.Models.Interfaces;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Адаптер для Гуляй-город: вариант стены щитов с изменённым здоровьем.
    /// </summary>
    public class ShieldWallAdapter : ShieldWall
    {
        public ShieldWallAdapter(int fighterNumber) : base(fighterNumber)
        {
            Name = "Гуляй город (адаптер)";
            MaxHealth = 100;
            Health = 100;
            Cost = 55;
            PowerLevel = "Гуляй город";
        }
    }
}
