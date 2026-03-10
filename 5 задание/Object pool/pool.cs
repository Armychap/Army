using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectPoolBulletExample
{
    // Класс пули - представляет объект, который можно переиспользовать в пуле
    public class Bullet
    {
        public int Id { get; private set; }
        public string BulletType { get; set; } = "Обычная";
        public int Damage { get; set; }
        public bool IsActive { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 TargetPosition { get; set; }
        public float Speed { get; set; }
        
        private static int _globalIdCounter = 0;

        public Bullet()
        {
            Id = Interlocked.Increment(ref _globalIdCounter);
            IsActive = false;
            
            Console.WriteLine($"[СИСТЕМА] Создана новая пуля #{Id}");
            Thread.Sleep(5);
        }

        // Инициализация пули перед выстрелом - устанавливает все параметры для нового использования
        public void Initialize(string bulletType, int damage, Vector3 startPos, Vector3 targetPos, float speed = 10f)
        {
            BulletType = bulletType;
            Damage = damage;
            Position = startPos;
            TargetPosition = targetPos;
            Speed = speed;
            IsActive = true;
            
            Console.WriteLine($"Пуля #{Id} [{BulletType}] Урон: {Damage}. " +
                            $"{Position.x} -> {TargetPosition.x}");
        }

        // Симуляция полета пули - асинхронный метод, имитирующий движение
        public async Task FlyAsync(float distance)
        {
            if (!IsActive) return;

            Console.WriteLine($"Пуля #{Id} летит...");
            
            float flightTime = distance / Speed;
            float elapsed = 0;
            
            while (elapsed < flightTime)
            {
                float t = elapsed / flightTime;
                float newX = Position.x + (TargetPosition.x - Position.x) * t;
                float newY = Position.y + (TargetPosition.y - Position.y) * t;
                Position = new Vector3(newX, newY);
                
                if (Math.Abs(t - 0.3) < 0.01 || Math.Abs(t - 0.7) < 0.01)
                {
                    Console.WriteLine($"Пуля #{Id} пролетает ({Position.x:F1})");
                }
                
                await Task.Delay(30);
                elapsed += 0.03f;
            }
            
            Position = TargetPosition;
        }

        // Попадание в цель - завершает активность пули
        public void Hit()
        {
            if (!IsActive) return;
            
            string hitEffect = BulletType switch
            {
                "Разрывная" => "взрыв",
                "Бронебойная" => "пробитие",
                "Трассирующая" => "светится",
                _ => "попадание"
            };
            
            Console.WriteLine($"Пуля #{Id} попала! {hitEffect}");
            IsActive = false;
        }

        // Сброс состояния для возврата в пул - очищает все параметры для повторного использования
        public void Reset()
        {
            IsActive = false;
            BulletType = "Обычная";
            Damage = 0;
            Position = Vector3.Zero;
            TargetPosition = Vector3.Zero;
            Speed = 10f;
        }
    }

    // Структура для представления позиции в 2D пространстве
    public struct Vector3
    {
        public float x, y;
        public static Vector3 Zero => new Vector3(0, 0);
        
        public Vector3(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    // Класс пула пуль - реализует паттерн Object Pool для переиспользования объектов пуль
    // Основная идея: вместо создания новых пуль каждый раз, брать их из пула и возвращать обратно
    public class BulletPool
    {
        // Стек для хранения доступных пуль - LIFO структура для быстрого доступа
        private readonly Stack<Bullet> _pool = new Stack<Bullet>();
        // Максимальный размер пула - ограничивает количество объектов в памяти
        private readonly int _maxSize;
        // Объект для синхронизации доступа к пулу в многопоточной среде
        private readonly object _lock = new object();

        // Количество доступных пуль в пуле
        public int AvailableCount => _pool.Count;
        // Общее количество созданных пуль (включая те, что вне пула)
        public int TotalCreated { get; private set; }
        // Общее количество использованных пуль
        public int TotalBulletsUsed { get; private set; }

        // Конструктор пула - создает начальный набор пуль
        public BulletPool(int maxSize)
        {
            _maxSize = maxSize;
            TotalCreated = 0;
            
            Console.WriteLine($"[ПУЛ] Создание {maxSize} пуль...");
            
            for (int i = 0; i < maxSize; i++)
            {
                _pool.Push(new Bullet());
                TotalCreated++;
            }
            
            Console.WriteLine($"[ПУЛ] Готово\n");
        }

        // Метод получения пули из пула - если пул пуст, создает новую пулю
        public Bullet Get()
        {
            lock (_lock)
            {
                TotalBulletsUsed++;
                
                if (_pool.Count > 0)
                {
                    var bullet = _pool.Pop();
                    Console.WriteLine($"[ПУЛ] Выдана пуля #{bullet.Id}");
                    return bullet;
                }
                
                Console.WriteLine($"[ПУЛ] Пул пуст! Создаем новую");
                TotalCreated++;
                return new Bullet();
            }
        }

        // Метод возврата пули в пул - сбрасывает состояние и помещает обратно в стек
        public void Return(Bullet bullet)
        {
            if (bullet == null) return;
            
            lock (_lock)
            {
                if (_pool.Count < _maxSize)
                {
                    bullet.Reset();
                    _pool.Push(bullet);
                    Console.WriteLine($"[ПУЛ] Пуля #{bullet.Id} возвращена");
                }
                else
                {
                    Console.WriteLine($"[ПУЛ] Пул полон. Пуля #{bullet.Id} уничтожена");
                }
            }
        }
    }

    // Класс бойца - представляет игрока, который может стрелять
    public class Player
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public Vector3 Position { get; set; }
        public int Damage { get; set; }
        public string AmmoType { get; set; }
        
        private static int _playerCounter = 0;
        private readonly Random _random = new Random();

        public Player(string name, Vector3 position, int damage, string ammoType)
        {
            Id = Interlocked.Increment(ref _playerCounter);
            Name = name;
            Position = position;
            Damage = damage;
            AmmoType = ammoType;
        }

        // Метод стрельбы по противнику - получает пулю из пула, инициализирует, симулирует полет и возвращает в пул
        public async Task ShootAt(Player enemy, BulletPool pool)
        {
            float distance = Math.Abs(enemy.Position.x - Position.x);
            
            Console.WriteLine($"\n{Name} -> {enemy.Name} ({distance}м)");
            
            var bullet = pool.Get(); // Получаем пулю из пула
            int bonusDamage = _random.Next(0, 3);
            bullet.Initialize(AmmoType, Damage + bonusDamage, Position, enemy.Position);
            
            await bullet.FlyAsync(distance);
            
            bullet.Hit();
            pool.Return(bullet); // Возвращаем пулю в пул для повторного использования
        }
    }

    // Главный класс программы - демонстрирует работу пула пуль в симуляции боя
    class Program
    {
        // Главный метод - точка входа в программу
        static async Task Main(string[] args)
        {
            // Настройки симуляции
            const int poolSize = 10; // Размер пула пуль
            const int playersPerTeam = 4; // Количество бойцов в команде
            const int roundsCount = 3; // Количество раундов боя

            // Создание пула пуль - здесь инициализируется пул с заданным размером
            var bulletPool = new BulletPool(poolSize);

            // Создание команд бойцов
            var teamFirst = new List<Player>();
            var teamSecond = new List<Player>();

            Console.WriteLine("Команды:");
            
            // Создание первой команды с разными типами боеприпасов
            string[] firstAmmo = { "Бронебойная", "Разрывная", "Обычная", "Трассирующая" };
            
            for (int i = 0; i < playersPerTeam; i++)
            {
                teamFirst.Add(new Player(
                    $"Боец {i + 1}", 
                    new Vector3(10, i * 3), 
                    20 + i * 2, 
                    firstAmmo[i]
                ));
            }

            // Создание второй команды
            string[] secondAmmo = { "Разрывная", "Бронебойная", "Трассирующая", "Обычная" };
            
            for (int i = 0; i < playersPerTeam; i++)
            {
                teamSecond.Add(new Player(
                    $"Боец {i + 5}", 
                    new Vector3(50, i * 3), 
                    18 + i * 2, 
                    secondAmmo[i]
                ));
            }

            Console.WriteLine($"Всего: {teamFirst.Count + teamSecond.Count} бойцов\n");

            // Симуляция боя - здесь демонстрируется работа пула в многопоточной среде
            Console.WriteLine("=== БОЙ ===");
            
            var stopwatch = Stopwatch.StartNew(); // Замер времени выполнения
            int totalShots = 0; // Счетчик выстрелов

            for (int round = 0; round < roundsCount; round++)
            {
                Console.WriteLine($"\nРаунд {round + 1}");
                
                var tasks = new List<Task>(); // Список асинхронных задач для параллельного выполнения
                var random = new Random();

                // Первая команда стреляет по второй
                foreach (var attacker in teamFirst)
                {
                    var target = teamSecond[random.Next(teamSecond.Count)];
                    tasks.Add(attacker.ShootAt(target, bulletPool)); // Каждый выстрел использует пул
                    totalShots++;
                }

                // Вторая команда стреляет по первой
                foreach (var attacker in teamSecond)
                {
                    var target = teamFirst[random.Next(teamFirst.Count)];
                    tasks.Add(attacker.ShootAt(target, bulletPool)); // Каждый выстрел использует пул
                    totalShots++;
                }

                await Task.WhenAll(tasks); // Ожидание завершения всех выстрелов в раунде
                Console.WriteLine($"\nРаунд {round + 1} окончен");
                await Task.Delay(1000); // Пауза между раундами
            }

            stopwatch.Stop();

            // Вывод итоговой статистики - показывает эффективность использования пула
            Console.WriteLine($"\n=== ИТОГИ ===");
            Console.WriteLine($"Время: {stopwatch.ElapsedMilliseconds} мс");
            Console.WriteLine($"Выстрелов: {totalShots}");
            Console.WriteLine($"Пул: {poolSize} пуль");
            Console.WriteLine($"Создано пуль: {bulletPool.TotalCreated}");
            Console.WriteLine($"Использовано: {bulletPool.TotalBulletsUsed}");
            Console.WriteLine($"Осталось: {bulletPool.AvailableCount}");
            Console.ReadKey();
        }
    }
}