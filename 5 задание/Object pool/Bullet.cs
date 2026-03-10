using System;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectPoolBulletExample
{
    public class Bullet
    {
        public int Id { get; private set; } // уникальный идентификатор
        public string BulletType { get; set; } = "Обычная"; // тип пули
        public int Damage { get; set; } // урон
        public bool IsActive { get; private set; } // находится ли в использовании
        public Vector3 Position { get; set; } // текущая позиция
        public Vector3 TargetPosition { get; set; } // цель
        public float Speed { get; set; } // скорость полета

        private static int _globalIdCounter = 0; // счетчик для генерации id

        public Bullet()
        {
            Id = Interlocked.Increment(ref _globalIdCounter); // новый идентификатор
            IsActive = false; // сразу неактивна

            Console.WriteLine($"[СИСТЕМА] Создана новая пуля #{Id}"); // лог создания
            Thread.Sleep(5); // эмулируем небольшую задержку
        }

        public void Initialize(string bulletType, int damage, Vector3 startPos, Vector3 targetPos, float speed = 10f)
        {
            BulletType = bulletType; // сохраняем тип
            Damage = damage; // урон
            Position = startPos; // начальная координата
            TargetPosition = targetPos; // куда летим
            Speed = speed; // скорость полета
            IsActive = true; // активируем пуля

            Console.WriteLine($"Пуля #{Id} [{BulletType}] Урон: {Damage}. " +
                            $"{Position.x} -> {TargetPosition.x}"); // вывод данных
        }

        public async Task FlyAsync(float distance)
        {
            if (!IsActive) return; // только если активна

            Console.WriteLine($"Пуля #{Id} летит..."); // старт полета

            float flightTime = distance / Speed; // сколько займет путь
            float elapsed = 0; // сколько прошли

            while (elapsed < flightTime) // пока не долетели
            {
                float t = elapsed / flightTime; // доля пути
                float newX = Position.x + (TargetPosition.x - Position.x) * t; // ЛКС по X
                float newY = Position.y + (TargetPosition.y - Position.y) * t; // по Y
                Position = new Vector3(newX, newY); // сохраняем новую позицию

                if (Math.Abs(t - 0.3) < 0.01 || Math.Abs(t - 0.7) < 0.01)
                {
                    Console.WriteLine($"Пуля #{Id} пролетает ({Position.x:F1})"); // промежуточный вывод
                }

                await Task.Delay(30); // имитация времени
                elapsed += 0.03f; // увеличиваем прошедшее
            }

            Position = TargetPosition; // пришли в цель
        }

        public void Hit()
        {
            if (!IsActive) return; // если не летела, ничего не делаем

            string hitEffect = BulletType switch
            {
                "Разрывная" => "взрыв",
                "Бронебойная" => "пробитие",
                "Трассирующая" => "светится",
                _ => "попадание"
            }; // эффект в зависимости от типа

            Console.WriteLine($"Пуля #{Id} попала! {hitEffect}"); // сообщаем о попадании
            IsActive = false; // сбрасываем флаг
        }

        public void Reset()
        {
            IsActive = false; // помечаем как свободную
            BulletType = "Обычная"; // возвращаем дефолты
            Damage = 0;
            Position = Vector3.Zero;
            TargetPosition = Vector3.Zero;
            Speed = 10f;
        }
    }
}
