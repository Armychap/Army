namespace ArmyBattle.Models.Interfaces
{
    /// <summary>
    /// Интерфейс наблюдателя за событиями юнита
    /// </summary>
    public interface IUnitObserver
    {
        /// <summary>
        /// Вызывается при получении урона
        /// </summary>
        void OnDamageTaken(IUnit unit, int damage, string attackerName, int newHealth);
        
        /// <summary>
        /// Вызывается при смерти юнита
        /// </summary>
        void OnDeath(IUnit unit, string killerName);
        
        /// <summary>
        /// Вызывается при лечении юнита
        /// </summary>
        void OnHealed(IUnit unit, int amount, int newHealth);
    }
}