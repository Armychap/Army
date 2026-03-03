using System;

namespace NotificationSystem
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class Notification
    {
        private static int _nextId = 1;

        public int Id { get; }
        public string Message { get; }
        public NotificationType Type { get; }
        public bool IsRead { get; set; }
        public DateTime Time { get; }

        public Notification(string message, NotificationType type)
        {
            Id = _nextId++;
            Message = message;
            Type = type;
            Time = DateTime.Now;
            IsRead = false;
        }
    }
}