using System;
using System.Threading.Tasks;

namespace LazyInitializationExample
{
    /// <summary>
    /// Боец, у которого есть тяжёлый ресурс (например, полная экипировка),
    /// инициализируемый только при первом обращении.
    /// </summary>
    public class Soldier
    {
        public string Name { get; }

        // Здесь демонстрируем использование собственного Lazy<T>
        private readonly Lazy<HeavyResource> _equipment;

        public Soldier(string name)
        {
            Name = name;
            _equipment = new Lazy<HeavyResource>(() => new HeavyResource());
        }

        public bool IsEquipmentLoaded => _equipment.IsValueCreated;

        public HeavyResource Equipment => _equipment.Value;

        public async Task InspectAsync()
        {
            Console.WriteLine($"\n[{Name}] Проверяет, загружено ли снаряжение...");
            Console.WriteLine($"  Снаряжение уже создано? {IsEquipmentLoaded}");

            await Task.Delay(300);

            Console.WriteLine($"[{Name}] Требуется доступ к снаряжению. Запрашиваем Equipment...");
            var value = Equipment;
            Console.WriteLine($"[{Name}] Получено снаряжение: {value}");
            Console.WriteLine($"  Снаряжение уже создано? {IsEquipmentLoaded}");
        }
    }
}
