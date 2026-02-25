using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace AudioSingletonDemo
{
    /// <summary>
    /// Потокобезопасный Singleton для управления звуковыми эффектами
    /// </summary>
    public sealed class AudioManager
    {
        // Ленивая потокобезопасная инициализация Singleton
        private static readonly Lazy<AudioManager> _lazyInstance = 
            new Lazy<AudioManager>(() => new AudioManager());

        // Объект для блокировки потоков при работе с кэшем
        private readonly object _lockObject = new object();
        
        // Словарь для кэширования загруженных звуков
        private readonly Dictionary<string, SoundPlayer> _soundCache = 
            new Dictionary<string, SoundPlayer>();
        
        // Базовая папка со звуками
        private readonly string _soundsFolder;

        /// <summary>
        /// Приватный конструктор (запрещает создание экземпляров извне)
        /// </summary>
        private AudioManager()
        {
            // Определяем путь к папке Sounds
            _soundsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");
            
            // Громкость по умолчанию (0.0 - 1.0)
            Volume = 0.8;
            
            Console.WriteLine("AudioManager инициализирован");
        }

        /// <summary>
        /// Публичное свойство для доступа к экземпляру Singleton
        /// </summary>
        public static AudioManager Instance => _lazyInstance.Value;

        /// <summary>
        /// Громкость звука (0.0 - тихо, 1.0 - громко)
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// Воспроизвести звук клика
        /// </summary>
        public void PlayClick()
        {
            PlaySound("click.wav");
        }

        /// <summary>
        /// Воспроизвести звук успеха
        /// </summary>
        public void PlaySuccess()
        {
            PlaySound("success.wav");
        }

        /// <summary>
        /// Воспроизвести звук ошибки
        /// </summary>
        public void PlayError()
        {
            PlaySound("error.wav");
        }

        /// <summary>
        /// Воспроизвести звук по имени файла
        /// </summary>
        /// <param name="fileName">Имя .wav файла</param>
        public void PlaySound(string fileName)
        {
            // Запускаем асинхронно, чтобы не блокировать UI
            Task.Run(() => PlaySoundInternal(fileName));
        }

        /// <summary>
        /// Внутренний метод воспроизведения (работает в фоновом потоке)
        /// </summary>
        private void PlaySoundInternal(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_soundsFolder, fileName);
                
                // Проверяем существование файла
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Файл не найден: {filePath}");
                    SimulateSound(fileName); // Запасной вариант - имитация
                    return;
                }

                // Используем кэширование для производительности
                SoundPlayer player;
                lock (_lockObject)
                {
                    if (!_soundCache.TryGetValue(fileName, out player))
                    {
                        player = new SoundPlayer(filePath);
                        player.LoadAsync(); // Загружаем асинхронно
                        _soundCache[fileName] = player;
                    }
                }

                Console.WriteLine($"Воспроизведение: {fileName} (громкость: {Volume:P0})");
                player.PlaySync(); // PlaySync блокирует поток до окончания воспроизведения
                Console.WriteLine($"Звук {fileName} завершён");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка воспроизведения {fileName}: {ex.Message}");
                SimulateSound(fileName);
            }
        }

        /// <summary>
        /// Имитация звука (если файл не найден)
        /// </summary>
        private void SimulateSound(string fileName)
        {
            Console.WriteLine($"Имитация звука: {fileName}");
            
            // Имитируем длительность звука
            int duration = fileName switch
            {
                "click.wav" => 200,
                "success.wav" => 500,
                "error.wav" => 800,
                _ => 300
            };
            
            System.Threading.Thread.Sleep(duration);
        }

        /// <summary>
        /// Предзагрузка всех звуков в кэш
        /// </summary>
        public void PreloadAllSounds()
        {
            Console.WriteLine("Предзагрузка звуков...");
            
            var soundFiles = new[] { "click.wav", "success.wav", "error.wav" };
            
            foreach (var file in soundFiles)
            {
                string filePath = Path.Combine(_soundsFolder, file);
                if (File.Exists(filePath))
                {
                    var player = new SoundPlayer(filePath);
                    player.Load();
                    
                    lock (_soundCache)
                    {
                        _soundCache[file] = player;
                    }
                    
                    Console.WriteLine($"Загружен: {file}");
                }
            }
            
            Console.WriteLine("Предзагрузка завершена");
        }

        /// <summary>
        /// Очистка кэша звуков
        /// </summary>
        public void ClearCache()
        {
            lock (_soundCache)
            {
                foreach (var player in _soundCache.Values)
                {
                    player.Stop();
                    player.Dispose();
                }
                
                _soundCache.Clear();
                Console.WriteLine("Кэш звуков очищен");
            }
        }
    }
}