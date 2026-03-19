using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ObjectPoolBulletExample
{
    // Класс, инкапсулирующий логику симуляции боя и работы пула
    public class BattleSimulation
    {
        private readonly int _poolSize; // сколько хранит пул
        private readonly int _playersPerTeam; // размер команды
        private readonly int _roundsCount; // раундов в матче

        public BattleSimulation(int poolSize = 10, int playersPerTeam = 4, int roundsCount = 1)
        {
            _poolSize = poolSize;
            _playersPerTeam = playersPerTeam;
            _roundsCount = roundsCount;
        }

        // Метод для запуска симуляции боя
        public async Task RunAsync()
        {
            var bulletPool = new BulletPool(_poolSize); // готовая коллекция пуль

            var teamFirst = CreateTeam(1, "Бронебойная", "Разрывная", "Обычная", "Трассирующая"); // первая команда
            var teamSecond = CreateTeam(5, "Разрывная", "Бронебойная", "Трассирующая", "Обычная"); // вторая команда

            Console.WriteLine("Бой");
            var stopwatch = Stopwatch.StartNew(); // время
            int totalShots = 0; // счетчик выстрелов
            var random = new Random(); // выбор случайного врага

            for (int round = 0; round < _roundsCount; round++)
            {
                Console.WriteLine($"\nРаунд {round + 1}");

                var tasks = new List<Task>(); // список задач 

                foreach (var attacker in teamFirst)
                {
                    var target = teamSecond[random.Next(teamSecond.Count)]; // выбираем случайного
                    tasks.Add(attacker.ShootAt(target, bulletPool)); // стреляем
                    totalShots++; // увеличиваем счетчик
                }

                foreach (var attacker in teamSecond) // повторяем для второй команды
                {
                    var target = teamFirst[random.Next(teamFirst.Count)];
                    tasks.Add(attacker.ShootAt(target, bulletPool));
                    totalShots++;
                }

                await Task.WhenAll(tasks); // дождаться всех выстрелов
                Console.WriteLine($"\nРаунд {round + 1} окончен"); 
                await Task.Delay(1000); //  пауза
            }

            stopwatch.Stop(); // остановить таймер
            PrintStats(stopwatch.ElapsedMilliseconds, totalShots, bulletPool); // выводим статистику
        }

        // Метод для создания команды с заданными типами снарядов
        private List<Player> CreateTeam(int startIndex, params string[] ammoTypes)
        {
            var team = new List<Player>(); // новый список
            for (int i = 0; i < _playersPerTeam; i++)
            {
                team.Add(new Player(
                    $"Боец {startIndex + i}", // имя
                    new Vector3(startIndex == 1 ? 10 : 50, i * 3), // позиция
                    startIndex == 1 ? 20 + i * 2 : 18 + i * 2, // урон
                    ammoTypes[i] // тип снаряда
                ));
            }

            Console.WriteLine($"Создана команда {startIndex} из {team.Count} бойцов"); // лог
            return team; // возвращаем сформированный список
        }

        private void PrintStats(long elapsedMs, int totalShots, BulletPool pool)
        {
            Console.WriteLine("\nИтоги"); 
            Console.WriteLine($"Время: {elapsedMs} мс"); 
            Console.WriteLine($"Выстрелов: {totalShots}"); 
            Console.WriteLine($"Пул: {_poolSize} пуль"); // размер пула
            Console.WriteLine($"Создано пуль: {pool.TotalCreated}"); 
            Console.WriteLine($"Использовано: {pool.TotalBulletsUsed}");
            Console.WriteLine($"Осталось: {pool.AvailableCount}");
            Console.ReadKey(); 
        }
    }
}