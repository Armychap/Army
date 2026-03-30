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

        /// <summary>
        /// Переоборачивает юнита с актуальными настройками логирования
        /// </summary>
        public IUnit ReWrap(IUnit unit)
        {
            return Wrap(unit);
        }

        /// <summary>
        /// Фабричный метод для создания юнита по названию типа
        /// </summary>
        public IUnit CreateFromType(string unitType, int fighterNumber)
        {
            IUnit unit = unitType switch
            {
                nameof(WeakFighter) => CreateWeakFighter(fighterNumber),
                nameof(Archer) => CreateArcher(fighterNumber),
                nameof(StrongFighter) => CreateStrongFighter(fighterNumber),
                nameof(Healer) => CreateHealer(fighterNumber),
                nameof(Wizard) => CreateWizard(fighterNumber),
                nameof(ShieldWall) => CreateShieldWall(fighterNumber),
                _ => throw new InvalidOperationException($"Неизвестный тип юнита: {unitType}")
            };

            return Wrap(unit);
        }

        public IUnit Create(Func<int, IUnit> creator, int fighterNumber)
        {
            var unit = creator(fighterNumber);
            return Wrap(unit);
        }

        // ========== Методы-создатели для каждого типа юнита ==========

        /// <summary>
        /// Создает слабого боевца
        /// </summary>
        private IUnit CreateWeakFighter(int fighterNumber) => new WeakFighter(fighterNumber);

        /// <summary>
        /// Создает лучника
        /// </summary>
        private IUnit CreateArcher(int fighterNumber) => new Archer(fighterNumber);

        /// <summary>
        /// Создает сильного боевца
        /// </summary>
        private IUnit CreateStrongFighter(int fighterNumber) => new StrongFighter(fighterNumber);

        /// <summary>
        /// Создает лекаря
        /// </summary>
        private IUnit CreateHealer(int fighterNumber) => new Healer(fighterNumber);

        /// <summary>
        /// Создает мага
        /// </summary>
        private IUnit CreateWizard(int fighterNumber) => new Wizard(fighterNumber);

        /// <summary>
        /// Создает гуляй-город
        /// </summary>
        private IUnit CreateShieldWall(int fighterNumber) => new ShieldWall(fighterNumber);
    }

    public static class UnitFactoryProvider
    {
        public static UnitFactory Instance { get; } = new UnitFactory();
    }
}
