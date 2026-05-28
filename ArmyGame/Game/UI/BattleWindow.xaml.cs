using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArmyBattle.Models;
using ArmyBattle.Models.Decorators;
using ArmyBattle.Game;
using ArmyBattle.Game.Commands;
using ArmyBattle.Services;
using System.Text;

namespace ArmyBattle.UI
{
    public partial class BattleWindow : Window, IBattleView
    {
        // ── engine state ────────────────────────────────────────────────────────────
        private BattleEngine? _engine;
        private IArmy? _army1;
        private IArmy? _army2;
        private FormationType _formation;
        private CommandManager _cmd = new();
        private ArmyManager _armyMgr = new();
        private bool _battleOver;
        private bool _autoBattleRunning;
        private CancellationTokenSource? _autoCts;

        // ── colours ─────────────────────────────────────────────────────────────────
        private static readonly string C1 = "#4ecdc4";   // army 1 teal
        private static readonly string C2 = "#ff6b6b";   // army 2 coral
        private static readonly string CLog = "#e0d5c7";
        private static readonly double RosterBarMax = 176; // px

        public BattleWindow() => InitializeComponent();

        // ════════════════════════════════════════════════════════════════════════════
        // Entry point
        // ════════════════════════════════════════════════════════════════════════════

        public void UpdateUI(BattleEngine engine, IArmy army1, IArmy army2, FormationType formation, string? initialLog = null)
        {
            _engine = engine;
            _army1 = army1;
            _army2 = army2;
            _formation = formation;

            Army1NameLbl.Text = army1.Name;
            Army2NameLbl.Text = army2.Name;
            FormationLbl.Text = FormatName(formation);
            RoundLbl.Text = engine.Round.ToString();

            LogPanel.Children.Clear();
            RenderRosters();
            RenderFormationField();
            RefreshButtons();

            AddLog($"Битва началась! {army1.Name} vs {army2.Name}", "#95e1d3");
            AddLog($"Построение: {FormatName(formation)}", "#ffd93d");

            if (!string.IsNullOrWhiteSpace(initialLog))
            {
                LoadBattleLog(initialLog);
                AddLog("Предыдущая история загружена.", "#95e1d3");
            }
        }

        // ════════════════════════════════════════════════════════════════════════════
        // IBattleView
        // ════════════════════════════════════════════════════════════════════════════

        public void DisplayStart(string a1, string a2, int cost) { }

        public void DisplayRound(int round, IUnit? f1, IUnit? f2, IArmy a1, IArmy a2)
        {
            Dispatcher.Invoke(() =>
            {
                RoundLbl.Text = round.ToString();
                RenderRosters();
                RenderFormationField();  // ← КРИТИЧЕСКИ ВАЖНО ДЛЯ ОБНОВЛЕНИЯ
                RefreshButtons();
            });
        }

        public void DisplayAttack(IUnit att, IUnit def, IArmy attArmy, IArmy defArmy, int dmg)
        {
            Dispatcher.Invoke(() =>
            {
                var c = attArmy == _army1 ? C1 : C2;
                AddLog($"⚔ {att.Name}#{att.FighterNumber} → {def.Name}#{def.FighterNumber}: −{dmg} HP", c);
                RenderRosters();
                RenderFormationField();
            });
        }

        public void DisplayDeath(IUnit killer, IUnit victim, IArmy kArmy, IArmy dArmy)
        {
            Dispatcher.Invoke(() =>
            {
                var c = dArmy == _army1 ? C1 : C2;
                AddLog($"☠ {victim.Name}#{victim.FighterNumber} пал от руки {killer.Name}#{killer.FighterNumber}", "#888");

                // КРИТИЧЕСКИ ВАЖНО: обновляем поле боя после смерти
                RenderRosters();
                RenderFormationField();

                // Проверяем завершение битвы
                if (!_engine.IsCombatActive)
                {
                    FinishBattle();
                    ShowWinnerAndExit();
                }
            });
        }

        public void DisplaySpecialAbility(IUnit user, IUnit? target, IArmy uArmy, IArmy tArmy, string name)
        {
            Dispatcher.Invoke(() =>
            {
                var c = uArmy == _army1 ? C1 : C2;
                var t = target == null ? "поле" : $"{target.Name}#{target.FighterNumber}";
                AddLog($"✨ {user.Name}#{user.FighterNumber}: {name} → {t}", c);
            });
        }

