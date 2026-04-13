using System;

namespace ArmyBattle.Models
{
    /// Расширения для IUnit, чтобы получать корневой тип юнита, его короткое имя и т.д.
    public static class UnitExtensions
    {
        /// Получить корневой юнит, разворачивая все прокси
        public static IUnit GetRootUnit(this IUnit unit)
        {
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
            if (type == typeof(ShieldWall)) return "стен";
            return "?";
        }
    }
}
