using System;
using System.IO;
using System.Text.Json;

namespace ArmyBattle.Services
{
    public class ProxySettings
    {
        private const string SettingsFile = "proxysettings.json";

        public bool EnableDamageLog { get; set; }
        public bool EnableDeathBeep { get; set; }

        public static ProxySettings Current { get; private set; } = new ProxySettings();

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    var settings = JsonSerializer.Deserialize<ProxySettings>(json);
                    if (settings != null)
                        Current = settings;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Ошибка загрузки настроек прокси: {ex.Message}");
            }
        }

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

        public static void Reset()
        {
            Current = new ProxySettings();
            Save();
        }
    }
}
