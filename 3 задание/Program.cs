using System;
using System.Threading;
using System.Threading.Tasks;

namespace AudioSingletonDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Получаем экземпляр AudioManager
            AudioManager audio1 = AudioManager.Instance;
            AudioManager audio2 = AudioManager.Instance;
            
            // Проверяем, что это действительно один и тот же объект
            Console.WriteLine($"audio1 и audio2 - это один объект? {ReferenceEquals(audio1, audio2)}\n");
            
            // Меняем громкость через один экземпляр
            audio1.Volume = 0.5;
            Console.WriteLine($"Громкость через audio2: {audio2.Volume:P0}\n");
            
            // Предзагружаем звуки для быстрого воспроизведения
            audio1.PreloadAllSounds();
            Console.WriteLine();
            
            // Демонстрация работы в разных потоках
            Console.WriteLine("Начинаем воспроизведение звуков...\n");
            
            // Поток 1 - пытается играть звуки
            Task.Run(() => {
                for (int i = 0; i < 3; i++)
                {
                    AudioManager.Instance.PlayClick();
                    Thread.Sleep(100);
                }
            });
            
            // Поток 2 - тоже пытается играть звуки
            Task.Run(() => {
                Thread.Sleep(150); // Небольшая задержка
                AudioManager.Instance.PlaySuccess();
                AudioManager.Instance.PlayError();
            });
            
            // Главный поток
            AudioManager.Instance.PlayClick();
            AudioManager.Instance.PlaySuccess();
            
            Console.WriteLine("\nНажмите любую клавишу для завершения...");
            Console.ReadKey();
            
            // Очистка ресурсов
            audio1.ClearCache();
        }
    }
}