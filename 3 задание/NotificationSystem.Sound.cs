using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationSingletonDemo
{
    public sealed partial class NotificationSystem
    {
        // Воспроизведение звука
        private async Task PlaySound(NotificationType type)
        {
            await _soundSemaphore.WaitAsync();

            try
            {
                Console.WriteLine($"звук {type} начал играть");

                string fileName = GetSoundFileName(type);
                string fullPath = Path.Combine(_soundsDirectory, fileName);

                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"файл {fileName} не найден");
                    return;
                }

                SoundPlayer? player;
                lock (_syncRoot)
                {
                    if (!_notificationSounds.TryGetValue(type, out player) || player == null)
                    {
                        player = new SoundPlayer(fullPath);
                        player.LoadAsync();
                        _notificationSounds[type] = player;
                        Console.WriteLine($"звук {type} загружается");
                    }
                }

                await Task.Delay(100);

                if (player != null)
                {
                    player.PlaySync();
                    Console.WriteLine($"звук {type} закончил играть");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ошибка со звуком {type}: {ex.Message}");
            }
            finally
            {
                _soundSemaphore.Release();
            }
        }

        // Имя файла для типа оповещения
        private string GetSoundFileName(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Information: return "info.wav";
                case NotificationType.Success: return "success.wav";
                case NotificationType.Warning: return "warning.wav";
                case NotificationType.Error: return "error.wav";
                case NotificationType.Critical: return "critical.wav";
                default: return "info.wav";
            }
        }

        // Предзагрузка звуков
        public void PreloadSounds()
        {
            if (!IsSoundEnabled) return;

            Console.WriteLine("Предзагрузка звуков");

            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                string fileName = GetSoundFileName(type);
                string fullPath = Path.Combine(_soundsDirectory, fileName);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        var player = new SoundPlayer(fullPath);
                        player.Load();

                        lock (_syncRoot)
                        {
                            _notificationSounds[type] = player;
                        }

                        Console.WriteLine($"  - {type} загружен");
                    }
                    catch
                    {
                        Console.WriteLine($"  - {type} не загрузился");
                        lock (_syncRoot)
                        {
                            _notificationSounds[type] = null;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"  - {type} файл отсутствует");
                    lock (_syncRoot)
                    {
                        _notificationSounds[type] = null;
                    }
                }
            }

            Console.WriteLine("Предзагрузка завершена");
        }

        // Остановка системы
        public void Shutdown()
        {
            Console.WriteLine("Остановка системы оповещений");

            lock (_syncRoot)
            {
                foreach (var player in _notificationSounds.Values)
                {
                    if (player != null)
                    {
                        try
                        {
                            player.Stop();
                            player.Dispose();
                        }
                        catch { }
                    }
                }

                _notificationSounds.Clear();
                _notificationHistory.Clear();
            }

            _soundSemaphore.Dispose();
            Console.WriteLine("Система остановлена");
        }
    }
}