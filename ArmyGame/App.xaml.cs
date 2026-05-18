using System;
using System.Windows;
using ArmyBattle.Services;

namespace ArmyBattle
{
    public partial class App : Application
    {
        public App()
        {
            ObserverSettings.Load();
            ObserverManager.LoadSettings();
        }
    }
}