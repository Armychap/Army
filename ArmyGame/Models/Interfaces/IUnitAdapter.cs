namespace ArmyBattle.Models.Interfaces
{
    /// <summary>
    /// Интерфейс для объектов-адаптеров, которые оборачивают другой IUnit.
    /// Позволяет разворачивать проксирующие экземпляры.
    /// </summary>
    public interface IUnitAdapter : IUnit
    {
        /// <summary>
        /// Возвращает внутренний оригинальный юнит.
        /// </summary>
        IUnit GetInnerUnit();
    }
}
