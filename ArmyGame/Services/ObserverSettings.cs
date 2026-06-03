using System;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Хранит настройки наблюдателей (логирование урона, звук смерти).
    /// SRP: отвечает ТОЛЬКО за хранение и предоставление настроек.
    /// Загрузка/сохранение делегируется сервису.
    /// </summary>
    public class ObserverSettings
    {
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
            var jsonService = new JsonStorageService(".");
            var settings = jsonService.LoadFromJson<ObserverSettings>("observersettings.json");
            if (settings != null)
                Current = settings;
        }

        /// <summary>
        /// Сохраняет текущие настройки в файл
        /// </summary>
        public static void Save()
        {
            var jsonService = new JsonStorageService(".");
            jsonService.SaveToJson(Current, "observersettings.json");
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