        public void DisplayBuff(IUnit unit, string buff, int atk, int def)
        {
            Dispatcher.Invoke(() =>
            {
                AddLog($"🔰 {unit.Name}#{unit.FighterNumber} получил бафф {buff} (+{atk}⚔ +{def}🛡)", "#ffd93d");
                RenderBuffs();
            });
        }

        public void DisplayStalemate(string reason)
        {
            Dispatcher.Invoke(() =>
            {
                AddLog($"⚖ Патовая ситуация: {reason}", "#ffd93d");
                FinishBattle();
            });
        }

        public void DisplayWinner(string? winner, ConsoleColor? color = null)
        {
            Dispatcher.Invoke(() =>
            {
                var name = winner ?? "НИЧЬЯ";
                AddLog($"🏆 ПОБЕДИТЕЛЬ: {name}", "#ffd93d");
                FinishBattle();
                MessageBox.Show($"🏆 {name}", "Битва завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void DisplayStatistics(int moves, int s1, int s2, int a1, int a2, int b1, int b2)
        {
            Dispatcher.Invoke(() =>
                AddLog($"Статистика: ходов {moves}, выжили {s1}/{s2}, добавлено {a1}/{a2}, баффов {b1}/{b2}", "#95e1d3"));
        }

        public void ClearScreen() { }
        public void WaitForKey(string msg = "") { }

        // ════════════════════════════════════════════════════════════════════════════
        // Button handlers
        // ════════════════════════════════════════════════════════════════════════════

        private void MoveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_engine == null || _battleOver) return;

            var cmd = new MakeMoveCommand(_engine);
            _cmd.ExecuteCommand(cmd);

            RenderRosters();
            RenderFormationField();
            RefreshButtons();

            // Проверяем завершение битвы
            if (!_engine.IsCombatActive || _engine.StalemateReached)
            {
                FinishBattle();
                ShowWinnerAndExit();
            }
        }

        private async void AutoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_engine == null || _battleOver) return;

            _autoBattleRunning = true;
            _autoCts = new CancellationTokenSource();
            AutoBtn.IsEnabled = false;
            MoveBtn.IsEnabled = false;
            FormationBtn.IsEnabled = false;
            SaveBtn.IsEnabled = false;

            try
            {
                await Task.Run(() =>
                {
                    while (_engine.IsCombatActive && !_autoCts.Token.IsCancellationRequested)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var cmd = new MakeMoveCommand(_engine);
                            _cmd.ExecuteCommand(cmd);

                            // Обновляем UI после каждого хода
                            RenderRosters();
                            RenderFormationField();
                        });
                        Thread.Sleep(200);
                    }
                }, _autoCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Отменено пользователем
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto battle error: {ex.Message}");
            }

            _autoBattleRunning = false;

