using System;

namespace ArmyBattle.Models
{
    public static class UnitExtensions
    {
        public static IUnit GetRootUnit(this IUnit unit)
        {
            IUnit current = unit;
            while (current is UnitProxy proxy)
            {
                current = proxy.InnerUnit;
            }
            return current;
        }

        public static bool Is<T>(this IUnit unit) where T : class, IUnit
        {
            return unit.GetRootUnit() is T;
        }

        public static Type GetRootType(this IUnit unit)
        {
            return unit.GetRootUnit().GetType();
        }

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
