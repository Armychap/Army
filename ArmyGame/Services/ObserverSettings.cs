using System;
using System.IO;
using System.Text.Json;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Настройки наблюдателей (логирование урона, звук смерти)
    /// </summary>
    public class ObserverSettings
    {
        /// <summary>
        /// Имя файла для сохранения настроек
        /// </summary>
        private const string SettingsFile = "observersettings.json";

        /// <summary>
        /// Включено ли логирование урона в файл
        /// </summary>
        public bool EnableDamageLog { get; set; } = true;
        
        /// <summary>
        /// Включён ли звуковой сигнал при смерти юнита
        /// </summary>
        public bool EnableDeathBeep { get; set; } = true;

        /// <summary>
        /// Текущие настройки (синглтон)
        /// </summary>
        public static ObserverSettings Current { get; private set; } = new ObserverSettings();

        /// <summary>
        /// Загружает настройки из файла
        /// </summary>
        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<ObserverSettings>(json);
                    if (settings != null)
                        Current = settings;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Ошибка загрузки настроек прокси: {ex.Message}");
            }
        }

        /// <summary>
        /// Сохраняет текущие настройки в файл
        /// </summary>
        public static void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Current, options);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Ошибка сохранения настроек прокси: {ex.Message}");
            }
        }

        /// <summary>
        /// Сбрасывает настройки к значениям по умолчанию
        /// </summary>
        public static void Reset()
        {
            Current = new ObserverSettings();
            Save();
        }
    }
}