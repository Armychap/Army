using System;
using ArmyBattle.Models;

namespace ArmyBattle.Services
{
    public class UnitFactory
    {
        public IUnit Wrap(IUnit unit)
        {
            var root = unit.GetRootUnit();
            IUnit wrapped = root;

            if (ProxySettings.Current.EnableDamageLog)
            {
                wrapped = new DamageLogUnitProxy(wrapped);
            }

            if (ProxySettings.Current.EnableDeathBeep)
            {
                wrapped = new DeathBeepUnitProxy(wrapped);
            }

            return wrapped;
        }

        public IUnit CreateFromType(string unitType, int fighterNumber)
        {
            IUnit unit = unitType switch
            {
                nameof(WeakFighter) => new WeakFighter(fighterNumber),
                nameof(Archer) => new Archer(fighterNumber),
                nameof(StrongFighter) => new StrongFighter(fighterNumber),
                nameof(Healer) => new Healer(fighterNumber),
                nameof(Wizard) => new Wizard(fighterNumber),
                nameof(ShieldWall) => new ShieldWall(fighterNumber),
                _ => throw new InvalidOperationException($"Неизвестный тип юнита: {unitType}")
            };

            return Wrap(unit);
        }

        public IUnit Create(Func<int, IUnit> creator, int fighterNumber)
        {
            var unit = creator(fighterNumber);
            return Wrap(unit);
        }
    }

    public static class UnitFactoryProvider
    {
        public static UnitFactory Instance { get; } = new UnitFactory();
    }
}
