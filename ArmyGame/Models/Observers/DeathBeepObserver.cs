using System;
using System.IO;
using System.Media;
using ArmyBattle.Models.Interfaces;

namespace ArmyBattle.Models.Observers
{
    /// <summary>
    /// Наблюдатель для воспроизведения звука при смерти юнита
    /// </summary>
    public class DeathBeepObserver : IUnitObserver
    {
        /// <summary>
        /// Плеер для воспроизведения звукового файла
        /// </summary>
        private static readonly SoundPlayer? soundPlayer;
        
        /// <summary>
        /// Флаг включения/выключения звуковых эффектов
        /// </summary>
        private readonly bool isEnabled;

        /// <summary>
        /// Статический конструктор, инициализирующий звуковой плеер
        /// </summary>
        static DeathBeepObserver()
        {
            try
            {
                string fileName = "death_sound.wav";
                string rootPath = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = File.Exists(fileName) ? fileName : Path.Combine(rootPath, fileName);

                if (File.Exists(filePath))
                {
                    soundPlayer = new SoundPlayer(filePath);
                    soundPlayer.Load();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Не удалось загрузить звук смерти: {ex.Message}");
            }
        }

        /// <summary>
        /// Конструктор наблюдателя звука смерти
        /// </summary>
        public DeathBeepObserver(bool enabled = true)
        {
            isEnabled = enabled;
        }

        /// <summary>
        /// Воспроизводит звук смерти
        /// </summary>
        private void PlayDeathSound()
        {
            if (!isEnabled) return;
            
            try
            {
                if (soundPlayer != null)
                    soundPlayer.Play();
                else
                    BeepFallback();
            }
            catch
            {
                BeepFallback();
            }
        }

        /// <summary>
        /// Запасной вариант воспроизведения звука через системный бип
        /// </summary>
        private void BeepFallback()
        {
            try
            {
                Console.Beep(400, 150);
                Console.Beep(600, 150);
                Console.Beep(400, 200);
            }
            catch
            {
                // Игнорируем ошибки звука
            }
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
            PlayDeathSound();
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