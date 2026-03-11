using System;
using System.Threading;

namespace LazyInitializationExample
{
    /// <summary>
    /// Тяжёлый объект, инициализация которого "дорогая".
    /// </summary>
    public class HeavyResource
    {
        public Guid Id { get; }
        public DateTime CreatedAt { get; }

        public HeavyResource()
        {
            Console.WriteLine("[HeavyResource] Начало тяжёлой инициализации...");
            // Эмулируем долгую загрузку (например, чтение большого файла/результат сложных вычислений)
            Thread.Sleep(1000);

            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;

            Console.WriteLine("[HeavyResource] Инициализация завершена.");
        }

        public override string ToString()
        {
            return $"HeavyResource {{ Id = {Id}, CreatedAt = {CreatedAt:HH:mm:ss} }}";
        }
    }
}
