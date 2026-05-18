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
        private static readonly SoundPlayer? soundPlayer;
        private readonly bool isEnabled;

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

        public DeathBeepObserver(bool enabled = true)
        {
            isEnabled = enabled;
        }

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

        public void OnDamageTaken(IUnit unit, int damage, string attackerName, int newHealth)
        {
            // Не реагируем на получение урона
        }

        public void OnDeath(IUnit unit, string killerName)
        {
            PlayDeathSound();
        }

        public void OnHealed(IUnit unit, int amount, int newHealth)
        {
            // Не реагируем на лечение
        }
    }
}