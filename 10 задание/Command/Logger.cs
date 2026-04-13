using System;
using System.Collections.Generic;

namespace ShoppingCartWithUndo
{
    // Интерфейс логгера - отвечает за логирование действий
    public interface ILogger
    {
        void Log(string message);
        void LogCommand(string action, ICommand command);
    }

    // Конкретный логгер в консоль - отвечает за вывод логов в консоль и хранение истории
    public class ConsoleLogger : ILogger
    {
        private readonly List<string> _logs = new List<string>();

        public void Log(string message)
        {
            var logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            _logs.Add(logEntry);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(logEntry);
            Console.ResetColor();
        }

        public void LogCommand(string action, ICommand command)
        {
            Log($"{action}: {command.GetDescription()}");
        }

        public void ShowAnalytics()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== ЛОГ АНАЛИТИКИ ===");
            foreach (var log in _logs)
            {
                Console.WriteLine(log);
            }
            Console.WriteLine($"Всего действий: {_logs.Count}");
            Console.WriteLine("=====================\n");
            Console.ResetColor();
        }
    }
}