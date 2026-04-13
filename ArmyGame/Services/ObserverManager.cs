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
        private static DamageLogObserver? _damageLogObserver;
        private static DeathBeepObserver? _deathBeepObserver;
        private static bool _damageLogEnabled = false;
        private static bool _deathBeepEnabled = false;

        /// <summary>
        /// Применить настройки наблюдателей ко всем юнитам в армиях
        /// </summary>
        public static void ApplySettingsToArmies(IArmy army1, IArmy army2)
        {
            ApplyToArmy(army1);
            ApplyToArmy(army2);
        }

        private static void ApplyToArmy(IArmy army)
        {
            if (army?.Units == null) return;
            
            foreach (var unit in army.Units)
            {
                ApplyToUnit(unit);
            }
        }

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
            ProxySettings.Current.EnableDamageLog = enabled;
            
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
            ProxySettings.Current.EnableDeathBeep = enabled;
            
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
        /// Загрузить настройки из ProxySettings
        /// </summary>
        public static void LoadSettings(IArmy? army1 = null, IArmy? army2 = null)
        {
            _damageLogEnabled = ProxySettings.Current.EnableDamageLog;
            _deathBeepEnabled = ProxySettings.Current.EnableDeathBeep;
            
            if (army1 != null && army2 != null)
            {
                ApplySettingsToArmies(army1, army2);
            }
        }
    }
}