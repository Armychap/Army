using System;
using ArmyBattle.Models;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Фабрика для создания юнитов различных типов
    /// </summary>
    public class UnitFactory
    {
        /// <summary>
        /// Создаёт юнита без оберток (наблюдатели добавляются через ObserverManager)
        /// </summary>
        // Абстрактная фабрика — умеет создавать ЛЮБОГО юнита по строковому типу
        public IUnit CreateFromType(string unitType, int fighterNumber)
        {
            return unitType switch
            {
                nameof(WeakFighter) => new WeakFighter(fighterNumber),
                nameof(Archer) => new Archer(fighterNumber),
                nameof(StrongFighter) => new StrongFighter(fighterNumber),
                nameof(Healer) => new Healer(fighterNumber),
                nameof(Wizard) => new Wizard(fighterNumber),
                nameof(ShieldWall) => new ShieldWall(fighterNumber),
                nameof(ShieldWallAdapter) => new ShieldWallAdapter(fighterNumber),
                _ => throw new InvalidOperationException($"Неизвестный тип юнита: {unitType}")
            };
        }

        /// <summary>
        /// Создаёт юнита с помощью фабричного метода
        /// </summary>
        // Фабричный метод — принимает ФАБРИКУ (делегат) и вызывает её
        public IUnit Create(Func<int, IUnit> creator, int fighterNumber)
        {
            return creator(fighterNumber);
        }
    }

    /// <summary>
    /// Статический провайдер для доступа к экземпляру фабрики юнитов
    /// </summary>
    public static class UnitFactoryProvider
    {
        /// <summary>
        /// Единственный экземпляр фабрики юнитов
        /// </summary>
        public static UnitFactory Instance { get; } = new UnitFactory();
    }
}