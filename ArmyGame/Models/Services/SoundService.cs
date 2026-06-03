using System;
using System.IO;
using System.Media;

namespace ArmyBattle.Models.Services
{
    /// <summary>
    /// Сервис воспроизведения звуков.
    /// SRP: отвечает ТОЛЬКО за воспроизведение звуков, не смешивает логику обработки событий.
    /// </summary>
    public class SoundService
    {
        private static readonly SoundPlayer? _soundPlayer;
        private readonly bool _isEnabled;

        /// <summary>
        /// Статический конструктор, инициализирующий звуковой плеер
        /// </summary>
        static SoundService()
        {
            try
            {
                string fileName = "death_sound.wav";
                string rootPath = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = File.Exists(fileName) ? fileName : Path.Combine(rootPath, fileName);

                if (File.Exists(filePath))
                {
                    _soundPlayer = new SoundPlayer(filePath);
                    _soundPlayer.Load();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Не удалось загрузить звук: {ex.Message}");
            }
        }

        public SoundService(bool enabled = true)
        {
            _isEnabled = enabled;
        }

        /// <summary>
        /// Воспроизводит звук смерти
        /// </summary>
        public void PlayDeathSound()
        {
            if (!_isEnabled) return;

            try
            {
                if (_soundPlayer != null)
                    _soundPlayer.Play();
                else
                    PlayBeep();
            }
            catch
            {
                PlayBeep();
            }
        }

        /// <summary>
        /// Запасной вариант воспроизведения звука через системный бип
        /// </summary>
        private void PlayBeep()
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
    }
}
