using ArmyBattle.Services;
using System;


namespace ArmyBattle.Models
{
    /// <summary>
    /// Представляет армию и её поведение.
    /// отвечает только за управление набором бойцов
    /// новые типы юнитов добавляются за счёт интерфейса IUnit, не изменяя код класса
    /// </summary>
    public class Army : IArmy
    {
        /// <summary>
        /// Название армии
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Список всех юнитов армии (живых и мёртвых)
        /// </summary>
        public List<IUnit> Units { get; set; }
        
        /// <summary>
        /// Цвет армии для вывода в консоли
        /// </summary>
        public ConsoleColor Color { get; set; }
        
        /// <summary>
        /// Общая стоимость всех юнитов армии
        /// </summary>
        public int TotalCost { get; set; }

        /// <summary>
        /// Список живых бойцов в порядке боя (перемешанный или сохранённый)
        /// </summary>
        public List<IUnit> AliveFightersInBattleOrder { get; set; }
        
        /// <summary>
        /// Индекс текущего бойца для поочерёдного вызова
        /// </summary>
        public int CurrentFighterIndex { get; set; }

        /// <summary>
        /// Генератор случайных чисел для армии
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Конструктор армии с указанием имени и цвета
        /// </summary>
        public Army(string name, ConsoleColor color)
        {
            Name = name;
            Color = color;
            Units = new List<IUnit>();
            AliveFightersInBattleOrder = new List<IUnit>();
            TotalCost = 0;
            CurrentFighterIndex = 0;
        }

        /// <summary>
        /// Добавление юнита в армию (работает с интерфейсом IUnit)
        /// </summary>
        public void AddUnit(IUnit unit)
        {
            Units.Add(unit);
            unit.Army = this;
            TotalCost += unit.Cost;
            if (unit.IsAlive)
            {
                AliveFightersInBattleOrder.Add(unit);
            }
        }

        /// <summary>
        /// Перемешивает список живых бойцов в случайном порядке
        /// </summary>
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

            // Алгоритм Фишера-Йетса для случайного перемешивания
            for (int i = AliveFightersInBattleOrder.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (AliveFightersInBattleOrder[i], AliveFightersInBattleOrder[j]) =
                    (AliveFightersInBattleOrder[j], AliveFightersInBattleOrder[i]);
            }

            CurrentFighterIndex = 0;
        }

        /// <summary>
        /// Возвращает следующего бойца из перемешанного списка.
        /// Если индекс вышел за пределы, возвращаем первого бойца заново.
        /// </summary>
        public IUnit? GetNextFighterInBattleOrder()
        {
            if (AliveFightersInBattleOrder.Count == 0)
                return null;

            if (CurrentFighterIndex >= AliveFightersInBattleOrder.Count)
                CurrentFighterIndex = 0;

            IUnit nextFighter = AliveFightersInBattleOrder[CurrentFighterIndex];
            CurrentFighterIndex++;
            return nextFighter;
        }

        /// <summary>
        /// Удаляет мёртвого бойца из порядка боя, корректируя индекс
        /// </summary>
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

        /// <summary>
        /// Проверка наличия живых юнитов
        /// </summary>
        public bool HasAliveUnits()
        {
            // Надёжно проверяем по основному списку Units на наличие живых юнитов
            foreach (var u in Units)
            {
                if (u.IsAlive) return true;
            }
            return false;
        }

        /// <summary>
        /// Количество живых юнитов
        /// </summary>
        public int AliveCount()
        {
            int count = 0;
            foreach (var u in Units)
            {
                if (u.IsAlive) count++;
            }
            return count;
        }

        /// <summary>
        /// Вывод информации об армии
        /// </summary>
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

        /// <summary>
        /// Генерация армии с заданным бюджетом
        /// </summary>
        public void GenerateArmyWithBudget(int budget)
        {
            Units.Clear();
            AliveFightersInBattleOrder.Clear();
            TotalCost = 0;

            int remainingBudget = budget;
            int fighterNumber = 1;

            // Каждый элемент списка — это ФАБРИЧНЫЙ МЕТОД для создания КОНКРЕТНОГО юнита
            // Список доступных типов бойцов с их стоимостью и фабричными методами
            var availableFighters = new List<Tuple<int, Func<int, IUnit>>>
            {
                new Tuple<int, Func<int, IUnit>>(55, (num) => new GulayGorod(num)),
                new Tuple<int, Func<int, IUnit>>(40, (num) => new StrongFighter(num)),
                new Tuple<int, Func<int, IUnit>>(30, (num) => new Wizard(num)),
                new Tuple<int, Func<int, IUnit>>(25, (num) => new Archer(num)),
                new Tuple<int, Func<int, IUnit>>(20, (num) => new Healer(num)),
                new Tuple<int, Func<int, IUnit>>(15, (num) => new WeakFighter(num))
            };

            // Пока есть бюджет на любого бойца (минимум 15 - самый дешёвый)
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

                // Вызов фабричного метода
                IUnit newUnit = selectedFighter.Item2(fighterNumber);
                AddUnit(newUnit);
                remainingBudget -= selectedFighter.Item1;
                fighterNumber++;
            }

            // Сохраняем порядок, который создаётся при построении армии
            CurrentFighterIndex = 0;
        }

        /// <summary>
        /// Обновляет список живых бойцов (с перемешиванием)
        /// </summary>
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
            CurrentFighterIndex = 0;
        }
        
        /// <summary>
        /// Обновляет список живых бойцов без перемешивания (сохраняет порядок)
        /// </summary>
        public void RefreshAliveFightersPreserveOrder()
        {
            AliveFightersInBattleOrder.Clear();
            foreach (var unit in Units)
            {
                if (unit.IsAlive)
                {
                    AliveFightersInBattleOrder.Add(unit);
                }
            }
            // НЕ вызываем ShuffleAliveFighters() - сохраняем порядок
            CurrentFighterIndex = 0;
        }

        /// <summary>
        /// Заменяет одного юнита другим в армии
        /// </summary>
        public void ReplaceUnit(IUnit oldUnit, IUnit newUnit)
        {
            int index = Units.IndexOf(oldUnit);
            if (index >= 0)
                Units[index] = newUnit;

            int orderIndex = AliveFightersInBattleOrder.IndexOf(oldUnit);
            if (orderIndex >= 0)
                AliveFightersInBattleOrder[orderIndex] = newUnit;
        }
    }
}