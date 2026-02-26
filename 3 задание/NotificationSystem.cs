using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationSingletonDemo
{
    /// <summary>
    /// Система оповещений, работает в одном экземпляре на всё приложение
    /// </summary>
    public sealed class NotificationSystem
    {
        // Ленивая загрузка - объект создастся только при первом обращении
        private static readonly Lazy<NotificationSystem> _lazyInstance =
            new Lazy<NotificationSystem>(() => new NotificationSystem());

        // Семафор - гарантирует, что одновременно играет только один звук
        private readonly SemaphoreSlim _soundSemaphore = new SemaphoreSlim(1, 1);

        // Блокировка для общих данных (очереди, кэш)
        private readonly object _syncRoot = new object();

        // Кэш загруженных звуков
        private readonly Dictionary<NotificationType, SoundPlayer> _notificationSounds =
            new Dictionary<NotificationType, SoundPlayer>();

        // Путь к папке со звуками
        private readonly string _soundsDirectory;

        // История последних 20 сообщений
        private readonly Queue<string> _notificationHistory = new Queue<string>();

        /// <summary>
        /// Типы оповещений
        /// </summary>
        public enum NotificationType
        {
            Information,
            Success,
            Warning,
            Error,
            Critical
        }

        /// <summary>
        /// Приватный конструктор - чтоб никто не создал второй экземпляр
        /// </summary>
        private NotificationSystem()
        {
            // Ищем папку со звуками
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string soundsPath = Path.Combine(baseDir, "Sounds");
            string notificationsPath = Path.Combine(baseDir, "Notifications");

            if (Directory.Exists(soundsPath))
                _soundsDirectory = soundsPath;
            else
                _soundsDirectory = notificationsPath;

            IsSoundEnabled = true;
            Console.WriteLine("Система оповещений запущена");
        }

        /// <summary>
        /// Доступ к единственному экземпляру
        /// </summary>
        public static NotificationSystem Instance => _lazyInstance.Value;

        /// <summary>
        /// Вкл/Выкл звук
        /// </summary>
        public bool IsSoundEnabled { get; set; }

        public void SendInformation(string message) => SendNotification(NotificationType.Information, message);
        public void SendSuccess(string message) => SendNotification(NotificationType.Success, message);
        public void SendWarning(string message) => SendNotification(NotificationType.Warning, message);
        public void SendError(string message) => SendNotification(NotificationType.Error, message);
        public void SendCritical(string message) => SendNotification(NotificationType.Critical, message);

        /// <summary>
        /// Отправка оповещения
        /// </summary>
        private void SendNotification(NotificationType type, string message)
        {
            // Сохраняем в историю
            lock (_syncRoot)
            {
                string entry = $"{type}: {message}";
                _notificationHistory.Enqueue(entry);

                while (_notificationHistory.Count > 20)
                    _notificationHistory.Dequeue();
            }

            // Показываем в консоли
            Console.WriteLine($"{type}: {message}");

            // Если звук включён - запускаем воспроизведение в фоне
            if (IsSoundEnabled)
            {
                // Асинхронно, чтоб не тормозить основную программу
                Task.Run(() => PlaySound(type));
            }
        }


        /// <summary>
        /// Воспроизвести звук (всегда только один за раз)
        /// </summary>
        private async Task PlaySound(NotificationType type)
        {
            // Ждём, пока освободится канал для звука
            await _soundSemaphore.WaitAsync();

            try
            {
                Console.WriteLine($"звук {type} начал играть");

                string fileName = GetSoundFileName(type);
                string fullPath = Path.Combine(_soundsDirectory, fileName);

                // Берём из кэша или загружаем
                SoundPlayer player;
                lock (_syncRoot)
                {
                    if (!_notificationSounds.TryGetValue(type, out player))
                    {
                        player = new SoundPlayer(fullPath);
                        player.Load();
                        _notificationSounds[type] = player;
                        Console.WriteLine($"звук {type} загружен");
                    }
                }

                // Играем до конца
                player.PlaySync();
                Console.WriteLine($"звук {type} закончил играть");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ошибка со звуком {type}: {ex.Message}");
                SimulateSound(type);
            }
            finally
            {
                // Освобождаем канал для следующего звука
                _soundSemaphore.Release();
            }
        }

        /// <summary>
        /// Имя файла для типа оповещения
        /// </summary>
        private string GetSoundFileName(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Information: return "info.wav";
                case NotificationType.Success: return "success.wav";
                case NotificationType.Warning: return "warning.wav";
                case NotificationType.Error: return "error.wav";
                case NotificationType.Critical: return "critical.wav";
                default: return "info.wav";
            }
        }

        /// <summary>
        /// Имитация звука через Beep
        /// </summary>
        private void SimulateSound(NotificationType type)
        {
            int duration = type switch
            {
                NotificationType.Information => 50,   
                NotificationType.Success => 80,       
                NotificationType.Warning => 100,      
                NotificationType.Error => 150,        
                NotificationType.Critical => 200,       
                _ => 80
            };
        }
        /// <summary>
        /// Показать историю
        /// </summary>
        public void ShowHistory()
        {
            Console.WriteLine("\nИстория оповещений");

            string[] history;
            lock (_syncRoot)
            {
                history = _notificationHistory.ToArray();
            }

            foreach (string entry in history)
                Console.WriteLine(entry);
        }

        /// <summary>
        /// Вывести текст
        /// </summary>
        public void Print(string text) => Console.WriteLine(text);

        /// <summary>
        /// Предзагрузить звуки
        /// </summary>
        public void PreloadSounds()
        {
            if (!IsSoundEnabled) return;

            Console.WriteLine("Предзагрузка звуков");

            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                string fileName = GetSoundFileName(type);
                string fullPath = Path.Combine(_soundsDirectory, fileName);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        var player = new SoundPlayer(fullPath);
                        player.Load();

                        lock (_syncRoot)
                        {
                            _notificationSounds[type] = player;
                        }

                        Console.WriteLine($"  - {type} загружен");
                    }
                    catch
                    {
                        Console.WriteLine($"  - {type} не загрузился");
                    }
                }
                else
                {
                    Console.WriteLine($"  - {type} файл отсутствует");
                }
            }

            Console.WriteLine("Предзагрузка завершена");
        }

        /// <summary>
        /// Остановка системы
        /// </summary>
        public void Shutdown()
        {
            Console.WriteLine("Остановка системы оповещений");

            lock (_syncRoot)
            {
                foreach (var player in _notificationSounds.Values)
                {
                    player.Stop();
                    player.Dispose();
                }

                _notificationSounds.Clear();
                _notificationHistory.Clear();
            }

            _soundSemaphore.Dispose();
            Console.WriteLine("Система остановлена");
        }
    }
}