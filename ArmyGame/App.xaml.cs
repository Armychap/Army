using System;
using System.Windows;
using ArmyBattle.Services;

namespace ArmyBattle
{
    /// <summary>
    /// Главный класс приложения WPF
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Конструктор приложения, загружает настройки наблюдателей
        /// </summary>
        public App()
        {
            // Загружаем сохранённые настройки наблюдателей из файла
            ObserverSettings.Load();
            // Применяем загруженные настройки к системе наблюдателей
            ObserverManager.LoadSettings();
        }
    }
}