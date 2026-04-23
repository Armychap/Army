// BuffFactory.cs
using System;
using System.Collections.Generic;
using ArmyBattle.Models;
using ArmyBattle.Models.Interfaces;
using ArmyBattle.Models.Decorators;

namespace ArmyBattle.Services
{
    public static class BuffFactory
    {
        public static IUnit ApplyRandomBuff(IUnit unit)
        {
            Random random = new Random();
            int choice = random.Next(1, 5);
            
            return choice switch
            {
                1 => new HorseBuffDecorator(unit),
                2 => new ShieldBuffDecorator(unit),
                3 => new HelmetBuffDecorator(unit),
                4 => new SpearBuffDecorator(unit),
                _ => unit
            };
        }
        
        public static IUnit ApplyBuff(IUnit unit, string buffType)
        {
            return buffType.ToLower() switch
            {
                "horse" => new HorseBuffDecorator(unit),
                "shield" => new ShieldBuffDecorator(unit),
                "helmet" => new HelmetBuffDecorator(unit),
                "spear" => new SpearBuffDecorator(unit),
                _ => unit
            };
        }
        
        /// <summary>
        /// Проверяет, есть ли у юнита бафф определённого типа
        /// </summary>
        public static bool HasBuff<T>(IUnit unit) where T : BuffDecorator
        {
            while (unit is BuffDecorator decorator)
            {
                if (decorator is T)
                    return true;
                unit = decorator.GetInnerUnit();
            }
            return false;
        }
        
    }
}