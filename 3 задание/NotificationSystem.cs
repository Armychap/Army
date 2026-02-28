using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;

namespace NotificationSingletonDemo
{
    // Базовый класс
    public sealed partial class NotificationSystem
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

        // Типы оповещений
        public enum NotificationType
        {
            Information,
            Success,
            Warning,
            Error,
            Critical
        }

        // Приватный конструктор
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

        // Доступ к единственному экземпляру
        public static NotificationSystem Instance => _lazyInstance.Value;

        // Включение - выключение звука
        public bool IsSoundEnabled { get; set; }
    }
}