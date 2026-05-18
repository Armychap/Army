using System.Windows;
using ArmyBattle.Services;
 
namespace ArmyBattle.UI
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }
 
        private void LoadSettings()
        {
            var settings = ObserverSettings.Current;
            EnableLoggingCheckbox.IsChecked = settings.EnableDamageLog;
            EnableDeathSoundCheckbox.IsChecked = settings.EnableDeathBeep;
            BattleSpeedSlider.Value = 400;
        }
 
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = ObserverSettings.Current;
                settings.EnableDamageLog = EnableLoggingCheckbox.IsChecked ?? false;
                settings.EnableDeathBeep = EnableDeathSoundCheckbox.IsChecked ?? false;
                
                ObserverSettings.Save();
                
                MessageBox.Show("Настройки сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
 
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}