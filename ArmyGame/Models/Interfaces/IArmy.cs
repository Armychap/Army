using System.Collections.Generic;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Абстракция для армии, чтобы модули боя (например, BattleEngine) работали с интерфейсом
    /// (SRP) интерфейс описывает только поведение армии, классы-реализации отвечают за хранение данных
    /// (DIP) движок битвы зависит от IArmy, а не от конкретного класса Army
    /// <summary>
    public interface IArmy
    {
        //Название армии
        string Name { get; set; }
        
        //Цвет для отображения армии в консоли
        ConsoleColor Color { get; set; }
        
        //Общая стоимость всех бойцов в армии
        int TotalCost { get; set; }

        //Список всех бойцов в армии
        List<IUnit> Units { get; }
        
        //Список живых бойцов в порядке боя
        List<IUnit> AliveFightersInBattleOrder { get; }

        /// Добавить нового бойца в армию
        void AddUnit(IUnit unit);
        
        //Перемешать живых бойцов в случайном порядке для боя
        void ShuffleAliveFighters();
        
        /// Получить следующего живого бойца в порядке боя
        IUnit? GetNextFighterInBattleOrder();
        
        /// Удалить убитого бойца из армии
        void RemoveDeadFighter(IUnit deadFighter);
        
        /// Проверить наличие живых бойцов
        bool HasAliveUnits();
        
        /// Получить количество живых бойцов
        int AliveCount();
        
        /// Вывести информацию об армии в консоль
        void DisplayArmyInfo(bool showDetails = false);
        
        /// Сгенерировать армию с заданным бюджетом средств
        void GenerateArmyWithBudget(int budget);
        
        //Обновить список живых бойцов
        void RefreshAliveFighters();
    }
}