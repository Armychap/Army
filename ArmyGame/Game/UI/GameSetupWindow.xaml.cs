using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ArmyBattle.Models;
using ArmyBattle.Services;
using ArmyBattle.Game;

namespace ArmyBattle.UI
{
    public partial class GameSetupWindow : Window
    {
        private int _currentStep = 1;
        private string? _army1Name;
        private string? _army2Name;
        private int _budget;
        private IArmy? _army1;
        private IArmy? _army2;
        private FormationType _formation = FormationType.OneColumn;

        public GameSetupWindow()
        {
            InitializeComponent();
            ShowStep(1);
        }

        private void ShowStep(int step)
        {
            _currentStep = step;
            Step1Panel.Visibility = step == 1 ? Visibility.Visible : Visibility.Collapsed;
            Step2Panel.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
            Step3Panel.Visibility = step == 3 ? Visibility.Visible : Visibility.Collapsed;
            Step4Panel.Visibility = step == 4 ? Visibility.Visible : Visibility.Collapsed;
            PrevBtn.Visibility = step > 1 ? Visibility.Visible : Visibility.Collapsed;

            string title = step switch
            {
                1 => "Шаг 1: Названия армий и бюджет",
                2 => $"Шаг 2: Сборка армии {_army1Name}",
                3 => $"Шаг 3: Сборка армии {_army2Name}",
                4 => "Шаг 4: Выбор построения",
                _ => ""
            };
            StepTitle.Text = title;
            ProgressBar.Value = step * 25;

            NextBtn.Content = step == 4 ? "Начать битву" : "Далее →";
            NextBtn.IsEnabled = step switch
            {
                2 => _army1 != null,
                3 => _army2 != null,
                _ => true
            };
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep == 1 && ValidateStep1())
                ShowStep(2);
            else if (_currentStep == 2 && _army1 != null)
                ShowStep(3);
            else if (_currentStep == 3 && _army2 != null)
                ShowStep(4);
            else if (_currentStep == 4)
                StartBattle();
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep > 1)
                ShowStep(_currentStep - 1);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateStep1()
        {
            if (string.IsNullOrWhiteSpace(Army1NameBox.Text))
            {
                MessageBox.Show("Введите название 1-й армии", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Army2NameBox.Text))
            {
                MessageBox.Show("Введите название 2-й армии", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!int.TryParse(BudgetBox.Text, out _budget) || _budget <= 0)
            {
                MessageBox.Show("Введите корректный бюджет", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            _army1Name = Army1NameBox.Text;
            _army2Name = Army2NameBox.Text;
            return true;
        }

        private async void Army1AutoPanel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var army = new Army(_army1Name!, ConsoleColor.Red);

            var previewWindow = new Window
            {
                Title = $"Генерация армии {_army1Name}...",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock { Text = "Идёт генерация...", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
            };
            previewWindow.Owner = this;
            previewWindow.Show();

            await Task.Run(() => army.GenerateArmyWithBudget(_budget));

            previewWindow.Close();

            // Открываем окно просмотра состава (только для чтения)
            var resultWindow = new ManualArmyWindow(_army1Name!, ConsoleColor.Red, _budget, army, isReadOnly: true);
            resultWindow.Owner = this;

            // Если нажали "Закрыть" (DialogResult = true) - переходим к следующему шагу
            if (resultWindow.ShowDialog() == true)
            {
                _army1 = resultWindow.ResultArmy;
                ShowStep(3);  // Переход ко второй армии
            }
            // Если закрыли по-другому (крестик) - остаёмся на этом же шаге
        }

        private void Army1ManualPanel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var wnd = new ManualArmyWindow(_army1Name!, ConsoleColor.Red, _budget, null, isReadOnly: false);
            wnd.Owner = this;
            if (wnd.ShowDialog() == true && wnd.ResultArmy != null)
            {
                _army1 = wnd.ResultArmy;
                ShowStep(3);
            }
        }

        private async void Army2AutoPanel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var army = new Army(_army2Name!, ConsoleColor.Blue);

            var previewWindow = new Window
            {
                Title = $"Генерация армии {_army2Name}...",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock { Text = "Идёт генерация...", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
            };
            previewWindow.Owner = this;
            previewWindow.Show();

            await Task.Run(() => army.GenerateArmyWithBudget(_budget));

            previewWindow.Close();

            var resultWindow = new ManualArmyWindow(_army2Name!, ConsoleColor.Blue, _budget, army, isReadOnly: true);
            resultWindow.Owner = this;
            if (resultWindow.ShowDialog() == true)
            {
                _army2 = resultWindow.ResultArmy;
                ShowStep(4);
            }
        }

        private void Army2ManualPanel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var wnd = new ManualArmyWindow(_army2Name!, ConsoleColor.Blue, _budget, null, isReadOnly: false);
            wnd.Owner = this;
            if (wnd.ShowDialog() == true && wnd.ResultArmy != null)
            {
                _army2 = wnd.ResultArmy;
                ShowStep(4);
            }
        }

        private void FormationOneColumn_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _formation = FormationType.OneColumn;
            StartBattle();
        }

        private void FormationThreeColumns_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _formation = FormationType.ThreeColumns;
            StartBattle();
        }

        private void FormationWall_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _formation = FormationType.Wall;
            StartBattle();
        }

        private void StartBattle()
        {
            if (_army1 == null || _army2 == null)
            {
                MessageBox.Show("Ошибка: армии не созданы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ObserverManager.LoadSettings(_army1, _army2);

            var battleWindow = new BattleWindow();
            var engine = new BattleEngine(_army1, _army2, 400);
            engine.SetView(battleWindow);
            engine.InitializeBattle(_formation);
            battleWindow.UpdateUI(engine, _army1, _army2, _formation);
            battleWindow.ShowDialog();

            DialogResult = true;
            Close();
        }
    }
}