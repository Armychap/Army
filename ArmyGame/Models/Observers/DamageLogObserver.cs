using System;
using System.IO;
using ArmyBattle.Models.Interfaces;

namespace ArmyBattle.Models.Observers
{
    /// <summary>
    /// Наблюдатель для логирования урона в файл
    /// </summary>
    public class DamageLogObserver : IUnitObserver
    {
        /// <summary>
        /// Директория для хранения логов
        /// </summary>
        private static readonly string logDirectory = "Logs";
        
        /// <summary>
        /// Путь к файлу лога урона
        /// </summary>
        private static readonly string logFile = Path.Combine(logDirectory, "unit_damage.log");
        
        /// <summary>
        /// Флаг включения/выключения логирования
        /// </summary>
        private readonly bool isEnabled;

        /// <summary>
        /// Конструктор наблюдателя логирования урона
        /// </summary>
        public DamageLogObserver(bool enabled = true)
        {
            isEnabled = enabled;
            if (isEnabled)
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        /// <summary>
        /// Обрабатывает событие получения урона юнитом
        /// </summary>
        public void OnDamageTaken(IUnit unit, int damage, string attackerName, int newHealth)
        {
            if (!isEnabled) return;
            
            try
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {unit.Army?.Name ?? "Unknown"} Боец {unit.FighterNumber}: {damage} урона от {attackerName}. HP {Math.Max(newHealth, 0)}/{unit.MaxHealth}";
                File.AppendAllText(logFile, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[LOG ERROR] {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Обрабатывает событие смерти юнита
        /// </summary>
        public void OnDeath(IUnit unit, string killerName)
        {
            if (!isEnabled) return;
            
            try
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {unit.Army?.Name ?? "Unknown"} Боец {unit.FighterNumber} убит {killerName}";
                File.AppendAllText(logFile, line + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[LOG ERROR] {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Обрабатывает событие лечения юнита
        /// </summary>
        public void OnHealed(IUnit unit, int amount, int newHealth)
        {
            if (!isEnabled) return;
            
            try
            {
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {unit.Army?.Name ?? "Unknown"} Боец {unit.FighterNumber}: вылечен на {amount}. HP {newHealth}/{unit.MaxHealth}";
                File.AppendAllText(logFile, line + Environment.NewLine);
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