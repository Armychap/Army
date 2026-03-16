using System;
using System.Collections.Generic;

namespace ArmyBattle.Models
{
    /// <summary>
    /// Представляет армию и её поведение.
    /// отвечает только за управление набором бойцов
    /// новые типы юнитов добавляются за счёт интерфейса IUnit, не изменяя код класса
    /// </summary>
    public class Army : IArmy
    {
        public string Name { get; set; }
        // Применяем интерфейсы для юнитов
        public List<IUnit> Units { get; set; }
        public ConsoleColor Color { get; set; }
        public int TotalCost { get; set; }

        // Список живых бойцов в случайном порядке для боя
        public List<IUnit> AliveFightersInBattleOrder { get; set; }
        public int CurrentFighterIndex { get; set; }

        private static Random random = new Random();

        public Army(string name, ConsoleColor color)
        {
            Name = name;
            Color = color;
            Units = new List<IUnit>();
            AliveFightersInBattleOrder = new List<IUnit>();
            TotalCost = 0;
            CurrentFighterIndex = 0;
        }

        // Добавление юнита в армию (работает с интерфейсом IUnit)
        public void AddUnit(IUnit unit)
        {
            Units.Add(unit);
            unit.Army = this;
            TotalCost += unit.Cost;
        }

        //Перемешивает список живых бойцов в случайном порядке
        public void ShuffleAliveFighters()
        {
            AliveFightersInBattleOrder.Clear();

            foreach (var unit in Units)
            {
                if (unit.IsAlive)
                {
                    AliveFightersInBattleOrder.Add(unit);
                }
            }

            for (int i = AliveFightersInBattleOrder.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = AliveFightersInBattleOrder[i];
                AliveFightersInBattleOrder[i] = AliveFightersInBattleOrder[j];
                AliveFightersInBattleOrder[j] = temp;
            }

            CurrentFighterIndex = 0;
        }

        //Возвращает следующего бойца из перемешанного списка
        public IUnit? GetNextFighterInBattleOrder()
        {
            if (CurrentFighterIndex < AliveFightersInBattleOrder.Count)
            {
                IUnit nextFighter = AliveFightersInBattleOrder[CurrentFighterIndex];
                CurrentFighterIndex++;
                return nextFighter;
            }

            return null;
        }

        /// Удаляет мёртвого бойца из порядка боя, корректируя индекс
        public void RemoveDeadFighter(IUnit deadFighter)
        {
            int removedIndex = AliveFightersInBattleOrder.IndexOf(deadFighter);

            if (removedIndex >= 0)
            {
                AliveFightersInBattleOrder.RemoveAt(removedIndex);

                // Если удалённый был перед текущим индексом, сдвигаем индекс влево
                if (removedIndex < CurrentFighterIndex && CurrentFighterIndex > 0)
                {
                    CurrentFighterIndex--;
                }

                // Если список пуст, сбрасываем индекс
                if (AliveFightersInBattleOrder.Count == 0)
                {
                    CurrentFighterIndex = 0;
                }
                else
                {
                    // Убедимся, что индекс не выходит за пределы списка
                    CurrentFighterIndex = Math.Min(CurrentFighterIndex, AliveFightersInBattleOrder.Count);
                }
            }
        }

        // Проверка наличия живых юнитов
        public bool HasAliveUnits()
        {
            // Надёжно проверяем по основному списку Units на наличие живых юнитов
            foreach (var u in Units)
            {
                if (u.IsAlive) return true;
            }
            return false;
        }

        // Количество живых юнитов
        public int AliveCount()
        {
            int count = 0;
            foreach (var u in Units)
            {
                if (u.IsAlive) count++;
            }
            return count;
        }

        // Вывод информации об армии
        public void DisplayArmyInfo(bool showDetails = false)
        {
            Console.ForegroundColor = Color;
            Console.WriteLine($"\n{Name}:");
            Console.ResetColor();

            Console.WriteLine($"Всего бойцов: {Units.Count}");
            Console.WriteLine($"Живых бойцов: {AliveCount()}");
            Console.WriteLine($"Общая стоимость: {TotalCost}");

            if (showDetails)
            {
                Console.WriteLine("\nСостав армии:");
                foreach (var unit in Units)
                {
                    string status = unit.IsAlive ?
                        $"Здоровье: {unit.Health}/{unit.MaxHealth}" :
                        "Убит";
                    Console.WriteLine($"  {Name} Боец {unit.FighterNumber} - {unit.PowerLevel} (Стоимость: {unit.Cost}) - {status}");
                }
            }
        }

        // Генерация армии с заданным бюджетом
        public void GenerateArmyWithBudget(int budget)
        {
            Units.Clear();
            AliveFightersInBattleOrder.Clear();
            TotalCost = 0;

            int remainingBudget = budget;
            int fighterNumber = 1;

            // Список доступных типов бойцов с их стоимостью
            var availableFighters = new List<Tuple<int, Func<int, IUnit>>>
            {
                new Tuple<int, Func<int, IUnit>>(40, (num) => new StrongFighter(num)),
                new Tuple<int, Func<int, IUnit>>(25, (num) => new Archer(num)),
                new Tuple<int, Func<int, IUnit>>(20, (num) => new Healer(num)),
                new Tuple<int, Func<int, IUnit>>(15, (num) => new WeakFighter(num))
            };

            // Пока есть бюджет на любого бойца
            while (remainingBudget >= 15)
            {
                // Выбираем случайного бойца, которого можем себе позволить
                var affordableFighters = new List<Tuple<int, Func<int, IUnit>>>();
                foreach (var fighter in availableFighters)
                {
                    if (fighter.Item1 <= remainingBudget)
                    {
                        affordableFighters.Add(fighter);
                    }
                }

                if (affordableFighters.Count == 0)
                    break;

                // Случайный выбор бойца из доступных
                var selectedFighter = affordableFighters[random.Next(affordableFighters.Count)];

                IUnit newUnit = selectedFighter.Item2(fighterNumber);
                AddUnit(newUnit);
                remainingBudget -= selectedFighter.Item1;
                fighterNumber++;
            }

            // Изначально все бойцы живые, добавляем их в список для боя
            foreach (var unit in Units)
            {
                if (unit.IsAlive)
                {
                    AliveFightersInBattleOrder.Add(unit);
                }
            }

            // Перемешиваем бойцов для случайного порядка в бою
            ShuffleAliveFighters();
        }

        public void RefreshAliveFighters()
        {
            AliveFightersInBattleOrder.Clear();
            foreach (var unit in Units)
            {
                if (unit.IsAlive)
                {
                    AliveFightersInBattleOrder.Add(unit);
                }
            }
            ShuffleAliveFighters();
        }
    }
}