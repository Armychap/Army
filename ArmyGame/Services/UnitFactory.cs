using System;
using ArmyBattle.Models;

namespace ArmyBattle.Services
{
    public class UnitFactory
    {
        /// <summary>
        /// Создает юнита без оберток (наблюдатели добавляются через ObserverManager)
        /// </summary>
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
                _ => throw new InvalidOperationException($"Неизвестный тип юнита: {unitType}")
            };
        }

        public IUnit Create(Func<int, IUnit> creator, int fighterNumber)
        {
            return creator(fighterNumber);
        }
    }

    public static class UnitFactoryProvider
    {
        public static UnitFactory Instance { get; } = new UnitFactory();
    }
}