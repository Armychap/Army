using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArmyBattle.Models;

namespace ArmyBattle.UI
{
    public partial class ManualArmyWindow : Window
    {
        private Army _army;
        private int _budget;
        private bool _isReadOnly;
        public IArmy? ResultArmy { get; private set; }

        public ManualArmyWindow(string name, ConsoleColor color, int budget, IArmy? existingArmy = null, bool isReadOnly = false)
        {
            InitializeComponent();
            _budget = budget;
            _isReadOnly = isReadOnly;

            if (existingArmy != null)
            {
                _army = (Army)existingArmy;
            }
            else
            {
                _army = new Army(name, color);
            }

            TitleText.Text = $"Армия: {name}";

            if (isReadOnly)
            {
                DisableEditing();
            }

            UpdateBudgetText();
        }

        private void DisableEditing()
        {
            // Скрываем левую панель с доступными юнитами
            var leftPanel = this.FindName("LeftPanel") as DockPanel;
            if (leftPanel != null)
            {
                leftPanel.Visibility = Visibility.Collapsed;
            }

            // Скрываем разделитель
            var divider = this.FindName("Divider") as Border;
            if (divider != null)
            {
                divider.Visibility = Visibility.Collapsed;
            }

            // Скрываем кнопку "Готово"
            SaveBtn.Visibility = Visibility.Collapsed;

            // Меняем текст кнопки "Отмена" на "Закрыть"
            CancelBtn.Content = "✖ Закрыть";
            CancelBtn.Width = 100;
            CancelBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2a1a1a"));
            CancelBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6b6b"));
            CancelBtn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff6b6b"));

            // Растягиваем правую панель на всю ширину
            var rightPanel = this.FindName("RightPanel") as DockPanel;
            if (rightPanel != null)
            {
                Grid.SetColumn(rightPanel, 0);
                Grid.SetColumnSpan(rightPanel, 3);
            }

            StatusText.Text = $"👁 Состав армии: {_army.Units.Count} юнитов, стоимость: {_army.TotalCost}💰";
        }

        private void UpdateBudgetText()
        {
            BudgetText.Text = $"{_army.TotalCost}/{_budget}";

            // Обновляем список юнитов
            var unitsList = new List<UnitDisplayItem>();
            foreach (var u in _army.Units)
            {
                string icon = u.Name switch
                {
                    "Слабый боец" => "🗡️",
                    "Лучник" => "🏹",
                    "Лекарь" => "✙",
                    "Маг" => "🔮",
                    "Сильный боец" => "🪖",
                    "Гуляй город" => "🧱",
                    _ => "⚔"
                };
                unitsList.Add(new UnitDisplayItem
                {
                    Icon = icon,
                    Name = u.Name,
                    FighterNumber = u.FighterNumber,
                    Health = u.Health,
                    MaxHealth = u.MaxHealth,
                    Attack = u.Attack,
                    Defence = u.Defence,
                    Cost = u.Cost
                });
            }
            UnitsList.ItemsSource = unitsList;

            if (_army.Units.Count > 0 && !_isReadOnly)
            {
                var last = _army.Units[^1];
                StatusText.Text = $"Добавлено: {last.Name} #{last.FighterNumber} ({last.Cost}💰)";
            }
            else if (_army.Units.Count > 0 && _isReadOnly)
            {
                StatusText.Text = $"Всего юнитов: {_army.Units.Count}, стоимость: {_army.TotalCost}💰";
            }
            else
            {
                StatusText.Text = "Добавьте хотя бы одного юнита";
            }

            UnitsScrollViewer?.ScrollToEnd();
        }

        private void AddWeak_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_army.TotalCost + 15 <= _budget)
            {
                _army.AddUnit(new WeakFighter(_army.Units.Count + 1));
                UpdateBudgetText();
            }
            else MessageBox.Show("Недостаточно бюджета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void AddArcher_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_army.TotalCost + 25 <= _budget)
            {
                _army.AddUnit(new Archer(_army.Units.Count + 1));
                UpdateBudgetText();
            }
            else MessageBox.Show("Недостаточно бюджета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void AddWizard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_army.TotalCost + 30 <= _budget)
            {
                _army.AddUnit(new Wizard(_army.Units.Count + 1));
                UpdateBudgetText();
            }
            else MessageBox.Show("Недостаточно бюджета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void AddStrong_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_army.TotalCost + 40 <= _budget)
            {
                _army.AddUnit(new StrongFighter(_army.Units.Count + 1));
                UpdateBudgetText();
            }
            else MessageBox.Show("Недостаточно бюджета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void AddShieldWall_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_army.TotalCost + 55 <= _budget)
            {
                _army.AddUnit(new GulayGorod(_army.Units.Count + 1));
                UpdateBudgetText();
            }
            else MessageBox.Show("Недостаточно бюджета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void AddHealer_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_army.TotalCost + 20 <= _budget)
            {
                _army.AddUnit(new Healer(_army.Units.Count + 1));
                UpdateBudgetText();
            }
            else MessageBox.Show("Недостаточно бюджета", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void RemoveLast_Click(object sender, RoutedEventArgs e)
        {
            if (_army.Units.Count > 0 && !_isReadOnly)
            {
                var last = _army.Units[_army.Units.Count - 1];
                _army.Units.RemoveAt(_army.Units.Count - 1);
                _army.TotalCost -= last.Cost;
                UpdateBudgetText();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // В режиме только просмотра - закрываем с результатом, чтобы продолжить
            if (_isReadOnly)
            {
                ResultArmy = _army;
                DialogResult = true;
                Close();
                return;
            }

            // В ручном режиме - отмена
            DialogResult = false;
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // В режиме только просмотра (авто-сборка)
            if (_isReadOnly)
            {
                ResultArmy = _army;
                DialogResult = true;  // Возвращаем true, чтобы окно закрылось и показало, что всё ОК
                Close();
                return;
            }

            // В ручном режиме - проверяем что армия не пустая
            if (_army.Units.Count == 0)
            {
                MessageBox.Show("Армия не может быть пустой! Добавьте хотя бы одного юнита.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ResultArmy = _army;
            DialogResult = true;
            Close();
        }
    }

    // Вспомогательный класс для отображения в ItemsControl
    public class UnitDisplayItem
    {
        public string Icon { get; set; } = "";
        public string Name { get; set; } = "";
        public int FighterNumber { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defence { get; set; }
        public int Cost { get; set; }
    }
}