using System;
using ArmyBattle.Models.Decorators;
using ArmyBattle.Models.Interfaces;

namespace ArmyBattle.Models
{
    /// Расширения для IUnit, чтобы получать корневой тип юнита, его короткое имя и т.д.
    public static class UnitExtensions
    {
        /// Получить корневой юнит, разворачивая все прокси
        ///
        /// Если у нас есть декоратор или адаптер, они оборачивают реальный юнит.
        /// Этот метод рекурсивно извлекает внутренний объект, чтобы получить
        /// исходный тип юнита (например, ShieldWall), даже если внешне он был
        /// обёрнут в прокси.
        public static IUnit GetRootUnit(this IUnit unit)
        {
            if (unit is BuffDecorator buffDecorator)
                return buffDecorator.GetInnerUnit().GetRootUnit();

            if (unit is IUnitAdapter adapter)
                return adapter.GetInnerUnit().GetRootUnit();

            return unit;
        }

        /// Проверить, является ли юнит определенным типом (например, Archer, Wizard и т.д.)
        public static bool Is<T>(this IUnit unit) where T : class, IUnit
        {
            return unit.GetRootUnit() is T;
        }

        // Получить тип корневого юнита
        public static Type GetRootType(this IUnit unit)
        {
            return unit.GetRootUnit().GetType();
        }

        // Получить короткое имя типа юнита для отображения в UI
        public static string GetShortType(this IUnit unit)
        {
            var type = unit.GetRootType();
            if (type == typeof(Wizard)) return "маг";
            if (type == typeof(Archer)) return "лук";
            if (type == typeof(Healer)) return "лек";
            if (type == typeof(StrongFighter)) return "сил";
            if (type == typeof(WeakFighter)) return "слаб";
            if (type == typeof(GulayGorod)) return "стен";
            return "?";
        }
    }
}
