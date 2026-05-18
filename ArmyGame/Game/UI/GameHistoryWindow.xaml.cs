using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ArmyBattle.UI
{
    public partial class GameHistoryWindow : Window
    {
        private readonly List<string> _historyFiles = new();

        public GameHistoryWindow()
        {
            InitializeComponent();
            LoadGameHistory();
        }

        private void LoadGameHistory()
        {
            _historyFiles.Clear();
            HistoryListBox.Items.Clear();
            GameDetailsText.Text = "Выберите игру для просмотра деталей";
            
            string logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logPath))
            {
                HistoryListBox.Items.Add(new TextBlock { Text = "Истории игр не найдены", Foreground = System.Windows.Media.Brushes.Gray, Margin = new Thickness(10) });
                return;
            }

            var files = Directory.GetFiles(logPath, "*.txt").OrderByDescending(f => new FileInfo(f).LastWriteTime);
            
            if (!files.Any())
            {
                HistoryListBox.Items.Add(new TextBlock { Text = "Истории игр не найдены", Foreground = System.Windows.Media.Brushes.Gray, Margin = new Thickness(10) });
                return;
            }

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var gameName = Path.GetFileNameWithoutExtension(file);
                _historyFiles.Add(file);

                var item = new ListBoxItem
                {
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = gameName,
                                FontSize = 12,
                                FontWeight = System.Windows.FontWeights.Bold,
                                Foreground = System.Windows.Media.Brushes.White
                            },
                            new TextBlock
                            {
                                Text = $"📅 {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}",
                                FontSize = 10,
                                Foreground = System.Windows.Media.Brushes.LightGray,
                                Margin = new Thickness(0, 4, 0, 0)
                            }
                        }
                    },
                    Tag = file,
                    Margin = new Thickness(0, 0, 0, 4)
                };

                HistoryListBox.Items.Add(item);
            }
        }

        private void ShowGameDetails(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                GameDetailsText.Text = content;
            }
            catch (Exception ex)
            {
                GameDetailsText.Text = $"Ошибка чтения файла: {ex.Message}";
            }
        }

        private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryListBox.SelectedItem is ListBoxItem item && item.Tag is string filePath)
            {
                ShowGameDetails(filePath);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
