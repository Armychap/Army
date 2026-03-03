using System;
using System.Collections.Generic;
using System.Linq;

namespace NotificationSystem
{
    public sealed class NotificationManager
    {
        private static NotificationManager _instance;
        private List<Notification> _notifications;

        private NotificationManager()
        {
            _notifications = new List<Notification>();
        }

        public static NotificationManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NotificationManager();
                return _instance;
            }
        }

        public void Send(string message, NotificationType type = NotificationType.Info)
        {
            var notification = new Notification(message, type);
            _notifications.Add(notification);
            Console.WriteLine($"\n[{type}] #{notification.Id}: {message}");
        }

        public void ShowAll()
        {
            Console.WriteLine("\nВСЕ ОПОВЕЩЕНИЯ");
            if (_notifications.Count == 0)
            {
                Console.WriteLine("Нет оповещений");
                return;
            }

            foreach (var n in _notifications)
            {
                string readMark = n.IsRead ? " Прочитано " : "Непрочитано";
                Console.WriteLine($"#{n.Id,-3} {readMark,-12} [{n.Type,-7}] {n.Message} ({n.Time:HH:mm:ss})");
            }
            Console.WriteLine($"Всего: {_notifications.Count}");
        }

        public void ShowUnread()
        {
            var unread = _notifications.Where(n => !n.IsRead).ToList();
            Console.WriteLine($"\nНЕПРОЧИТАННЫХ: {unread.Count}");

            if (unread.Count == 0)
            {
                Console.WriteLine("Нет непрочитанных");
                return;
            }

            foreach (var n in unread)
            {
                Console.WriteLine($"#{n.Id} [{n.Type}] {n.Message} ({n.Time:HH:mm:ss})");
            }
        }

        public void MarkAsRead(int id)
        {
            var notification = _notifications.FirstOrDefault(n => n.Id == id);
            if (notification != null)
            {
                notification.IsRead = true;
                Console.WriteLine($"\nУведомление #{id} отмечено как прочитанное");
            }
            else
            {
                Console.WriteLine($"\nУведомление #{id} не найдено");
            }
        }

        public int Count => _notifications.Count;
    }
}