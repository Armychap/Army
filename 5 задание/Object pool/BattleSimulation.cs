using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ObjectPoolBulletExample
{
    // основная логика команд и учёта статистики
    // Класс, инкапсулирующий логику симуляции боя и работы пула
    public class BattleSimulation
    {
        private readonly int _poolSize; // сколько хранит пул
        private readonly int _playersPerTeam; // размер команды
        private readonly int _roundsCount; // раундов в матче

        public BattleSimulation(int poolSize = 10, int playersPerTeam = 4, int roundsCount = 3)
        {
            _poolSize = poolSize; // записываем конфиг
            _playersPerTeam = playersPerTeam;
            _roundsCount = roundsCount;
        }

        public async Task RunAsync()
        {
            var bulletPool = new BulletPool(_poolSize); // готовая коллекция пуль

            var teamFirst = CreateTeam(1, "Бронебойная", "Разрывная", "Обычная", "Трассирующая"); // создаём 1ю команду
            var teamSecond = CreateTeam(5, "Разрывная", "Бронебойная", "Трассирующая", "Обычная"); // 2ю

            Console.WriteLine("Бой"); // стартовый текст
            var stopwatch = Stopwatch.StartNew(); // время
            int totalShots = 0; // счетчик выстрелов
            var random = new Random(); // выбор случайного врага

            for (int round = 0; round < _roundsCount; round++)
            {
                Console.WriteLine($"\nРаунд {round + 1}"); // номер раунда

                var tasks = new List<Task>(); // список задач

                foreach (var attacker in teamFirst)
                {
                    var target = teamSecond[random.Next(teamSecond.Count)]; // выбираем случайного
                    tasks.Add(attacker.ShootAt(target, bulletPool)); // стреляем
                    totalShots++; // увеличиваем счетчик
                }

                foreach (var attacker in teamSecond)
                {
                    var target = teamFirst[random.Next(teamFirst.Count)];
                    tasks.Add(attacker.ShootAt(target, bulletPool));
                    totalShots++;
                }

                await Task.WhenAll(tasks); // дождаться всех выстрелов
                Console.WriteLine($"\nРаунд {round + 1} окончен"); // раунд завершён
                await Task.Delay(1000); // небольшая пауза
            }

            stopwatch.Stop(); // остановить таймер
            PrintStats(stopwatch.ElapsedMilliseconds, totalShots, bulletPool); // выводим статистику
        }

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
            Console.WriteLine("\nИтоги"); // заголовок
            Console.WriteLine($"Время: {elapsedMs} мс"); // время работы
            Console.WriteLine($"Выстрелов: {totalShots}"); // количество выстрелов
            Console.WriteLine($"Пул: {_poolSize} пуль"); // размер пула
            Console.WriteLine($"Создано пуль: {pool.TotalCreated}"); // статистика пула
            Console.WriteLine($"Использовано: {pool.TotalBulletsUsed}");
            Console.WriteLine($"Осталось: {pool.AvailableCount}");
            Console.ReadKey(); // ждем нажатия
        }
    }
}