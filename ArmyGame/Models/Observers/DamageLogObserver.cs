using ArmyBattle.Models.Interfaces;
using ArmyBattle.Models.Services;

namespace ArmyBattle.Models.Observers
{
    /// <summary>
    /// Наблюдатель для логирования урона в файл.
    /// SRP: отвечает ТОЛЬКО за обработку событий, логирование делегирует сервису.
    /// </summary>
    public class DamageLogObserver : IUnitObserver
    {
        /// <summary>
        /// Сервис логирования (SRP: работает с файловой системой)
        /// </summary>
        private readonly LoggingService _loggingService;

        /// <summary>
        /// Конструктор наблюдателя логирования урона
        /// </summary>
        public DamageLogObserver(bool enabled = true)
        {
            _loggingService = new LoggingService("Logs", "unit_damage.log", enabled);
        }

        /// <summary>
        /// Обрабатывает событие получения урона юнитом
        /// </summary>
        public void OnDamageTaken(IUnit unit, int damage, string attackerName, int newHealth)
        {
            string message = $"{unit.Army?.Name ?? "Unknown"} Боец {unit.FighterNumber}: {damage} урона от {attackerName}. HP {System.Math.Max(newHealth, 0)}/{unit.MaxHealth}";
            _loggingService.Log(message);
        }

        /// <summary>
        /// Обрабатывает событие смерти юнита
        /// </summary>
        public void OnDeath(IUnit unit, string killerName)
        {
            string message = $"{unit.Army?.Name ?? "Unknown"} Боец {unit.FighterNumber} убит {killerName}";
            _loggingService.Log(message);
        }

        /// <summary>
        /// Обрабатывает событие лечения юнита
        /// </summary>
        public void OnHealed(IUnit unit, int amount, int newHealth)
        {
            string message = $"{unit.Army?.Name ?? "Unknown"} Боец {unit.FighterNumber}: вылечен на {amount}. HP {newHealth}/{unit.MaxHealth}";
            _loggingService.Log(message);
        }
    }
}