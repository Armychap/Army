using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectPoolBulletExample
{
    // Игрок/боец, умеющий стрелять в другого.
    // Сам по себе он простой — состояние, позиция и тип снаряда.
    // В пул передается только в момент выстрела, чтобы снизить связность.
    // Класс бойца (игрок, который может стрелять)
    public class Player
    {
        public string Name { get; private set; }
        public Vector3 Position { get; set; }
        public int Damage { get; set; }
        public string AmmoType { get; set; }

        private readonly Random _random = new Random();

        public Player(string name, Vector3 position, int damage, string ammoType)
        {
            Name = name;
            Position = position;
            Damage = damage;
            AmmoType = ammoType;
        }

        // Метод стрельбы по противнику (получает пулю из пула, инициализирует, симулирует полет и возвращает в пул)
        public async Task ShootAt(Player enemy, BulletPool pool)
        {
            // вычисляем расстояние до цели по оси X
            float distance = Math.Abs(enemy.Position.x - Position.x);

            Console.WriteLine($"\n{Name} -> {enemy.Name} ({distance}м)"); // выводим действие

            var bullet = pool.Get(); // берём пулю из пула
            int bonusDamage = _random.Next(0, 3); // рандомный бонус
            bullet.Initialize(AmmoType, Damage + bonusDamage, Position, enemy.Position); // настраиваем пулю

            await bullet.FlyAsync(distance); // моделируем полёт

            bullet.Hit(); // сообщаем о попадании
            pool.Return(bullet); // возвращаем пулю в пул для повторного использования
        }
    }
}