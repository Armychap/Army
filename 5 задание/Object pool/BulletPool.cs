using System;
using System.Collections.Generic;

namespace ObjectPoolBulletExample
{
    // набор готовых к использованию пуль
    public class BulletPool
    {
        // Стек для хранения доступных пуль
        private readonly Stack<Bullet> _pool = new Stack<Bullet>();
        // Максимальный размер пула (количество объектов в памяти)
        private readonly int _maxSize;
        // Объект для синхронизации доступа к пулу
        private readonly object _lock = new object();
        // Множество для отслеживания выданных пуль
        private readonly HashSet<Bullet> _issuedBullets = new HashSet<Bullet>();

        // Количество доступных пуль в пуле
        public int AvailableCount => _pool.Count;
        // Общее количество созданных пуль (и те что вне пула)
        public int TotalCreated { get; private set; }
        // Общее количество использованных пуль
        public int TotalBulletsUsed { get; private set; }

        // Конструктор пула (начальный набор пуль)
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

        // Метод получения пули из пула (если пул пуст, создает новую пулю)
        public Bullet Get()
        {
            lock (_lock)
            {
                TotalBulletsUsed++; // считаем, что пул выдан

                if (_pool.Count > 0)
                {
                    var bullet = _pool.Pop(); // берем последнюю
                    _issuedBullets.Add(bullet); // отслеживаем выданную пулю
                    Console.WriteLine($"[ПУЛ] Выдана пуля #{bullet.Id}");
                    return bullet; // возвращаем
                }

                Console.WriteLine($"[ПУЛ] Пул пуст! Создаем новую"); // если пусто, создаем дополнительно
                TotalCreated++;
                var newBullet = new Bullet();
                _issuedBullets.Add(newBullet); // отслеживаем новую пулю
                return newBullet;
            }
        }

        // Метод возврата пули в пул (сбрасывает состояние и помещает обратно в стек)
        public void Return(Bullet bullet)
        {
            if (bullet == null) return; // ничего не делаем с пустым

            lock (_lock) // синхронизация, чтобы не потерять пулю
            {
                if (_issuedBullets.Contains(bullet)) // проверяем, что пуля была выдана
                {
                    _issuedBullets.Remove(bullet); // удаляем из выданных

                    if (_pool.Count < _maxSize)
                    {
                        bullet.Reset(); // сбрасываем состояние
                        _pool.Push(bullet); // кладем обратно
                        Console.WriteLine($"[ПУЛ] Пуля #{bullet.Id} возвращена");
                    }
                    else
                    {
                        Console.WriteLine($"[ПУЛ] Пул полон. Пуля #{bullet.Id} уничтожена");
                    }
                }
                else
                {
                    Console.WriteLine($"[ПУЛ] Попытка вернуть пулю #{bullet.Id}, которая не была выдана или уже возвращена");
                }
            }
        }
    }
}
