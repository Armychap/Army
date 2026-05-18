using System.IO;
using System.Windows;
using System.Windows.Controls;
using ArmyBattle.Models;
using ArmyBattle.Game;
using ArmyBattle.Services;
using System.Collections.Generic;

namespace ArmyBattle.UI
{
    public partial class LoadGameWindow : Window
    {
        private ArmyManager _armyManager;
        private BattleManager _battleManager;
        private List<string> _savedGames = new();

        public LoadGameWindow(ArmyManager armyManager, BattleManager battleManager)
        {
            InitializeComponent();
            _armyManager = armyManager;
            _battleManager = battleManager;
            LoadSavedGames();
        }

        private void LoadSavedGames()
        {
            _savedGames.Clear();
            GameListBox.Items.Clear();

            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "saves");
            if (!Directory.Exists(savePath))
            {
                NoGamesMessage.Visibility = Visibility.Visible;
                GameListBox.Visibility = Visibility.Collapsed;
                return;
            }

            var files = Directory.GetFiles(savePath, "*.json");
            if (files.Length == 0)
            {
                NoGamesMessage.Visibility = Visibility.Visible;
                GameListBox.Visibility = Visibility.Collapsed;
                return;
            }

            GameListBox.Visibility = Visibility.Visible;
            NoGamesMessage.Visibility = Visibility.Collapsed;

            foreach (var file in files.OrderByDescending(f => new FileInfo(f).LastWriteTime))
            {
                string gameName = Path.GetFileNameWithoutExtension(file);
                var fileInfo = new FileInfo(file);
                _savedGames.Add(gameName);

                var item = new TextBlock
                {
                    Text = $"{gameName}\n└ {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm}",
                    Foreground = System.Windows.Media.Brushes.White,
                    Margin = new Thickness(10),
                    FontSize = 12
                };
                GameListBox.Items.Add(item);
            }
        }

        private void LoadGameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GameListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите игру для загрузки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string selectedGame = _savedGames[GameListBox.SelectedIndex];
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "saves", selectedGame + ".json");

                if (!_armyManager.LoadArmies(filePath, out IArmy? army1, out IArmy? army2, out int round, out int attackTurn, out bool firstAtt, out bool needHeader, out string? battleLogName, out int moveCount, out FormationType currentFormation) || army1 == null || army2 == null)
                {
                    MessageBox.Show("Не удалось загрузить сохранение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ObserverManager.LoadSettings(army1, army2);

                // Загружаем существующий лог
                string logName = string.IsNullOrWhiteSpace(battleLogName) ? selectedGame : battleLogName;
                string logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", logName + ".txt");
                string previousLog = File.Exists(logPath) ? File.ReadAllText(logPath) : string.Empty;

                var battleWindow = new BattleWindow();
                var engine = new BattleEngine(army1, army2, 400);
                engine.SetView(battleWindow);

                // Восстанавливаем состояние
                engine.SetBattleState(round, attackTurn, firstAtt, needHeader);
                engine.SetMoveCount(moveCount);
                engine.SetFormationStrategy(currentFormation);

                if (currentFormation == FormationType.OneColumn)
                {
                    engine.SetCurrentFightersForContinuation();
                }
                else if (currentFormation == FormationType.ThreeColumns)
                {
                    engine.InitializeThreeColumns();
                }
                else if (currentFormation == FormationType.Wall)
                {
                    engine.GetCurrentStrategy()?.Reinitialize(engine);
                }

                engine.SetBattleInitialized(true);

                battleWindow.UpdateUI(engine, army1, army2, currentFormation, previousLog);
                battleWindow.ShowDialog();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DeleteGameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GameListBox.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите игру для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string selectedGame = _savedGames[GameListBox.SelectedIndex];
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "saves", selectedGame + ".json");

                if (File.Exists(filePath))
                {
                    var result = MessageBox.Show($"Удалить игру '{selectedGame}'?\n\nЭто действие необратимо!", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        File.Delete(filePath);
                        LoadSavedGames();
                        MessageBox.Show("Игра удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
