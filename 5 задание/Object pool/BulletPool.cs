using System;
using System.Collections.Generic;

namespace ObjectPoolBulletExample
{
    // Класс реализует пул — набор готовых к использованию пуль.
    // Сразу создаем ограниченное количество экземпляров и выдаем по запросу.
    // Когда они больше не нужны, сбрасываем и кладем обратно.
    // Это помогает избежать частого выделения памяти в горячем цикле.
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

        // Метод получения пули из пула (если пул пуст, создает новую пулю)
        public Bullet Get()
        {
            lock (_lock) // блокируем доступ так, чтобы из разных потоков было безопасно
            {
                TotalBulletsUsed++; // считаем, что пул выдан

                if (_pool.Count > 0)
                {
                    var bullet = _pool.Pop(); // берем последнюю
                    Console.WriteLine($"[ПУЛ] Выдана пуля #{bullet.Id}");
                    return bullet; // возвращаем клиенту
                }

                Console.WriteLine($"[ПУЛ] Пул пуст! Создаем новую"); // если пусто, создаем дополнительно
                TotalCreated++;
                return new Bullet();
            }
        }

        // Метод возврата пули в пул (сбрасывает состояние и помещает обратно в стек)
        public void Return(Bullet bullet)
        {
            if (bullet == null) return; // ничего не делаем с пустым

            lock (_lock) // синхронизация, чтобы не потерять пулю
            {
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
        }
    }
}