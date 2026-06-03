using ArmyBattle.Models.Interfaces;
using ArmyBattle.Models.Services;

namespace ArmyBattle.Models.Observers
{
    /// <summary>
    /// Наблюдатель для воспроизведения звука при смерти юнита.
    /// SRP: отвечает ТОЛЬКО за обработку события смерти, звук воспроизводит сервис.
    /// </summary>
    public class DeathBeepObserver : IUnitObserver
    {
        /// <summary>
        /// Сервис воспроизведения звуков (SRP: работает со звуком)
        /// </summary>
        private readonly SoundService _soundService;

        /// <summary>
        /// Конструктор наблюдателя звука смерти
        /// </summary>
        public DeathBeepObserver(bool enabled = true)
        {
            _soundService = new SoundService(enabled);
        }

        /// <summary>
        /// Обрабатывает событие получения урона (не используется)
        /// </summary>
        public void OnDamageTaken(IUnit unit, int damage, string attackerName, int newHealth)
        {
            // Не реагируем на получение урона
        }

        /// <summary>
        /// Обрабатывает событие смерти юнита - воспроизводит звук
        /// </summary>
        public void OnDeath(IUnit unit, string killerName)
        {
            _soundService.PlayDeathSound();
        }

        /// <summary>
        /// Обрабатывает событие лечения (не используется)
        /// </summary>
        public void OnHealed(IUnit unit, int amount, int newHealth)
        {
            // Не реагируем на лечение
        }
    }
}