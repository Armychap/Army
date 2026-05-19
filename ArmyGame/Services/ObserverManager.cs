using System.Collections.Generic;
using ArmyBattle.Models;
using ArmyBattle.Models.Interfaces;
using ArmyBattle.Models.Observers;

namespace ArmyBattle.Services
{
    /// <summary>
    /// Менеджер для управления наблюдателями всех юнитов
    /// </summary>
    public static class ObserverManager
    {
        /// <summary>
        /// Наблюдатель для логирования урона в файл
        /// </summary>
        private static DamageLogObserver? _damageLogObserver;
        
        /// <summary>
        /// Наблюдатель для воспроизведения звука при смерти юнита
        /// </summary>
        private static DeathBeepObserver? _deathBeepObserver;
        
        /// <summary>
        /// Флаг включения логирования урона
        /// </summary>
        private static bool _damageLogEnabled = false;
        
        /// <summary>
        /// Флаг включения звука при смерти
        /// </summary>
        private static bool _deathBeepEnabled = false;

        /// <summary>
        /// Применить настройки наблюдателей ко всем юнитам в армиях
        /// </summary>
        public static void ApplySettingsToArmies(IArmy army1, IArmy army2)
        {
            ApplyToArmy(army1);
            ApplyToArmy(army2);
        }

        /// <summary>
        /// Применяет настройки наблюдателей ко всем юнитам указанной армии
        /// </summary>
        private static void ApplyToArmy(IArmy army)
        {
            if (army?.Units == null) return;

            foreach (var unit in army.Units)
            {
                ApplyToUnit(unit);
            }
        }

        /// <summary>
        /// Применяет настройки наблюдателей к конкретному юниту
        /// </summary>
        private static void ApplyToUnit(IUnit unit)
        {
            // Очищаем старых наблюдателей
            unit.ClearObservers();

            // Добавляем новых в соответствии с настройками
            if (_damageLogEnabled)
            {
                _damageLogObserver ??= new DamageLogObserver(true);
                unit.AttachObserver(_damageLogObserver);
            }

            if (_deathBeepEnabled)
            {
                _deathBeepObserver ??= new DeathBeepObserver(true);
                unit.AttachObserver(_deathBeepObserver);
            }
        }

        /// <summary>
        /// Включить/выключить логирование урона
        /// </summary>
        public static void SetDamageLogEnabled(bool enabled, IArmy? army1 = null, IArmy? army2 = null)
        {
            _damageLogEnabled = enabled;
            ObserverSettings.Current.EnableDamageLog = enabled;

            if (army1 != null && army2 != null)
            {
                ApplySettingsToArmies(army1, army2);
            }
        }

        /// <summary>
        /// Включить/выключить звук при смерти
        /// </summary>
        public static void SetDeathBeepEnabled(bool enabled, IArmy? army1 = null, IArmy? army2 = null)
        {
            _deathBeepEnabled = enabled;
            ObserverSettings.Current.EnableDeathBeep = enabled;

            if (army1 != null && army2 != null)
            {
                ApplySettingsToArmies(army1, army2);
            }
        }

        /// <summary>
        /// Получить состояние логирования урона
        /// </summary>
        public static bool IsDamageLogEnabled() => _damageLogEnabled;

        /// <summary>
        /// Получить состояние звука при смерти
        /// </summary>
        public static bool IsDeathBeepEnabled() => _deathBeepEnabled;

        /// <summary>
        /// Загрузить настройки из ObserverSettings
        /// </summary>
        public static void LoadSettings(IArmy? army1 = null, IArmy? army2 = null)
        {
            _damageLogEnabled = ObserverSettings.Current.EnableDamageLog;
            _deathBeepEnabled = ObserverSettings.Current.EnableDeathBeep;

            // Применяем настройки только если армии не null
            if (army1 != null && army2 != null)
            {
                ApplySettingsToArmies(army1, army2);
            }
        }
    }
}