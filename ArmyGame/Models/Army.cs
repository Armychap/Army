using System;
using System.Collections.Generic;

namespace ArmyBattle.Models
{
    // Класс армии
    public class Army
    {
        public string Name { get; set; }
        public string Prefix { get; set; }
        public List<Unit> Units { get; set; }
        public ConsoleColor Color { get; set; }
        public int TotalCost { get; set; }
        
        // Список живых бойцов в случайном порядке для боя
        public List<Unit> AliveFightersInBattleOrder { get; set; }
        public int CurrentFighterIndex { get; set; }

        private static Random random = new Random();

        public Army(string name, string prefix, ConsoleColor color)
        {
            Name = name;
            Prefix = prefix;
            Color = color;
            Units = new List<Unit>();
            AliveFightersInBattleOrder = new List<Unit>();
            TotalCost = 0;
            CurrentFighterIndex = 0;
        }

        // Добавление юнита в армию
        public void AddUnit(Unit unit)
        {
            Units.Add(unit);
            TotalCost += unit.Cost;
        }

        // Перемешивание живых бойцов в случайном порядке для боя
        public void ShuffleAliveFighters()
        {
            AliveFightersInBattleOrder.Clear();
            
            // Собираем всех живых бойцов
            foreach (var unit in Units)
            {
                if (unit.IsAlive)
                {
                    AliveFightersInBattleOrder.Add(unit);
                }
            }
            
            // Перемешиваем список живых бойцов
            for (int i = AliveFightersInBattleOrder.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                Unit temp = AliveFightersInBattleOrder[i];
                AliveFightersInBattleOrder[i] = AliveFightersInBattleOrder[j];
                AliveFightersInBattleOrder[j] = temp;
            }
            
            CurrentFighterIndex = 0;
        }

        // Получение следующего бойца в случайном порядке
        public Unit? GetNextFighterInBattleOrder()
        {
            if (CurrentFighterIndex < AliveFightersInBattleOrder.Count)
            {
                Unit nextFighter = AliveFightersInBattleOrder[CurrentFighterIndex];
                CurrentFighterIndex++;
                return nextFighter;
            }
            
            return null;
        }

        // Удаление убитого бойца из списка живых
        public void RemoveDeadFighter(Unit deadFighter)
        {
            // Найдём индекс удаляемого бойца в текущем порядке боя
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
            return AliveFightersInBattleOrder.Count;
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
                    Console.WriteLine($"  {unit.GetDisplayName(Prefix)} ({unit.PowerLevel}) - {status}");
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
            var availableFighters = new List<Tuple<int, Func<int, Unit>>>
            {
                new Tuple<int, Func<int, Unit>>(40, (num) => new StrongFighter(num)),
                new Tuple<int, Func<int, Unit>>(15, (num) => new WeakFighter(num))
            };
            
            // Пока есть бюджет на любого бойца
            while (remainingBudget >= 15)
            {
                // Выбираем случайного бойца, которого можем себе позволить
                var affordableFighters = new List<Tuple<int, Func<int, Unit>>>();
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
                
                Unit newUnit = selectedFighter.Item2(fighterNumber);
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
    }
}