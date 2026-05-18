using System.IO;
using System.Windows;
using ArmyBattle.Services;

namespace ArmyBattle.UI
{
    public partial class MainWindow : Window
    {
        private readonly ArmyManager _armyManager;
        private readonly BattleManager _battleManager;

        public MainWindow()
        {
            InitializeComponent();
            _armyManager = new ArmyManager();
            _battleManager = new BattleManager();
        }

        private void NewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            var setupWindow = new GameSetupWindow
            {
                Owner = this
            };
            setupWindow.ShowDialog();
        }

        private void LoadGameBtn_Click(object sender, RoutedEventArgs e)
        {
            var loadWindow = new LoadGameWindow(_armyManager, _battleManager)
            {
                Owner = this
            };
            loadWindow.ShowDialog();
        }

        private void GameHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new GameHistoryWindow
            {
                Owner = this
            };
            historyWindow.ShowDialog();
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = this
            };
            settingsWindow.ShowDialog();
        }

        private void ClearHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Удалить ВСЮ историю игр?\n\nЭто удалит и завершённые, и незавершённые игры!\nДействие необратимо!",
                                          "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Удаляем логи (завершённые игры)
                string logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                if (Directory.Exists(logPath))
                {
                    var files = Directory.GetFiles(logPath, "*.*");
                    foreach (var file in files)
                    {
                        try { File.Delete(file); } catch { }
                    }
                }

                // Удаляем сохранения (незавершённые игры)
                string savesPath = Path.Combine(Directory.GetCurrentDirectory(), "Saves");
                if (Directory.Exists(savesPath))
                {
                    var files = Directory.GetFiles(savesPath, "*.*");
                    foreach (var file in files)
                    {
                        try { File.Delete(file); } catch { }
                    }
                }

                MessageBox.Show("История игр полностью очищена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
