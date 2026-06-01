using System;
using System.Windows;
using ArmyBattle.Models;

namespace ArmyBattle.UI
{
    public partial class BattleResultWindow : Window
    {
        public BattleResultWindow(string winnerName, ConsoleColor winnerColor, 
                                   IArmy winnerArmy, IArmy loserArmy, 
                                   int totalMoves, int winnerAdded, int winnerBuffs,
                                   int loserAdded, int loserBuffs, bool isDraw = false)
        {
            InitializeComponent();
            
            if (isDraw)
            {
                WinnerNameText.Text = "НИЧЬЯ!";
                WinnerNameText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow);
            }
            else
            {
                WinnerNameText.Text = winnerName;
                // Конвертируем ConsoleColor в System.Windows.Media.Color
                var color = ConvertConsoleColor(winnerColor);
                WinnerNameText.Foreground = new System.Windows.Media.SolidColorBrush(color);
            }
            
            // Формируем статистику
            var stats = new System.Text.StringBuilder();
            stats.AppendLine($"Всего ходов: {totalMoves}");
            stats.AppendLine("");
            
            if (!isDraw)
            {
                stats.AppendLine($"📊 {winnerArmy.Name} (ПОБЕДИТЕЛЬ):");
                stats.AppendLine($"   Выжило бойцов: {winnerArmy.AliveCount()}/{winnerArmy.Units.Count}");
                stats.AppendLine($"   Добавлено бойцов: {winnerAdded}");
                stats.AppendLine($"   Надето баффов: {winnerBuffs}");
                stats.AppendLine("");
                stats.AppendLine($"📊 {loserArmy.Name} (ПРОИГРАВШИЙ):");
                stats.AppendLine($"   Выжило бойцов: 0/{loserArmy.Units.Count}");
                stats.AppendLine($"   Добавлено бойцов: {loserAdded}");
                stats.AppendLine($"   Надето баффов: {loserBuffs}");
            }
            else
            {
                stats.AppendLine($"📊 {winnerArmy.Name}:");
                stats.AppendLine($"   Выжило бойцов: {winnerArmy.AliveCount()}/{winnerArmy.Units.Count}");
                stats.AppendLine($"   Добавлено бойцов: {winnerAdded}");
                stats.AppendLine($"   Надето баффов: {winnerBuffs}");
                stats.AppendLine("");
                stats.AppendLine($"📊 {loserArmy.Name}:");
                stats.AppendLine($"   Выжило бойцов: {loserArmy.AliveCount()}/{loserArmy.Units.Count}");
                stats.AppendLine($"   Добавлено бойцов: {loserAdded}");
                stats.AppendLine($"   Надето баффов: {loserBuffs}");
            }
            
            StatsText.Text = stats.ToString();
        }
        
        private System.Windows.Media.Color ConvertConsoleColor(ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Red => System.Windows.Media.Colors.Red,
                ConsoleColor.Blue => System.Windows.Media.Colors.DodgerBlue,
                ConsoleColor.Green => System.Windows.Media.Colors.Green,
                ConsoleColor.Yellow => System.Windows.Media.Colors.Yellow,
                ConsoleColor.Cyan => System.Windows.Media.Colors.Cyan,
                ConsoleColor.Magenta => System.Windows.Media.Colors.Magenta,
                ConsoleColor.White => System.Windows.Media.Colors.White,
                _ => System.Windows.Media.Colors.White
            };
        }
        
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}