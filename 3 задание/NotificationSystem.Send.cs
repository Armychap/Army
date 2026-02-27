using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationSingletonDemo
{
    public sealed partial class NotificationSystem
    {
        // Публичные методы отправки
        public void SendInformation(string message) => SendNotification(NotificationType.Information, message);
        public void SendSuccess(string message) => SendNotification(NotificationType.Success, message);
        public void SendWarning(string message) => SendNotification(NotificationType.Warning, message);
        public void SendError(string message) => SendNotification(NotificationType.Error, message);
        public void SendCritical(string message) => SendNotification(NotificationType.Critical, message);

        // Приватный метод отправки
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

            // Запускаем воспроизведение в фоне
            Task.Run(() => PlaySound(type));
        }
    }
}