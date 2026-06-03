using System;
using System.IO;

namespace ArmyBattle.Models.Services
{
    /// <summary>
    /// Сервис логирования событий в файлы.
    /// SRP: отвечает ТОЛЬКО за запись логов в файл, не смешивает логику обработки событий.
    /// </summary>
    public class LoggingService
    {
        private readonly string _logDirectory;
        private readonly string _logFile;
        private readonly bool _isEnabled;

        public LoggingService(string logDirectory, string logFileName, bool enabled = true)
        {
            _logDirectory = logDirectory;
            _logFile = Path.Combine(logDirectory, logFileName);
            _isEnabled = enabled;

            if (_isEnabled)
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Записывает строку в лог-файл
        /// </summary>
        public void Log(string message)
        {
            if (!_isEnabled) return;

            try
            {
                string timestampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(_logFile, timestampedMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[LOG ERROR] {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