            Dispatcher.Invoke(() =>
            {
                RenderRosters();
                RenderFormationField();
                RefreshButtons();

                // ПРОВЕРЯЕМ ЗАВЕРШЕНИЕ БИТВЫ
                if (!_engine.IsCombatActive || _engine.StalemateReached)
                {
                    FinishBattle();
                    ShowWinnerAndExit();  // ← ВАЖНО: показываем победителя
                }
                else
                {
                    RefreshButtons();
                }
            });
        }
        private void UndoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_cmd.UndoCount == 0) return;
            _cmd.Undo();
            if (_engine != null)
            {
                _formation = _engine.GetCurrentFormation();
                FormationLbl.Text = FormatName(_formation);
            }
            RenderRosters();
            RenderFormationField();
            RefreshButtons();
        }

        private void RedoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_cmd.RedoCount == 0) return;
            _cmd.Redo();
            if (_engine != null)
            {
                _formation = _engine.GetCurrentFormation();
                FormationLbl.Text = FormatName(_formation);
            }
            RenderRosters();
            RenderFormationField();
            RefreshButtons();
        }

        private void FormationBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_engine == null) return;

            var dlg = new FormationPickDialog(_engine.GetCurrentFormation());
            if (dlg.ShowDialog() != true) return;

            var cmd = new ChangeFormationCommand(_engine, dlg.SelectedFormation);
            _cmd.ExecuteCommand(cmd);
            _formation = dlg.SelectedFormation;
            FormationLbl.Text = FormatName(_formation);
            RenderFormationField();
            RefreshButtons();
            AddLog($"⚙ Построение изменено: {FormatName(_formation)}", "#ffd93d");
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_engine == null || _army1 == null || _army2 == null) return;

            string saveName = SanitizeFileName($"{_army1.Name} vs {_army2.Name}");
            _armyMgr.SaveArmies(_army1, _army2, saveName,
                _engine.Round, _engine.AttackTurn,
                _engine.FirstAttackerIsArmy1, _engine.NeedNewRoundHeader,
                moveCount: _engine.MoveCount,
                currentFormation: _engine.GetCurrentFormation());

            MessageBox.Show($"Игра сохранена как '{saveName}' и возвращаемся в главное меню.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            _autoCts?.Cancel();
            Close();
        }

        // ════════════════════════════════════════════════════════════════════════════
        // Rendering helpers
        // ════════════════════════════════════════════════════════════════════════════

        private void RenderRosters()
        {
            if (_army1 == null || _army2 == null) return;
            RenderSideRoster(_army1, Army1RosterPanel, Army1CountLbl, Army1Bar, true);
            RenderSideRoster(_army2, Army2RosterPanel, Army2CountLbl, Army2Bar, false);
            RenderBuffs();
        }

        private void RenderSideRoster(IArmy army, StackPanel panel, TextBlock countLbl, Border bar, bool isA1)
        {
            panel.Children.Clear();

            // Важно: используем AliveFightersInBattleOrder, а не Units!
            var aliveUnits = army.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();
            int alive = aliveUnits.Count;
            int total = army.Units.Count;
            countLbl.Text = $"Живых: {alive}/{total}";
            bar.Width = total > 0 ? Math.Max(4, (double)alive / total * RosterBarMax) : 0;

            // Отображаем в порядке боя
            foreach (var u in aliveUnits)
            {
                panel.Children.Add(MakeUnitCard(u, isA1));
            }
        }

        private Border MakeUnitCard(IUnit u, bool isA1)
        {
            double hpPct = u.MaxHealth > 0 ? (double)u.Health / u.MaxHealth : 0;
            string barC = hpPct > 0.5 ? "#2ecc71" : hpPct > 0.2 ? "#ffd93d" : "#ff6b6b";
            string edgeC = isA1 ? C1 : C2;

            // Buffs text
            var buffNames = GetBuffNames(u);
            bool hasBuff = buffNames.Count > 0;

            var border = new Border
            {
                Background = ParseBrush(u.IsAlive ? "#1a2030" : "#111"),
                BorderBrush = ParseBrush(u.IsAlive ? edgeC : "#333"),
                BorderThickness = new Thickness(u.IsAlive ? 1 : 0.5),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 0, 0, 4),
                Padding = new Thickness(8, 6, 8, 6),
                Opacity = u.IsAlive ? 1.0 : 0.45
            };

            var stack = new StackPanel();

            // Name row
            var nameRow = new Grid();
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            nameRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });

            var icon = new TextBlock { Text = UnitIcon(u.Name), FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
            Grid.SetColumn(icon, 0);

            var nameBlk = new TextBlock
            {
                Text = $"{u.Name} #{u.FighterNumber}",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = ParseBrush("#e0d5c7"),
                TextDecorations = u.IsAlive ? null : TextDecorations.Strikethrough,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameBlk, 1);

            // HP text inline
            var hpBlk = new TextBlock
            {
                Text = $"❤ {Math.Max(0, u.Health)}/{u.MaxHealth}",
                FontSize = 9,
                Foreground = ParseBrush(barC),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(hpBlk, 2);

            nameRow.Children.Add(icon);
            nameRow.Children.Add(nameBlk);
            nameRow.Children.Add(hpBlk);
            stack.Children.Add(nameRow);

            // Attack/Defence row
            var statsRow = new TextBlock
            {
                Text = $"⚔ {u.EffectiveAttack}  🛡 {u.EffectiveDefence}",
                FontSize = 8,
                Foreground = ParseBrush("#a09080"),
                Margin = new Thickness(0, 2, 0, 0)
            };
            stack.Children.Add(statsRow);

            // Buff badge
            if (hasBuff && u.IsAlive)
            {
                var buffRow = new WrapPanel { Margin = new Thickness(0, 4, 0, 0) };
                foreach (var b in buffNames)
                {
                    var badge = new Border { Background = ParseBrush("#2a2a1a"), CornerRadius = new CornerRadius(3), Padding = new Thickness(4, 1, 4, 1), Margin = new Thickness(0, 0, 3, 0) };
                    badge.Child = new TextBlock { Text = b, FontSize = 8, Foreground = ParseBrush("#ffd93d") };
                    buffRow.Children.Add(badge);
                }
                stack.Children.Add(buffRow);
            }

            border.Child = stack;
            return border;
        }

        private void RenderBuffs()
        {
            if (_army1 == null || _army2 == null) return;
            Army1BuffsLbl.Text = BuildBuffSummary(_army1);
            Army2BuffsLbl.Text = BuildBuffSummary(_army2);
        }

        private static string BuildBuffSummary(IArmy army)
        {
            var lines = new List<string>();
            foreach (var u in army.Units.Where(x => x.IsAlive))
            {
                var buffs = GetBuffNames(u);
                if (buffs.Count > 0)
                    lines.Add($"#{u.FighterNumber} {u.Name}: {string.Join(", ", buffs)}");
            }
            return lines.Count > 0 ? string.Join("\n", lines) : "Нет баффов";
        }

        // ─── Formation field renderer ───────────────────────────────────────────────

        private void RenderFormationField()
        {
            if (_engine == null || _army1 == null || _army2 == null) return;
            _formation = _engine.GetCurrentFormation();
            FormationLbl.Text = FormatName(_formation);

            CombatGrid.Children.Clear();
            CombatGrid.RowDefinitions.Clear();
            CombatGrid.ColumnDefinitions.Clear();

            switch (_formation)
            {
                case FormationType.OneColumn:
                    RenderOneColumn();
                    break;
                case FormationType.ThreeColumns:
                    RenderThreeColumns();
                    break;
                case FormationType.Wall:
                    RenderWall();
                    break;
            }
        }

        private void RenderOneColumn()
        {
            // Two columns: left=army1 fighter, right=army2 fighter
            CombatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            CombatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // VS
            CombatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            CombatGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var f1 = _engine!.GetCurrentFighter1();
            var f2 = _engine.GetCurrentFighter2();

            var card1 = MakeCombatCard(f1, true, active: true);
            Grid.SetColumn(card1, 0); Grid.SetRow(card1, 0);

            var vsLbl = new TextBlock { Text = "VS", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = ParseBrush("#ffd93d"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(vsLbl, 1); Grid.SetRow(vsLbl, 0);

            var card2 = MakeCombatCard(f2, false, active: true);
            Grid.SetColumn(card2, 2); Grid.SetRow(card2, 0);

            CombatGrid.Children.Add(card1);
            CombatGrid.Children.Add(vsLbl);
            CombatGrid.Children.Add(card2);
        }

        private void RenderThreeColumns()
        {
            if (_engine == null) return;

            CombatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            CombatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            CombatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            CombatGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            for (int col = 0; col < 3; col++)
            {
                var f1 = _engine.GetCurrentFighterInColumn(col, true);
                var f2 = _engine.GetCurrentFighterInColumn(col, false);

                bool hasPair = f1?.IsAlive == true && f2?.IsAlive == true;

                var colBorder = new Border
                {
                    Background = ParseBrush("#0d1520"),
                    BorderBrush = ParseBrush(hasPair ? "#2a3a4a" : "#1a2228"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(4)
                };

                var innerGrid = new Grid();
                innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
                innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });
                innerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                // Название колонны
                var colLbl = new TextBlock
                {
                    Text = hasPair ? $"Колонна {col + 1}" : (f1 == null && f2 == null ? $"Колонна {col + 1} (пусто)" : $"Колонна {col + 1} (ожидание)"),
                    FontSize = 10,
                    Foreground = ParseBrush(hasPair ? "#ffd93d" : "#666"),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 8, 0, 4)
                };
                Grid.SetRow(colLbl, 0);
                innerGrid.Children.Add(colLbl);

                // Боец армии 1 (сверху)
                var c1 = MakeCombatCard(f1, true, f1?.IsAlive == true);
                Grid.SetRow(c1, 1);
                innerGrid.Children.Add(c1);

                // VS
                var vs = new TextBlock
                {
                    Text = hasPair ? "⚔" : (f1?.IsAlive == true && f2 == null ? "→" : (f2?.IsAlive == true ? "←" : "—")),
                    FontSize = 16,
                    Foreground = ParseBrush("#ffd93d"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(vs, 2);
                innerGrid.Children.Add(vs);

                // Боец армии 2 (снизу)
                var c2 = MakeCombatCard(f2, false, f2?.IsAlive == true);
                Grid.SetRow(c2, 3);
                innerGrid.Children.Add(c2);

                colBorder.Child = innerGrid;
                Grid.SetColumn(colBorder, col);
                CombatGrid.Children.Add(colBorder);
            }
        }

        private void RenderWall()
        {
            if (_engine == null || _army1 == null || _army2 == null) return;

            CombatGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            CombatGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            CombatGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Получаем живых бойцов в порядке боя
            var a1Alive = _army1.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();
            var a2Alive = _army2.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();

            // Верхний ряд (армия 1)
            var row1Panel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4)
            };

            if (a1Alive.Count == 0)
            {
                row1Panel.Children.Add(new TextBlock { Text = "☠ ВСЕ ПАЛИ ☠", Foreground = ParseBrush("#ff6b6b"), FontSize = 14, Margin = new Thickness(20) });
            }
            else
            {
                foreach (var u in a1Alive)
                {
                    var card = MakeCombatCard(u, true, true);
                    card.Width = 110;
                    card.Margin = new Thickness(4);
                    row1Panel.Children.Add(card);
                }
            }

            var row1Border = new Border
            {
                Background = ParseBrush("#0d1520"),
                BorderBrush = ParseBrush("#2a3a4a"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8),
                Child = row1Panel
            };
            Grid.SetRow(row1Border, 0);
            CombatGrid.Children.Add(row1Border);

            // Центральная надпись
            var centerText = a1Alive.Count > 0 && a2Alive.Count > 0 ? "⚔ СТЕНКА НА СТЕНКУ ⚔" : "🏆 ПОБЕДИТЕЛЬ ОПРЕДЕЛЯЕТСЯ 🏆";
            var vsRow = new TextBlock
            {
                Text = centerText,
                FontSize = 12,
                Foreground = ParseBrush("#ffd93d"),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(vsRow, 1);
            CombatGrid.Children.Add(vsRow);

            // Нижний ряд (армия 2)
            var row2Panel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4)
            };

            if (a2Alive.Count == 0)
            {
                row2Panel.Children.Add(new TextBlock { Text = "☠ ВСЕ ПАЛИ ☠", Foreground = ParseBrush("#ff6b6b"), FontSize = 14, Margin = new Thickness(20) });
            }
            else
            {
                foreach (var u in a2Alive)
                {
                    var card = MakeCombatCard(u, false, true);
                    card.Width = 110;
                    card.Margin = new Thickness(4);
                    row2Panel.Children.Add(card);
                }
            }

            var row2Border = new Border
            {
                Background = ParseBrush("#0d1520"),
                BorderBrush = ParseBrush("#2a3a4a"),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8),
                Child = row2Panel
            };
            Grid.SetRow(row2Border, 2);
            CombatGrid.Children.Add(row2Border);
        }

        private WrapPanel BuildWallRow(List<IUnit> units, bool isA1)
        {
            var panel = new WrapPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4) };
            foreach (var u in units)
            {
                var card = MakeCombatCard(u, isA1, u.IsAlive);
                card.Width = 110;
                card.Margin = new Thickness(4);
                panel.Children.Add(card);
            }
            return panel;
        }

        private void ShowWinnerAndExit()
        {
            if (_engine == null || _army1 == null || _army2 == null) return;

            string winnerName;
            IArmy winnerArmy;
            IArmy loserArmy;
            bool isDraw = false;

            if (_engine.StalemateReached)
            {
                isDraw = true;
                winnerName = "НИЧЬЯ!";
                winnerArmy = _army1;
                loserArmy = _army2;
            }
            else if (_army1.HasAliveUnits())
            {
                winnerName = _army1.Name;
                winnerArmy = _army1;
                loserArmy = _army2;
            }
            else
            {
                winnerName = _army2.Name;
                winnerArmy = _army2;
                loserArmy = _army1;
            }

            // Получаем статистику
            int winnerAdded = winnerArmy == _army1 ? _engine.Army1AddedFightersCount : _engine.Army2AddedFightersCount;
            int winnerBuffs = winnerArmy == _army1 ? _engine.Army1BuffsAppliedCount : _engine.Army2BuffsAppliedCount;
            int loserAdded = loserArmy == _army1 ? _engine.Army1AddedFightersCount : _engine.Army2AddedFightersCount;
            int loserBuffs = loserArmy == _army1 ? _engine.Army1BuffsAppliedCount : _engine.Army2BuffsAppliedCount;

            // Показываем красивое окно результата
            var resultWindow = new BattleResultWindow(
                winnerName,
                winnerArmy == _army1 ? _army1.Color : _army2.Color,
                winnerArmy,
                loserArmy,
                _engine.MoveCount,
                winnerAdded,
                winnerBuffs,
                loserAdded,
                loserBuffs,
                isDraw
            );

            resultWindow.Owner = this;
            resultWindow.ShowDialog();

            // Сохраняем результат
            SaveBattleResult();

            // Закрываем окно битвы
            this.Close();
        }

        private void SaveBattleResult()
        {
            if (_engine == null || _army1 == null || _army2 == null) return;

            try
            {
                var battleManager = new BattleManager();
                string logName = $"{_army1.Name} vs {_army2.Name}";

                // Собираем ВЕСЬ лог из UI
                var logBuilder = new System.Text.StringBuilder();

                // Добавляем заголовок битвы
                logBuilder.AppendLine($"Битва началась! {_army1.Name} vs {_army2.Name}");
                logBuilder.AppendLine($"Построение: {GetFormationName(_formation)}");
                logBuilder.AppendLine("");

                // Добавляем все записи из лога
                foreach (TextBlock block in LogPanel.Children)
                {
                    logBuilder.AppendLine(block.Text);
                }

                // Добавляем итоговую статистику
                logBuilder.AppendLine("");
                logBuilder.AppendLine("═══════════════════════════════════════");
                logBuilder.AppendLine($"Всего ходов: {_engine.MoveCount}");

                if (_engine.StalemateReached)
                {
                    logBuilder.AppendLine("РЕЗУЛЬТАТ: НИЧЬЯ!");
                }
                else if (_army1.HasAliveUnits())
                {
                    logBuilder.AppendLine($"РЕЗУЛЬТАТ: ПОБЕДИЛА АРМИЯ {_army1.Name}!");
                    logBuilder.AppendLine($"Выжило бойцов: {_army1.AliveCount()}/{_army1.Units.Count}");
                    logBuilder.AppendLine($"Добавлено бойцов: {_engine.Army1AddedFightersCount}");
                    logBuilder.AppendLine($"Надето баффов: {_engine.Army1BuffsAppliedCount}");
                    logBuilder.AppendLine("");
                    logBuilder.AppendLine($"{_army2.Name} (ПРОИГРАВШИЙ):");
                    logBuilder.AppendLine($"Выжило бойцов: 0/{_army2.Units.Count}");
                    logBuilder.AppendLine($"Добавлено бойцов: {_engine.Army2AddedFightersCount}");
                    logBuilder.AppendLine($"Надето баффов: {_engine.Army2BuffsAppliedCount}");
                }
                else
                {
                    logBuilder.AppendLine($"РЕЗУЛЬТАТ: ПОБЕДИЛА АРМИЯ {_army2.Name}!");
                    logBuilder.AppendLine($"Выжило бойцов: {_army2.AliveCount()}/{_army2.Units.Count}");
                    logBuilder.AppendLine($"Добавлено бойцов: {_engine.Army2AddedFightersCount}");
                    logBuilder.AppendLine($"Надето баффов: {_engine.Army2BuffsAppliedCount}");
                    logBuilder.AppendLine("");
                    logBuilder.AppendLine($"{_army1.Name} (ПРОИГРАВШИЙ):");
                    logBuilder.AppendLine($"Выжило бойцов: 0/{_army1.Units.Count}");
                    logBuilder.AppendLine($"Добавлено бойцов: {_engine.Army1AddedFightersCount}");
                    logBuilder.AppendLine($"Надето баффов: {_engine.Army1BuffsAppliedCount}");
                }

                battleManager.SaveBattleLog(logBuilder.ToString(), logName, _army1, _army2, useTimestamp: true);

                // Удаляем файл сохранения если есть
                string savePath = System.IO.Path.Combine("Saves", $"{logName}.json");
                if (System.IO.File.Exists(savePath))
                {
                    System.IO.File.Delete(savePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }

        private string GetFormationName(FormationType formation)
        {
            return formation switch
            {
                FormationType.OneColumn => "Одна колонна (1×1)",
                FormationType.ThreeColumns => "Три колонны (3×3)",
                FormationType.Wall => "Стенка (все×все)",
                _ => "Неизвестно"
            };
        }

        private Border MakeCombatCard(IUnit? unit, bool isA1, bool active)
        {
            string edgeC = isA1 ? C1 : C2;
            var card = new Border
            {
                Background = ParseBrush(active ? (isA1 ? "#0d1f2e" : "#2e0d0d") : "#0d1015"),
                BorderBrush = ParseBrush(active ? edgeC : "#222"),
                BorderThickness = new Thickness(active ? 2 : 1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(8),
                MinWidth = 130
            };

            if (unit == null || !unit.IsAlive)
            {
                card.Child = new TextBlock { Text = unit == null ? "—" : "☠ Пал", FontSize = 14, Foreground = ParseBrush("#444"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                return card;
            }

            double hpPct = unit.MaxHealth > 0 ? (double)unit.Health / unit.MaxHealth : 0;
            string barC = hpPct > 0.5 ? "#2ecc71" : hpPct > 0.2 ? "#ffd93d" : "#ff6b6b";

            var stack = new StackPanel();

            // Icon + name
            var hdr = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 8) };
            hdr.Children.Add(new TextBlock { Text = UnitIcon(unit.Name), FontSize = 28, HorizontalAlignment = HorizontalAlignment.Center });
            hdr.Children.Add(new TextBlock { Text = $"{unit.Name}", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = ParseBrush(edgeC), HorizontalAlignment = HorizontalAlignment.Center });
            hdr.Children.Add(new TextBlock { Text = $"ID: #{unit.FighterNumber}", FontSize = 9, Foreground = ParseBrush("#a09080"), HorizontalAlignment = HorizontalAlignment.Center });
            stack.Children.Add(hdr);

            // HP bar
            var barOuter = new Border { Background = ParseBrush("#0a1419"), Height = 8, CornerRadius = new CornerRadius(4), Margin = new Thickness(0, 0, 0, 4) };
            var barFill = new Border { Background = ParseBrush(barC), Height = 8, CornerRadius = new CornerRadius(4), Width = Math.Max(3, hpPct * 110), HorizontalAlignment = HorizontalAlignment.Left };
            barOuter.Child = barFill;
            stack.Children.Add(barOuter);

            // HP text
            stack.Children.Add(new TextBlock { Text = $"❤ {Math.Max(0, unit.Health)} / {unit.MaxHealth}", FontSize = 10, Foreground = ParseBrush(barC), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 4) });

            // Stats
            stack.Children.Add(new TextBlock { Text = $"⚔ {unit.EffectiveAttack}  🛡 {unit.EffectiveDefence}", FontSize = 9, Foreground = ParseBrush("#a09080"), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 4) });

            // Buffs
            var buffs = GetBuffNames(unit);
            if (buffs.Count > 0)
            {
                var wrap = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center };
                foreach (var b in buffs)
                {
                    var badge = new Border { Background = ParseBrush("#2a2a1a"), CornerRadius = new CornerRadius(3), Padding = new Thickness(4, 1, 4, 1), Margin = new Thickness(2, 0, 0, 0) };
                    badge.Child = new TextBlock { Text = b, FontSize = 8, Foreground = ParseBrush("#ffd93d") };
                    wrap.Children.Add(badge);
                }
                stack.Children.Add(wrap);
            }

            card.Child = stack;
            return card;
        }

        // ─── Log ───────────────────────────────────────────────────────────────────

        private void AddLog(string msg, string hex = "#e0d5c7")
        {
            var blk = new TextBlock
            {
                Text = msg,
                FontSize = 10,
                Foreground = ParseBrush(hex),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 2)
            };
            LogPanel.Children.Add(blk);
            while (LogPanel.Children.Count > 300)
                LogPanel.Children.RemoveAt(0);
            LogScroll?.ScrollToEnd();
        }

        // ─── Misc ──────────────────────────────────────────────────────────────────

        private void FinishBattle()
        {
            _battleOver = true;
            MoveBtn.IsEnabled = false;
            AutoBtn.IsEnabled = false;
        }

        private void RefreshButtons()
        {
            bool over = _battleOver || _engine?.IsCombatActive == false;
            MoveBtn.IsEnabled = !over && !_autoBattleRunning;
            AutoBtn.IsEnabled = !over && !_autoBattleRunning;
            UndoBtn.IsEnabled = _cmd.UndoCount > 0;
            RedoBtn.IsEnabled = _cmd.RedoCount > 0;
        }

        private static List<string> GetBuffNames(IUnit u)
        {
            var list = new List<string>();
            var cur = u;
            while (cur is BuffDecorator d)
            {
                list.Add(d switch
                {
                    HorseBuffDecorator => "🐴 Конь",
                    ShieldBuffDecorator => "🛡 Щит",
                    HelmetBuffDecorator => "⛑ Шлем",
                    SpearBuffDecorator => "🗡 Копьё",
                    _ => "?"
                });
                cur = d.GetInnerUnit();
            }
            return list;
        }

        private static string UnitIcon(string name) => name switch
        {
            "Слабый боец" => "🗡️",
            "Сильный боец" => "🪖",
            "Лучник" => "🏹",
            "Лекарь" => "✙",
            "Маг" => "🔮",
            "Гуляй город" => "🧱",
            _ => "⚔"
        };

        public void LoadBattleLog(string logContent)
        {
            if (string.IsNullOrWhiteSpace(logContent))
                return;

            foreach (var line in logContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                AddLog(line, "#a09080");
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalid, '_');
            }
            return name.Trim();
        }

        private static string FormatName(FormationType f) => f switch
        {
            FormationType.OneColumn => "Одна колонна (1×1)",
            FormationType.ThreeColumns => "Три колонны (3×3)",
            FormationType.Wall => "Стенка (все×все)",
            _ => "—"
        };

        private static Brush ParseBrush(string hex) =>
            new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    }

    // ════════════════════════════════════════════════════════════════════════════
    // Formation pick dialog
    // ════════════════════════════════════════════════════════════════════════════

    public class FormationPickDialog : Window
    {
        public FormationType SelectedFormation { get; private set; }

        public FormationPickDialog(FormationType current)
        {
            Title = "Сменить Построение";
            Width = 400;
            Height = 300;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0f1419"));
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e0d5c7"));
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            SelectedFormation = current;

            var stack = new StackPanel { Margin = new Thickness(20) };
            stack.Children.Add(new TextBlock { Text = "Выберите тип построения:", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffd93d")), Margin = new Thickness(0, 0, 0, 16) });

            AddOption(stack, "🌉 Одна колонна (1 vs 1)", FormationType.OneColumn, "#4ecdc4");
            AddOption(stack, "🏛 Три колонны (3 vs 3)", FormationType.ThreeColumns, "#2ecc71");
            AddOption(stack, "⚔ Стенка (все vs всех)", FormationType.Wall, "#ff6b6b");

            Content = stack;
        }

        private void AddOption(StackPanel parent, string label, FormationType type, string color)
        {
            var btn = new Button
            {
                Content = label,
                Height = 44,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a2030")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 0, 8),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold
            };
            btn.Click += (_, _) => { SelectedFormation = type; DialogResult = true; Close(); };
            parent.Children.Add(btn);
        }


    }
}
