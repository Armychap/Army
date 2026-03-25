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
    }
}
