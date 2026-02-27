using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace NotificationSingletonDemo
{

    // Система оповещений, работает в одном экземпляре на всё приложение
    public sealed class NotificationSystem
    {
        // Ленивая загрузка - объект создастся только при первом обращении
        private static readonly Lazy<NotificationSystem> _lazyInstance =
            new Lazy<NotificationSystem>(() => new NotificationSystem());

        // Семафор гарантирует, что одновременно играет только один звук
        private readonly SemaphoreSlim _soundSemaphore = new SemaphoreSlim(1, 1);

        // Блокировка для общих данных (очереди, кэш)
        private readonly object _syncRoot = new object();

        // Кэш загруженных звуков
        private readonly Dictionary<NotificationType, SoundPlayer?> _notificationSounds = [];

        // Путь к папке со звуками
        private readonly string _soundsDirectory;

        // История последних сообщений
        private readonly Queue<string> _notificationHistory = new Queue<string>();


        // оповещения
        public enum NotificationType
        {
            Information,
            Success,
            Warning,
            Error,
            Critical
        }

        private NotificationSystem() //конструктор
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


        // Доступ к единственному экземпляру
        public static NotificationSystem Instance => _lazyInstance.Value;


        // Вкючение - выключение звука
        public bool IsSoundEnabled { get; set; }

        public void SendInformation(string message) => SendNotification(NotificationType.Information, message);
        public void SendSuccess(string message) => SendNotification(NotificationType.Success, message);
        public void SendWarning(string message) => SendNotification(NotificationType.Warning, message);
        public void SendError(string message) => SendNotification(NotificationType.Error, message);
        public void SendCritical(string message) => SendNotification(NotificationType.Critical, message);


        // Отправка оповещения
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

            // запускаем воспроизведение в фоне асинхронно, чтоб не тормозить основную программу
            Task.Run(() => PlaySound(type));
        }


        private async Task PlaySound(NotificationType type)
        {
            // Ждём, пока освободится канал для звука
            await _soundSemaphore.WaitAsync();

            try
            {
                Console.WriteLine($"звук {type} начал играть");

                string fileName = GetSoundFileName(type);
                string fullPath = Path.Combine(_soundsDirectory, fileName);

                // Проверяем существование файла
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"файл {fileName} не найден");
                    return;
                }

                // Берём из кэша или загружаем
                SoundPlayer? player;
                lock (_syncRoot)
                {
                    if (!_notificationSounds.TryGetValue(type, out player) || player == null)
                    {
                        player = new SoundPlayer(fullPath);
                        player.LoadAsync();
                        _notificationSounds[type] = player;
                        Console.WriteLine($"звук {type} загружается");
                    }
                }

                // Даем время на загрузку
                await Task.Delay(100);

                // Играем до конца (проверка на null)
                if (player != null)
                {
                    player.PlaySync();
                    Console.WriteLine($"звук {type} закончил играть");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ошибка со звуком {type}: {ex.Message}");
            }
            finally
            {
                // Освобождаем канал для следующего звука
                _soundSemaphore.Release();
            }
        }

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


        // Показать историю
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


        // Вывести текст
        public void Print(string text) => Console.WriteLine(text);

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
                        lock (_syncRoot)
                        {
                            _notificationSounds[type] = null;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"  - {type} файл отсутствует");
                    lock (_syncRoot)
                    {
                        _notificationSounds[type] = null;
                    }
                }
            }

            Console.WriteLine("Предзагрузка завершена");
        }

        public void Shutdown()
        {
            Console.WriteLine("Остановка системы оповещений");

            lock (_syncRoot)
            {
                foreach (var player in _notificationSounds.Values)
                {
                    if (player != null)
                    {
                        try
                        {
                            player.Stop();
                            player.Dispose();
                        }
                        catch
                        {
                            // Игнорируем ошибки при остановке
                        }
                    }
                }

                _notificationSounds.Clear();
                _notificationHistory.Clear();
            }

            _soundSemaphore.Dispose();
            Console.WriteLine("Система остановлена");
        }
    

        public void RunDemo()
        {
            // Проверка Singleton
            NotificationSystem sameNotifications = NotificationSystem.Instance;
            Print($"Один экземпляр? {ReferenceEquals(this, sameNotifications)}");
            Print("");
            
            // Включаем звук
            IsSoundEnabled = true;
            
            // Предзагружаем звуки
            PreloadSounds();
            Print("");
            
            //главный поток
            Print("Запуск программы");
            SendInformation("Программа запущена");
            SendSuccess("Загрузка завершена");
            
            Print("Нажмите Enter для запуска задач");
            Console.ReadLine();
            
            //два потока
            Print("");
            Print("Все потоки");
            
            // 1 поток 
            Task.Run(() => {
                Print("[Поток 1] Начинаю загрузку");
                
                NotificationSystem.Instance.SendInformation("Файл 1 - загрузка");
                Thread.Sleep(400);
                NotificationSystem.Instance.SendSuccess("Файл 1  - загружен");
                
                NotificationSystem.Instance.SendInformation("Файл 2 - загрузка");
                Thread.Sleep(300);
                NotificationSystem.Instance.SendSuccess("Файл 2 - загружен");
                
                // Предупреждение
                NotificationSystem.Instance.SendWarning("Файл 3 - повреждён, пропускаю");
                
                Print("[Поток 1] Загрузка завершена");
            });
            
            // 2 поток
            Task.Run(() => {
                Thread.Sleep(200);
                Print("[Поток 2] Начинаю обработку");
                
                NotificationSystem.Instance.SendInformation("Обработка - распаковка");
                Thread.Sleep(500);
                NotificationSystem.Instance.SendSuccess("Обработка - распаковано");
                
                // Ошибка
                NotificationSystem.Instance.SendError("Обработка - ошибка в данных");
                Thread.Sleep(300);
                NotificationSystem.Instance.SendSuccess("Обработка - ошибка исправлена");
                
                // Критическая ошибка
                NotificationSystem.Instance.SendCritical("Обработка - сбой системы");
                Thread.Sleep(400);
                NotificationSystem.Instance.SendSuccess("Обработка - восстановлено");
                
                Print("[Поток 2] Обработка завершена");
            });
            
            // главный поток
            Thread.Sleep(300);
            NotificationSystem.Instance.SendInformation("Главный - проверка статуса");
            Thread.Sleep(400);
            NotificationSystem.Instance.SendInformation("Главный - всё работает");
            Thread.Sleep(400);
            NotificationSystem.Instance.SendSuccess("Главный - задача выполнена");
            
            Print("");
            Print("Нажмите Enter для просмотра истории");
            Console.ReadLine();
            
            // Показываем историю
            ShowHistory();
            
            Print("");
            Print("Нажмите Enter для выхода");
            Console.ReadLine();
            
            Shutdown();
        }
    }
}