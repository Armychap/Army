// BuffFactory.cs
using System;
using System.Collections.Generic;
using ArmyBattle.Models;
using ArmyBattle.Models.Interfaces;
using ArmyBattle.Models.Decorators;
using ArmyBattle.Services;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Фабрика для создания и применения баффов к юнитам
    /// </summary>
    public static class BuffFactory
    {
        /// <summary>
        /// Генератор случайных чисел для выбора случайного баффа
        /// </summary>
        private static readonly Random _random = new Random();

        /// <summary>
        /// Применяет случайный бафф к юниту
        /// </summary>
        public static IUnit ApplyRandomBuff(IUnit unit)
        {
            int choice = _random.Next(1, 5);
            
            return choice switch
            {
                1 => new HorseBuffDecorator(unit),
                2 => new ShieldBuffDecorator(unit),
                3 => new HelmetBuffDecorator(unit),
                4 => new SpearBuffDecorator(unit),
                _ => unit
            };
        }
        
        /// <summary>
        /// Применяет конкретный бафф к юниту по его названию
        /// </summary>
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
            // Проходим по цепочке декораторов, пока не найдём нужный бафф или не дойдём до конца
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