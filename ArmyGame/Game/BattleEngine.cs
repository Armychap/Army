using System;
using System.Linq;
using System.Threading;
using ArmyBattle.Models;
using ArmyBattle.Game.Formations;
using ArmyBattle.Models.Decorators;
using ArmyBattle.Services;

namespace ArmyBattle.Game
{
    /// <summary>
    /// Логика проведения боя между двумя армиями.
    /// занимается только пошаговым выполнением поединка
    /// принимает IArmy, IUnit, не зависит от конкретных реализаций
    /// </summary>
    public class BattleEngine
    {
        private readonly IArmy army1;
        private readonly IArmy army2;
        private int round;
        private readonly Random random;
        private int battleSpeed;
        private IUnit? currentFighter1;
        private IUnit? currentFighter2;
        private bool needNewRoundHeader;
        private bool firstAttackerIsArmy1;
        private int attackTurn; // 0 = первый атакующий, 1 = второй атакующий

        private bool battleInitialized = false;
        private int noLethalActions = 0;
        private const int maxNoLethalActions = 80;
        private bool stalemateReached = false;
        private int moveCount = 0;
        private int noHealthChangeCount = 0;
        private Dictionary<IUnit, int> allUnitsHealthBefore = new Dictionary<IUnit, int>();
        private const int maxNoHealthChangeActions = 30;
        private FormationType currentFormation = FormationType.OneColumn;

        private IUnit?[] currentFightersArmy1 = new IUnit?[3];
        private IUnit?[] currentFightersArmy2 = new IUnit?[3];

        // Единые очереди резерва для обеих армий
        private List<IUnit> army1BackupQueue = new();
        private List<IUnit> army2BackupQueue = new();

        // Стратегия текущего построения
        private IFormationStrategy? _currentStrategy;
        private IUnit? _lastDisplayedFighter1;
        private IUnit? _lastDisplayedFighter2;
        private bool _needDisplayPair = true;
        // Сохранённое состояние колонн для переключения обратно
        private IUnit?[] _savedColumnsArmy1 = new IUnit?[3];
        private IUnit?[] _savedColumnsArmy2 = new IUnit?[3];
        private List<IUnit> _savedBackupArmy1 = new();
        private List<IUnit> _savedBackupArmy2 = new();

        public BattleEngine(IArmy army1, IArmy army2, int speed = 400)
        {
            this.army1 = army1;
            this.army2 = army2;
            round = 0;
            random = new Random();
            battleSpeed = Math.Max(100, Math.Min(1000, speed));
            currentFighter1 = null;
            currentFighter2 = null;
            needNewRoundHeader = true;
            firstAttackerIsArmy1 = false;
            attackTurn = 0;
            battleInitialized = false;
        }

        // Публичные свойства для доступа к состоянию битвы
        public int Round => round;
        public int MoveCount => moveCount;
        public int AttackTurn => attackTurn;
        public bool FirstAttackerIsArmy1 => firstAttackerIsArmy1;
        public bool NeedNewRoundHeader => needNewRoundHeader;
        public bool StalemateReached => stalemateReached;

        public bool IsCombatActive
        {
            get
            {
                if (_currentStrategy != null)
                    return _currentStrategy.IsCombatActive(this);

                if (currentFormation == FormationType.OneColumn)
                    return army1.HasAliveUnits() && army2.HasAliveUnits();
                else
                    return HasActiveColumnPair();
            }
        }

        // Запуск битвы с выводом заголовка и полной симуляцией ходов
        public void StartBattle()
        {
            try
            {
                Console.Clear();
            }
            catch { }

            Console.WriteLine("НАЧАЛО БИТВЫ");
            Console.WriteLine($"{army1.Name} против {army2.Name}");
            Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
            Console.WriteLine();
            Thread.Sleep(1000);

            // подготовка и выполнение всех ходов
            InitializeBattle();
            DoAllMoves();
        }

        private void DisplayRoundHeader()
        {
            if (_currentStrategy != null)
            {
                _currentStrategy.DisplayRoundHeader(this, round);
            }
            else
            {
                // Старая логика как fallback
                if (currentFormation == FormationType.OneColumn)
                {
                    Console.WriteLine();
                    Console.Write($"РАУНД {round}: ");
                    Console.ForegroundColor = army1.Color;
                    Console.Write($"{army1.Name} {currentFighter1?.FighterNumber}");
                    Console.ResetColor();
                    Console.Write($" ({currentFighter1?.PowerLevel}) vs ");
                    Console.ForegroundColor = army2.Color;
                    Console.Write($"{army2.Name} {currentFighter2?.FighterNumber}");
                    Console.ResetColor();
                    Console.WriteLine($" ({currentFighter2?.PowerLevel})");
                }
                else
                {
                    Console.WriteLine($"\nРАУНД {round} (Три колонны)");
                    for (int col = 0; col < 3; col++)
                    {
                        var f1 = currentFightersArmy1[col];
                        var f2 = currentFightersArmy2[col];
                        if (f1 != null && f2 != null && f1.IsAlive && f2.IsAlive)
                        {
                            Console.ForegroundColor = army1.Color;
                            Console.Write($"К{col + 1}: {f1.FighterNumber}({f1.PowerLevel.Substring(0, 3)}) ");
                            Console.ResetColor();
                            Console.Write(" vs ");
                            Console.ForegroundColor = army2.Color;
                            Console.Write($"{f2.FighterNumber}({f2.PowerLevel.Substring(0, 3)})");
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                    }
                    Console.WriteLine(new string('═', 40) + "\n");
                }
            }
        }

        public void DisplayBattleOrder()
        {
            if (_currentStrategy != null)
            {
                _currentStrategy.DisplayBattleOrder(this);
            }
            else
            {
                // Старая логика как fallback
                if (currentFormation == FormationType.OneColumn)
                {
                    Console.WriteLine("Порядок боя");

                    string FormatUnit(IUnit unit)
                    {
                        string shortType = unit.PowerLevel.ToLowerInvariant() switch
                        {
                            "слабый" => "слаб",
                            "маг" => "маг",
                            "стена" => "стен",
                            "гуляй город" => "стен",
                            "лучник" => "луч",
                            "лекарь" => "лек",
                            "сильный" => "сил",
                            _ => unit.PowerLevel.Length <= 4 ? unit.PowerLevel.ToLowerInvariant() : unit.PowerLevel.Substring(0, 4).ToLowerInvariant()
                        };
                        return $"{unit.FighterNumber} ({shortType})";
                    }

                    var order1 = string.Join(" -> ", army1.AliveFightersInBattleOrder.Select(FormatUnit));
                    var order2 = string.Join(" -> ", army2.AliveFightersInBattleOrder.Select(FormatUnit));

                    Console.WriteLine($"{army1.Name}: {order1}");
                    Console.WriteLine($"{army2.Name}: {order2}");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine($"Порядок боя {army1.Name} vs {army2.Name}");
                    for (int col = 0; col < 3; col++)
                    {
                        var f1 = currentFightersArmy1[col];
                        var f2 = currentFightersArmy2[col];
                        Console.Write($"Колонна {col + 1}: ");
                        Console.Write(f1 != null ? $"{f1.FighterNumber}({f1.PowerLevel.Substring(0, 3)})" : "Пусто");
                        Console.Write("  vs  ");
                        Console.Write(f2 != null ? $"{f2.FighterNumber}({f2.PowerLevel.Substring(0, 3)})" : "Пусто");
                        Console.WriteLine();
                    }
                    Console.WriteLine($"Резерв {army1.Name}: {string.Join("->", army1BackupQueue.Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})"))}");
                    Console.WriteLine($"Резерв {army2.Name}: {string.Join("<-", army2BackupQueue.Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})"))}");
                    Console.WriteLine();
                }
            }
        }

        private void DisplayHealthInfo()
        {
            Console.WriteLine($"Здоровье {currentFighter1?.FighterNumber}: {Math.Max(0, currentFighter1?.Health ?? 0)}/{currentFighter1?.MaxHealth ?? 0}");
            Console.WriteLine($"Здоровье {currentFighter2?.FighterNumber}: {Math.Max(0, currentFighter2?.Health ?? 0)}/{currentFighter2?.MaxHealth ?? 0}");
            DisplayBuffsOnUnit(currentFighter1, army1);
            DisplayBuffsOnUnit(currentFighter2, army2);
            Console.WriteLine();
        }

        private void DisplayBuffsOnUnit(IUnit? unit, IArmy army)
        {
            if (unit == null || !unit.IsAlive) return;

            var buffNames = new List<string>();
            var current = unit;
            while (current is BuffDecorator decorator)
            {
                string buffName = decorator switch
                {
                    HorseBuffDecorator => "Конь",
                    ShieldBuffDecorator => "Щит",
                    HelmetBuffDecorator => "Шлем",
                    SpearBuffDecorator => "Копье",
                    _ => "?"
                };
                buffNames.Add(buffName);
                current = decorator.GetInnerUnit();
            }

            if (buffNames.Any())
            {
                Console.WriteLine($"Бафы {unit.FighterNumber}: {string.Join(", ", buffNames)}");
            }
        }

        // Выполняет один удар от attacker к defender, выводит сообщения и обновляет очередь.
        private void PerformAttack(IArmy attackingArmy, IArmy defendingArmy, ref IUnit attacker, ref IUnit defender)
        {
            // Если атакующий не имеет атаки (например, ShieldWall), он пропускает ход
            if (attacker.EffectiveAttack == 0)
            {
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write(attacker.GetDisplayName(attackingArmy.Name));
                Console.ResetColor();
                Console.Write(" пропускает ход (нет атаки)");
                Console.WriteLine();

                noLethalActions++;
                if (noLethalActions >= maxNoLethalActions)
                {
                    stalemateReached = true;
                    Console.WriteLine("Патовая ситуация: слишком много ходов без смертей. Битва объявлена ничьей.");
                }

                needNewRoundHeader = false;
                attackTurn = 1 - attackTurn;
                return;
            }

            // Выводим текущую пару если нужно
            if (_needDisplayPair && attacker.IsAlive && defender.IsAlive)
            {
                Console.WriteLine();
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write($"{attacker.GetDisplayName(attackingArmy.Name)} ({attacker.PowerLevel})");
                Console.ResetColor();
                Console.Write(" vs ");
                Console.ForegroundColor = defendingArmy.Color;
                Console.Write($"{defender.GetDisplayName(defendingArmy.Name)} ({defender.PowerLevel})");
                Console.ResetColor();
                Console.WriteLine();
                _needDisplayPair = false;
            }

            Console.ForegroundColor = attackingArmy.Color;
            Console.Write(attacker.GetDisplayName(attackingArmy.Name));
            Console.ResetColor();
            Console.Write(" бьет ");
            Console.ForegroundColor = defendingArmy.Color;
            Console.Write($"{defender.GetDisplayName(defendingArmy.Name)}");
            Console.ResetColor();
            Console.WriteLine();

            int healthBefore = defender.Health;
            attacker.AttackUnit(defender);
            int damage = healthBefore - defender.Health;

            Console.WriteLine($"Урон: {damage}");
            DisplayHealthInfo();

            if (!defender.IsAlive)
            {
                Console.WriteLine();
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write(attacker.GetDisplayName(attackingArmy.Name));
                Console.ResetColor();
                Console.Write(" убивает ");
                Console.ForegroundColor = defendingArmy.Color;
                Console.Write(defender.GetDisplayName(defendingArmy.Name));
                Console.ResetColor();
                Console.WriteLine();

                defendingArmy?.RemoveDeadFighter(defender);

                // Сохраняем старого бойца для сравнения
                var oldDefender = defender;
                defender = defendingArmy.GetNextFighterInBattleOrder();

                noLethalActions = 0;
                noHealthChangeCount = 0;

                // Если defender изменился (пришёл новый боец), нужно показать новую пару
                if (oldDefender != defender)
                {
                    _needDisplayPair = true;

                    // Если новый defender есть и attacker жив, показываем новую пару
                    if (defender != null && attacker.IsAlive)
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write($"{attacker.GetDisplayName(attackingArmy.Name)} ({attacker.PowerLevel})");
                        Console.ResetColor();
                        Console.Write(" vs ");
                        Console.ForegroundColor = defendingArmy.Color;
                        Console.Write($"{defender.GetDisplayName(defendingArmy.Name)} ({defender.PowerLevel})");
                        Console.ResetColor();
                        Console.WriteLine();
                        _needDisplayPair = false;
                    }
                }
            }
            else
            {
                noLethalActions++;
                if (noLethalActions >= maxNoLethalActions)
                {
                    stalemateReached = true;
                    Console.WriteLine("Патовая ситуация: слишком много ходов без смертей. Битва объявлена ничьей.");
                }
            }

            attackTurn = 1 - attackTurn;
        }

        private void ReplaceUnitInArmy(IUnit oldUnit, IUnit newUnit)
        {
            var army = oldUnit.Army;
            if (army == null) return;

            // Заменяем в общем списке
            int index = army.Units.IndexOf(oldUnit);
            if (index >= 0)
                army.Units[index] = newUnit;

            // Заменяем в порядке боя
            int orderIndex = army.AliveFightersInBattleOrder.IndexOf(oldUnit);
            if (orderIndex >= 0)
                army.AliveFightersInBattleOrder[orderIndex] = newUnit;

            // Если это текущий боец - обновляем
            if (currentFighter1 == oldUnit) currentFighter1 = newUnit;
            if (currentFighter2 == oldUnit) currentFighter2 = newUnit;
        }

        // Завершение битвы и вывод результатов
        private void EndBattle()
        {
            Console.WriteLine();
            Console.WriteLine("БИТВА ЗАВЕРШЕНА");
            Console.WriteLine(new string('=', 40));

            bool army1Wins = army1.HasAliveUnits();
            bool army2Wins = army2.HasAliveUnits();

            // Проверяем, была ли ничья
            if (stalemateReached && army1Wins && army2Wins)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("НИЧЬЯ!");
                Console.ResetColor();
            }
            else if (army1Wins)
            {
                Console.ForegroundColor = army1.Color;
                Console.WriteLine($"ПОБЕДИТЕЛЬ: {army1.Name}!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = army2.Color;
                Console.WriteLine($"ПОБЕДИТЕЛЬ: {army2.Name}!");
                Console.ResetColor();
            }
        }

        // Инициализация без вывода текста
        public void InitializeBattle(FormationType formation = FormationType.OneColumn)
        {
            if (battleInitialized)
                return;

            currentFormation = formation;
            SetFormationStrategy(formation);

            if (_currentStrategy != null)
            {
                _currentStrategy.Initialize(this);
            }
            // Убираем else блок, так как стратегия уже создана в SetFormationStrategy

            round = 0;
            moveCount = 0;
            noHealthChangeCount = 0;
            needNewRoundHeader = true;
            attackTurn = 0;
            battleInitialized = true;

            _needDisplayPair = true;
            _lastDisplayedFighter1 = null;
            _lastDisplayedFighter2 = null;
        }

        // Установить состояние битвы для продолжения
        public void SetBattleState(int currentRound, int attackTurn, bool firstAttackerIsArmy1, bool needNewRoundHeader)
        {
            this.round = currentRound;
            this.attackTurn = attackTurn;
            this.firstAttackerIsArmy1 = firstAttackerIsArmy1;
            this.needNewRoundHeader = needNewRoundHeader;
            battleInitialized = true;
        }

        public void SetMoveCount(int count)
        {
            this.moveCount = count;
        }

        // Выбрать текущего бойца для армии (без продвижения индекса)
        private IUnit? SelectFighterForArmy(IArmy army)
        {
            if (army.CurrentFighterIndex < army.AliveFightersInBattleOrder.Count)
            {
                return army.AliveFightersInBattleOrder[army.CurrentFighterIndex];
            }
            return null;
        }

        // Установить текущих бойцов
        public void SetCurrentFightersForContinuation()
        {
            currentFighter1 = SelectFighterForArmy(army1);
            currentFighter2 = SelectFighterForArmy(army2);
        }

        // Установить флаг инициализации битвы
        public void SetBattleInitialized(bool initialized)
        {
            battleInitialized = initialized;
        }

        // Выполнить один полный раунд (оба удара + проверка специальных способностей)
        public bool DoSingleRound()
        {
            if (!battleInitialized)
            {
                InitializeBattle();
            }

            if (!(army1.HasAliveUnits() && army2.HasAliveUnits()))
            {
                Console.WriteLine("Битва завершена: одна из армий не имеет живых юнитов");
                return false; // Битва закончена
            }

            Console.WriteLine($"Начинаем раунд {round}");

            // Выполняем ходы пока раунд не закончится (т.е. пока никто не умер)
            while (!needNewRoundHeader)
            {
                if (!DoSingleMove())
                    return false;
            }

            // После раунда не увеличиваем round здесь, он меняется в PerformAttack при смерти
            needNewRoundHeader = false;

            return true;
        }

        // Выполнить один ход (один удар) в битве
        public bool DoSingleMove()
        {
            if (!battleInitialized)
            {
                InitializeBattle();
                // При инициализации показываем первую пару
                _needDisplayPair = true;
            }

            if (stalemateReached)
            {
                Console.WriteLine("Битва прекращена: патовая ситуация.");
                return false;
            }

            // Проверка окончания битвы для обоих режимов
            // Проверка окончания битвы для всех режимов
            if (currentFormation == FormationType.OneColumn)
            {
                bool alive1 = army1.HasAliveUnits();
                bool alive2 = army2.HasAliveUnits();
                if (!alive1 || !alive2)
                {
                    Console.WriteLine($"[DEBUG] Завершение: армия1 живых={alive1}, армия2 живых={alive2}");
                    Console.WriteLine($"  {army1.Name}: {string.Join(", ", army1.Units.Where(u => u.IsAlive).Select(u => u.FighterNumber))}");
                    Console.WriteLine($"  {army2.Name}: {string.Join(", ", army2.Units.Where(u => u.IsAlive).Select(u => u.FighterNumber))}");
                    return false;
                }
            }
            else if (currentFormation == FormationType.ThreeColumns)
            {
                if (!HasActiveColumnPair()) return false;
            }
            else // Wall
            {
                if (_currentStrategy != null && !_currentStrategy.IsCombatActive(this)) return false;
            }

            moveCount++;
            Console.WriteLine($"Ход {moveCount}");

            // === ЛОГИКА БАФФОВ ===
            var army1StrongFighters = army1.Units
                .Where(u => u.IsAlive && u != currentFighter1 && u != currentFighter2
                            && IsStrongFighter(u) && CanEquipBuff(u, u.Army))
                .ToList();

            if (army1StrongFighters.Any())
            {
                var chosen = army1StrongFighters[random.Next(army1StrongFighters.Count)];
                EquipBuff(chosen);
            }

            var army2StrongFighters = army2.Units
                .Where(u => u.IsAlive && u != currentFighter1 && u != currentFighter2
                            && IsStrongFighter(u) && CanEquipBuff(u, u.Army))
                .ToList();

            if (army2StrongFighters.Any())
            {
                var chosen = army2StrongFighters[random.Next(army2StrongFighters.Count)];
                EquipBuff(chosen);
            }

            // === ПРОВЕРКА ИЗМЕНЕНИЯ ЗДОРОВЬЯ ===
            allUnitsHealthBefore.Clear();
            foreach (var unit in army1.Units.Concat(army2.Units).Where(u => u.IsAlive))
            {
                allUnitsHealthBefore[unit] = unit.Health;
            }

            bool anyAction = false;

            if (_currentStrategy != null)
            {
                anyAction = _currentStrategy.ProcessMove(this);
            }
            else
            {
                // Fallback для обратной совместимости
                if (currentFormation == FormationType.OneColumn)
                {
                    bool currentAttackerIsArmy1 = attackTurn == 0 ? firstAttackerIsArmy1 : !firstAttackerIsArmy1;
                    if (currentAttackerIsArmy1)
                        PerformAttack(army1, army2, ref currentFighter1, ref currentFighter2);
                    else
                        PerformAttack(army2, army1, ref currentFighter2, ref currentFighter1);
                    anyAction = true;
                }
                else
                {
                    anyAction = ProcessThreeColumnMove();
                }
            }

            // Проверка специальных способностей
            if (currentFighter1?.IsAlive == true && currentFighter2?.IsAlive == true)
                CheckAndExecuteSpecialAbilities();
            else if (currentFormation == FormationType.ThreeColumns)
                CheckSpecialAbilitiesThreeColumns();

            // === ПРОВЕРКА СТАГНАЦИИ ===
            bool anyHealthChanged = false;
            foreach (var unit in army1.Units.Concat(army2.Units).Where(u => u.IsAlive))
            {
                if (allUnitsHealthBefore.ContainsKey(unit) && allUnitsHealthBefore[unit] != unit.Health)
                {
                    anyHealthChanged = true;
                    break;
                }
            }

            if (!anyHealthChanged)
            {
                noHealthChangeCount++;
                if (noHealthChangeCount >= maxNoHealthChangeActions)
                {
                    stalemateReached = true;
                    Console.WriteLine();
                    Console.WriteLine("НИЧЬЯ: Жизнь ни одного бойца не изменялась в течение 10 ходов!");
                    return false;
                }
            }
            else
            {
                noHealthChangeCount = 0;
            }

            return anyAction;
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ТРЁХ КОЛОНН ===

        public bool HasActiveColumnPair()
        {
            for (int col = 0; col < 3; col++)
            {
                if (currentFightersArmy1[col]?.IsAlive == true && currentFightersArmy2[col]?.IsAlive == true)
                    return true;
            }
            return false;
        }

        public bool ProcessThreeColumnMove()
        {
            bool anyAction = false;

            for (int col = 0; col < 3; col++)
            {
                var fighter1 = currentFightersArmy1[col];
                var fighter2 = currentFightersArmy2[col];

                if (fighter1 == null || fighter2 == null || !fighter1.IsAlive || !fighter2.IsAlive)
                    continue;

                anyAction = true;

                bool army1AttacksFirst = (col + attackTurn) % 2 == 0;

                if (army1AttacksFirst)
                {
                    PerformAttackInColumn(army1, army2, ref fighter1, ref fighter2, col);
                    if (fighter1?.IsAlive == true && fighter2?.IsAlive == true)
                    {
                        PerformAttackInColumn(army2, army1, ref fighter2, ref fighter1, col);
                    }
                }
                else
                {
                    PerformAttackInColumn(army2, army1, ref fighter2, ref fighter1, col);
                    if (fighter1?.IsAlive == true && fighter2?.IsAlive == true)
                    {
                        PerformAttackInColumn(army1, army2, ref fighter1, ref fighter2, col);
                    }
                }

                currentFightersArmy1[col] = fighter1;
                currentFightersArmy2[col] = fighter2;
            }

            return anyAction;
        }

        private void PerformAttackInColumn(IArmy attackingArmy, IArmy defendingArmy,
            ref IUnit? attacker, ref IUnit? defender, int column)
        {
            if (attacker?.EffectiveAttack == 0)
            {
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write(attacker?.GetDisplayName(attackingArmy.Name));
                Console.ResetColor();
                Console.WriteLine(" пропускает ход (нет атаки)");
                noLethalActions++;
                needNewRoundHeader = false;
                attackTurn = 1 - attackTurn;
                return;
            }

            Console.ForegroundColor = attackingArmy.Color;
            Console.Write(attacker?.GetDisplayName(attackingArmy.Name));
            Console.ResetColor();
            Console.Write(" бьет ");
            Console.ForegroundColor = defendingArmy.Color;
            Console.Write(defender?.GetDisplayName(defendingArmy.Name));
            Console.ResetColor();
            Console.WriteLine();

            int healthBefore = defender!.Health;
            attacker.AttackUnit(defender);
            int damage = healthBefore - defender.Health;
            Console.WriteLine($"Урон: {damage}");
            Console.WriteLine($"Здоровье {defender.FighterNumber}: {Math.Max(0, defender.Health)}/{defender.MaxHealth}");

            if (!defender.IsAlive)
            {
                Console.WriteLine();
                Console.ForegroundColor = attackingArmy.Color;
                Console.Write(attacker.GetDisplayName(attackingArmy.Name));
                Console.ResetColor();
                Console.Write(" убивает ");
                Console.ForegroundColor = defendingArmy.Color;
                Console.Write(defender.GetDisplayName(defendingArmy.Name));
                Console.ResetColor();
                Console.WriteLine();

                bool isArmy1Dead = defendingArmy == army1;
                ReplaceDeadFighterInColumn(column, defendingArmy == army1);
                defender = isArmy1Dead ? currentFightersArmy1[column] : currentFightersArmy2[column];

                noLethalActions = 0;
                noHealthChangeCount = 0;
                needNewRoundHeader = true;
            }
            else
            {
                needNewRoundHeader = false;
                noLethalActions++;
            }

            attackTurn = 1 - attackTurn;
        }

        private void ReplaceDeadFighterInColumn(int column, bool isArmy1Dead)
        {
            if (isArmy1Dead)
            {
                if (army1BackupQueue.Count > 0)
                {
                    currentFightersArmy1[column] = army1BackupQueue[0];
                    army1BackupQueue.RemoveAt(0);
                }
                else currentFightersArmy1[column] = null;
            }
            else
            {
                if (army2BackupQueue.Count > 0)
                {
                    currentFightersArmy2[column] = army2BackupQueue[0];
                    army2BackupQueue.RemoveAt(0);
                }
                else currentFightersArmy2[column] = null;
            }
        }

        private void CheckSpecialAbilitiesThreeColumns()
        {
            for (int col = 0; col < 3; col++)
            {
                var f1 = currentFightersArmy1[col];
                var f2 = currentFightersArmy2[col];
                if (f1?.IsAlive == true && f2?.IsAlive == true)
                {
                    var oldF1 = currentFighter1; var oldF2 = currentFighter2;
                    currentFighter1 = f1; currentFighter2 = f2;
                    CheckAndExecuteSpecialAbilities();
                    currentFighter1 = oldF1; currentFighter2 = oldF2;
                }
            }
        }

        // Выполнить все ходы до конца битвы
        public void DoAllMoves()
        {
            if (!battleInitialized)
            {
                InitializeBattle();
            }

            while (DoSingleMove())
            {
                Thread.Sleep(battleSpeed);
                Thread.Sleep(200);
            }

            EndBattle();
        }

        private ConsoleColor GetAbilityColor(Type unitType)
        {
            if (unitType == typeof(Archer)) return ConsoleColor.Yellow;
            if (unitType == typeof(Wizard)) return ConsoleColor.Magenta;
            if (unitType == typeof(Healer)) return ConsoleColor.Green;
            return ConsoleColor.White;
        }

        private void CheckAndExecuteSpecialAbilities()
        {
            if (currentFighter1 == null || currentFighter2 == null || !currentFighter1.IsAlive || !currentFighter2.IsAlive)
                return;

            Console.WriteLine();

            // Сначала лучники
            ExecuteSpecialAbilitiesForArmy(army1, army2, typeof(Archer));
            if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
                ExecuteSpecialAbilitiesForArmy(army2, army1, typeof(Archer));

            // Потом маги (раньше лекарей)
            if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
            {
                ExecuteSpecialAbilitiesForArmy(army1, army2, typeof(Wizard));
                if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
                    ExecuteSpecialAbilitiesForArmy(army2, army1, typeof(Wizard));
            }

            // Затем лекари
            if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
            {
                ExecuteSpecialAbilitiesForArmy(army1, army2, typeof(Healer));
                if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
                    ExecuteSpecialAbilitiesForArmy(army2, army1, typeof(Healer));
            }

            Console.WriteLine();
        }

        private void ExecuteSpecialAbilitiesForArmy(IArmy attackingArmy, IArmy defendingArmy, Type? unitType = null)
        {
            var unitsCopy = attackingArmy.Units.ToList();
            foreach (var unit in unitsCopy)
            {
                if (!unit.IsAlive)
                    continue;

                if (unitType != null && unit.GetRootType() != unitType)
                    continue;

                if (unit == currentFighter1 || unit == currentFighter2)
                    continue;

                if (unit.SpecialAbility == null)
                    continue;

                var realUnit = unit.GetRootUnit();
                bool isHealing = realUnit is Healer;
                bool isCloning = realUnit is Wizard;
                IUnit? target;

                if (realUnit is Archer)
                {
                    int range = random.Next(1, defendingArmy.AliveCount() + 1);
                    var possibleTargets = defendingArmy.AliveFightersInBattleOrder.Where((u, index) => index < range && u.IsAlive).ToList();
                    if (possibleTargets.Count == 0) continue;
                    target = possibleTargets[random.Next(possibleTargets.Count)];
                }
                else if (isCloning)
                {
                    var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && u != unit && !u.Is<Healer>() && !u.Is<StrongFighter>() && !u.Is<Wizard>()).ToList();
                    if (possibleTargets.Count == 0) continue;
                    target = possibleTargets[random.Next(possibleTargets.Count)];
                }
                else if (isHealing)
                {
                    var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && !u.Is<StrongFighter>()).ToList();
                    if (possibleTargets.Count == 0) continue;

                    var filtered = possibleTargets.Where(u => u != unit).ToList();
                    if (filtered.Count > 0)
                    {
                        target = filtered[random.Next(filtered.Count)];
                    }
                    else
                    {
                        target = unit;
                    }
                }
                else
                {
                    target = attackingArmy == army1 ? currentFighter2 : currentFighter1;
                    if (target == null || !target.IsAlive)
                        continue;
                }

                if (unit.CanUseSpecialAbility(target))
                {
                    int healthBefore = (isHealing || isCloning) ? 0 : (target?.Health ?? 0);

                    if (isCloning)
                    {
                        unit.UseSpecialAbility(target);
                    }
                    else if (realUnit is Archer)
                    {
                        unit.UseSpecialAbility(target);
                    }

                    ConsoleColor abilityColor = GetAbilityColor(realUnit.GetType());

                    if (!isHealing)
                    {
                        string unitTypeName = realUnit is Archer ? "лучник" : "маг";

                        if (realUnit is Wizard)
                        {
                            if (unit.SpecialAbility is CloneAbility ca && ca.ChosenToClone != null)
                            {
                                Console.ForegroundColor = abilityColor;
                                Console.Write(unitTypeName + " ");
                                Console.ForegroundColor = attackingArmy.Color;
                                Console.Write(unit.GetDisplayName(attackingArmy.Name));
                                Console.ForegroundColor = abilityColor;
                                Console.Write(" клонирует ");
                                Console.ForegroundColor = abilityColor;
                                Console.Write($"({ca.ChosenToClone.PowerLevel}) ");
                                Console.ForegroundColor = attackingArmy.Color;
                                Console.Write(ca.ChosenToClone.GetDisplayName(attackingArmy.Name));
                                Console.ResetColor();
                                Console.WriteLine();
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = abilityColor;
                            Console.Write(unitTypeName + " ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(unit.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" стреляет в ");
                            Console.ForegroundColor = defendingArmy.Color;
                            Console.Write(target?.GetDisplayName(defendingArmy.Name));
                            Console.ResetColor();
                            Console.WriteLine();
                        }

                        if (!(realUnit is Wizard))
                        {
                            int damage = healthBefore - (target?.Health ?? 0);
                            Console.WriteLine($"Урон: {damage}");
                            Console.Write($"Здоровье ");
                            Console.ForegroundColor = defendingArmy.Color;
                            Console.Write(target?.GetDisplayName(defendingArmy.Name));
                            Console.ResetColor();
                            Console.WriteLine($": {Math.Max(0, target?.Health ?? 0)}/{target?.MaxHealth ?? 0}");
                        }

                        if (!target?.IsAlive ?? false)
                        {
                            Console.ForegroundColor = abilityColor;
                            Console.Write(unitTypeName + " ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(unit.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" убивает ");
                            Console.ForegroundColor = defendingArmy.Color;
                            Console.Write(target?.GetDisplayName(defendingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" специальной способностью!");
                            Console.ResetColor();
                            Console.WriteLine();

                            defendingArmy.RemoveDeadFighter(target);
                            if (defendingArmy == army1)
                                currentFighter1 = army1.GetNextFighterInBattleOrder();
                            else
                                currentFighter2 = army2.GetNextFighterInBattleOrder();
                            needNewRoundHeader = true;
                        }
                    }

                    if (isHealing)
                    {
                        unit.UseSpecialAbility(target);
                    }

                    if (isHealing)
                    {
                        if (unit.SpecialAbility is SpecialAbility sa && sa.LastHealed != null)
                        {
                            Console.ForegroundColor = abilityColor;
                            Console.Write("лекарь ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(unit.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" лечит ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(sa.LastHealed.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.ResetColor();
                            Console.WriteLine();

                            Console.Write($"Здоровье ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(sa.LastHealed.GetDisplayName(attackingArmy.Name));
                            Console.ResetColor();
                            Console.WriteLine($": {sa.LastHealed.Health}/{sa.LastHealed.MaxHealth}");
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Разделяет армии на 3 колонны для режима "Три колонны"
        /// Теперь пары формируются последовательно, как в режиме "Стенка"
        /// </summary>
        public void InitializeThreeColumnBattle()
        {
            currentFormation = FormationType.ThreeColumns;
            army1BackupQueue.Clear();
            army2BackupQueue.Clear();

            for (int i = 0; i < 3; i++)
            {
                currentFightersArmy1[i] = null;
                currentFightersArmy2[i] = null;
            }

            var alive1 = army1.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();
            var alive2 = army2.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();

            // Берем пары последовательно, как в стенке (первые N пар)
            int pairsToTake = Math.Min(3, Math.Min(alive1.Count, alive2.Count));

            for (int i = 0; i < pairsToTake; i++)
            {
                currentFightersArmy1[i] = alive1[i];
                currentFightersArmy2[i] = alive2[i];
            }

            // Остальные уходят в резерв
            army1BackupQueue.AddRange(alive1.Skip(pairsToTake));
            army2BackupQueue.AddRange(alive2.Skip(pairsToTake));
        }

        /// <summary>
        /// Пересоздаёт боевой порядок при смене построения во время боя.
        /// </summary>
        /// <summary>
        /// Пересоздаёт боевой порядок при смене построения во время боя.
        /// </summary>
        public void ReinitializeFormation(FormationType newFormation)
        {
            if (currentFormation == newFormation) return;

            // === СОХРАНЯЕМ ТЕКУЩЕЕ СОСТОЯНИЕ ===
            if (currentFormation == FormationType.ThreeColumns)
            {
                for (int i = 0; i < 3; i++)
                {
                    _savedColumnsArmy1[i] = currentFightersArmy1[i];
                    _savedColumnsArmy2[i] = currentFightersArmy2[i];
                }
                _savedBackupArmy1 = new List<IUnit>(army1BackupQueue);
                _savedBackupArmy2 = new List<IUnit>(army2BackupQueue);
            }
            else if (currentFormation == FormationType.OneColumn)
            {
                _savedColumnsArmy1[0] = currentFighter1;
                _savedColumnsArmy2[0] = currentFighter2;
            }

            currentFormation = newFormation;
            SetFormationStrategy(newFormation);

            // === ВОССТАНАВЛИВАЕМ СОХРАНЁННОЕ СОСТОЯНИЕ ===
            if (newFormation == FormationType.ThreeColumns)
            {
                // Если есть сохранённые колонны - восстанавливаем
                if (_savedColumnsArmy1[0] != null || _savedColumnsArmy2[0] != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        currentFightersArmy1[i] = _savedColumnsArmy1[i];
                        currentFightersArmy2[i] = _savedColumnsArmy2[i];
                    }
                    army1BackupQueue = new List<IUnit>(_savedBackupArmy1);
                    army2BackupQueue = new List<IUnit>(_savedBackupArmy2);
                }
                else
                {
                    InitializeThreeColumnBattle();
                }
                _currentStrategy?.Initialize(this);
            }
            else if (newFormation == FormationType.OneColumn &&
                     (_savedColumnsArmy1[0] != null || _savedColumnsArmy2[0] != null))
            {
                currentFighter1 = _savedColumnsArmy1[0];
                currentFighter2 = _savedColumnsArmy2[0];

                if (currentFighter1?.IsAlive != true)
                    currentFighter1 = army1.GetNextFighterInBattleOrder();
                if (currentFighter2?.IsAlive != true)
                    currentFighter2 = army2.GetNextFighterInBattleOrder();
            }
            else
            {
                _currentStrategy?.Initialize(this);
            }

            needNewRoundHeader = true;
        }
        // === НОВЫЕ ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ СТРАТЕГИЙ ===

        public IArmy GetArmy1() => army1;
        public IArmy GetArmy2() => army2;
        public IUnit? GetCurrentFighter1() => currentFighter1;
        public IUnit? GetCurrentFighter2() => currentFighter2;
        public void SetCurrentFighter1(IUnit? fighter) => currentFighter1 = fighter;
        public void SetCurrentFighter2(IUnit? fighter) => currentFighter2 = fighter;
        public Random GetRandom() => random;
        public List<IUnit> GetArmy1BackupQueue() => army1BackupQueue;
        public List<IUnit> GetArmy2BackupQueue() => army2BackupQueue;

        public IUnit? GetCurrentFighterInColumn(int column, bool isArmy1)
        {
            return isArmy1 ? currentFightersArmy1[column] : currentFightersArmy2[column];
        }

        public bool ProcessOneColumnMove()
        {
            bool currentAttackerIsArmy1 = attackTurn == 0 ? firstAttackerIsArmy1 : !firstAttackerIsArmy1;
            if (currentAttackerIsArmy1)
                PerformAttack(army1, army2, ref currentFighter1, ref currentFighter2);
            else
                PerformAttack(army2, army1, ref currentFighter2, ref currentFighter1);
            return true;
        }

        public void InitializeThreeColumns()
        {
            InitializeThreeColumnBattle();
        }

        public void ReinitializeThreeColumns()
        {
            army1BackupQueue.Clear();
            army2BackupQueue.Clear();

            for (int i = 0; i < 3; i++)
            {
                currentFightersArmy1[i] = null;
                currentFightersArmy2[i] = null;
            }

            var alive1 = army1.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();
            var alive2 = army2.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();

            // Берем пары последовательно
            int pairsToTake = Math.Min(3, Math.Min(alive1.Count, alive2.Count));

            for (int i = 0; i < pairsToTake; i++)
            {
                currentFightersArmy1[i] = alive1[i];
                currentFightersArmy2[i] = alive2[i];
            }

            army1BackupQueue.AddRange(alive1.Skip(pairsToTake));
            army2BackupQueue.AddRange(alive2.Skip(pairsToTake));
        }

        public void SetNeedRebuildPairs(bool value)
        {
            // Для WallStrategy - будет использоваться через стратегию
        }

        public void SetFormationStrategy(FormationType type)
        {
            currentFormation = type;
            _currentStrategy = type switch
            {
                FormationType.OneColumn => new OneColumnStrategy(),
                FormationType.ThreeColumns => new ThreeColumnsStrategy(),
                FormationType.Wall => new WallStrategy(),
                _ => new OneColumnStrategy()
            };
        }

        public IFormationStrategy? GetCurrentStrategy() => _currentStrategy;

        // Добавить в BattleEngine.cs

        public ref IUnit? GetCurrentFighter1Ref()
        {
            return ref currentFighter1;
        }

        public ref IUnit? GetCurrentFighter2Ref()
        {
            return ref currentFighter2;
        }

        public void PerformOneColumnAttack(IArmy attackingArmy, IArmy defendingArmy,
            ref IUnit attacker, ref IUnit defender)
        {
            PerformAttack(attackingArmy, defendingArmy, ref attacker, ref defender);
        }

        public void PerformAttackInColumnPublic(IArmy attackingArmy, IArmy defendingArmy,
            ref IUnit? attacker, ref IUnit? defender, int column)
        {
            PerformAttackInColumn(attackingArmy, defendingArmy, ref attacker, ref defender, column);
        }

        public void UpdateCurrentFighterInColumn(int column, bool isArmy1, IUnit? fighter)
        {
            if (isArmy1)
                currentFightersArmy1[column] = fighter;
            else
                currentFightersArmy2[column] = fighter;
        }


        // Добавить в BattleEngine.cs

        /// <summary>
        /// Проверяет специальные способности для бойцов, которые не участвовали в атаках
        /// </summary>
        /// <summary>
        /// Проверяет специальные способности для бойцов, которые не участвовали в атаках
        /// </summary>
        public void CheckAndExecuteSpecialAbilitiesForNonAttackers(List<IUnit> fightersWhoAttacked)
        {
            Console.WriteLine();

            //  СОЗДАЁМ КОПИЮ списка, чтобы избежать ошибки изменения коллекции
            var army1UnitsCopy = army1.Units.ToList();
            var army2UnitsCopy = army2.Units.ToList();

            // Проверяем всех бойцов армии 1 (по копии)
            foreach (var unit in army1UnitsCopy)
            {
                if (!unit.IsAlive) continue;
                if (fightersWhoAttacked.Contains(unit)) continue;
                if (unit.SpecialAbility == null) continue;

                var oldF1 = currentFighter1;
                var oldF2 = currentFighter2;
                currentFighter1 = unit;

                ExecuteSpecialAbilitiesForSingleUnit(unit, army1, army2);

                currentFighter1 = oldF1;
                currentFighter2 = oldF2;
            }

            // Проверяем всех бойцов армии 2 (по копии)
            foreach (var unit in army2UnitsCopy)
            {
                if (!unit.IsAlive) continue;
                if (fightersWhoAttacked.Contains(unit)) continue;
                if (unit.SpecialAbility == null) continue;

                var oldF1 = currentFighter1;
                var oldF2 = currentFighter2;
                currentFighter2 = unit;

                ExecuteSpecialAbilitiesForSingleUnit(unit, army2, army1);

                currentFighter1 = oldF1;
                currentFighter2 = oldF2;
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Выполняет специальную способность для одного юнита
        /// </summary>
        private void ExecuteSpecialAbilitiesForSingleUnit(IUnit unit, IArmy attackingArmy, IArmy defendingArmy)
        {
            var realUnit = unit.GetRootUnit();
            bool isHealing = realUnit is Healer;
            bool isCloning = realUnit is Wizard;
            IUnit? target;

            if (realUnit is Archer)
            {
                int range = random.Next(1, defendingArmy.AliveCount() + 1);
                var possibleTargets = defendingArmy.AliveFightersInBattleOrder.Where((u, index) => index < range && u.IsAlive).ToList();
                if (possibleTargets.Count == 0) return;
                target = possibleTargets[random.Next(possibleTargets.Count)];
            }
            else if (isCloning)
            {
                var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && u != unit && !u.Is<Healer>() && !u.Is<StrongFighter>() && !u.Is<Wizard>()).ToList();
                if (possibleTargets.Count == 0) return;
                target = possibleTargets[random.Next(possibleTargets.Count)];
            }
            else if (isHealing)
            {
                var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && !u.Is<StrongFighter>()).ToList();
                if (possibleTargets.Count == 0) return;

                var filtered = possibleTargets.Where(u => u != unit).ToList();
                target = filtered.Count > 0 ? filtered[random.Next(filtered.Count)] : unit;
            }
            else
            {
                target = attackingArmy == army1 ? currentFighter2 : currentFighter1;
                if (target == null || !target.IsAlive) return;
            }

            if (unit.CanUseSpecialAbility(target))
            {
                int healthBefore = (isHealing || isCloning) ? 0 : (target?.Health ?? 0);

                if (isCloning)
                {
                    unit.UseSpecialAbility(target);
                }
                else if (realUnit is Archer)
                {
                    unit.UseSpecialAbility(target);
                }

                ConsoleColor abilityColor = GetAbilityColor(realUnit.GetType());

                if (!isHealing)
                {
                    string unitTypeName = realUnit is Archer ? "лучник" : "маг";

                    if (realUnit is Wizard)
                    {
                        if (unit.SpecialAbility is CloneAbility ca && ca.ChosenToClone != null)
                        {
                            Console.ForegroundColor = abilityColor;
                            Console.Write(unitTypeName + " ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(unit.GetDisplayName(attackingArmy.Name));
                            Console.ForegroundColor = abilityColor;
                            Console.Write(" клонирует ");
                            Console.ForegroundColor = abilityColor;
                            Console.Write($"({ca.ChosenToClone.PowerLevel}) ");
                            Console.ForegroundColor = attackingArmy.Color;
                            Console.Write(ca.ChosenToClone.GetDisplayName(attackingArmy.Name));
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = abilityColor;
                        Console.Write(unitTypeName + " ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(unit.GetDisplayName(attackingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.Write(" стреляет в ");
                        Console.ForegroundColor = defendingArmy.Color;
                        Console.Write(target?.GetDisplayName(defendingArmy.Name));
                        Console.ResetColor();
                        Console.WriteLine();
                    }

                    if (!(realUnit is Wizard))
                    {
                        int damage = healthBefore - (target?.Health ?? 0);
                        Console.WriteLine($"Урон: {damage}");
                        Console.Write($"Здоровье ");
                        Console.ForegroundColor = defendingArmy.Color;
                        Console.Write(target?.GetDisplayName(defendingArmy.Name));
                        Console.ResetColor();
                        Console.WriteLine($": {Math.Max(0, target?.Health ?? 0)}/{target?.MaxHealth ?? 0}");
                    }

                    if (!target?.IsAlive ?? false)
                    {
                        Console.ForegroundColor = abilityColor;
                        Console.Write(unitTypeName + " ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(unit.GetDisplayName(attackingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.Write(" убивает ");
                        Console.ForegroundColor = defendingArmy.Color;
                        Console.Write(target?.GetDisplayName(defendingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.Write(" специальной способностью!");
                        Console.ResetColor();
                        Console.WriteLine();

                        defendingArmy.RemoveDeadFighter(target);
                        needNewRoundHeader = true;
                    }
                }

                if (isHealing)
                {
                    unit.UseSpecialAbility(target);
                    if (unit.SpecialAbility is SpecialAbility sa && sa.LastHealed != null)
                    {
                        Console.ForegroundColor = abilityColor;
                        Console.Write("лекарь ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(unit.GetDisplayName(attackingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.Write(" лечит ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(sa.LastHealed.GetDisplayName(attackingArmy.Name));
                        Console.ForegroundColor = abilityColor;
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.Write($"Здоровье ");
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(sa.LastHealed.GetDisplayName(attackingArmy.Name));
                        Console.ResetColor();
                        Console.WriteLine($": {sa.LastHealed.Health}/{sa.LastHealed.MaxHealth}");
                    }
                }

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Полностью восстанавливает состояние битвы из сохранения (без сброса)
        /// </summary>
        public void RestoreFromSave(FormationType formation, int currentRound, int attackTurn, bool firstAttackerIsArmy1, bool needNewRoundHeader, int moveCount)
        {
            this.round = currentRound;
            this.attackTurn = attackTurn;
            this.firstAttackerIsArmy1 = firstAttackerIsArmy1;
            this.needNewRoundHeader = needNewRoundHeader;
            this.moveCount = moveCount;

            currentFormation = formation;
            SetFormationStrategy(formation);

            // Восстанавливаем специфичные для стратегии данные
            if (_currentStrategy is OneColumnStrategy)
            {
                SetCurrentFightersForContinuation();
            }
            else if (_currentStrategy is ThreeColumnsStrategy)
            {
                InitializeThreeColumns();
            }
            else if (_currentStrategy is WallStrategy)
            {
                _currentStrategy.Reinitialize(this);
            }

            battleInitialized = true;
            _needDisplayPair = true;
        }

        /// <summary>
        /// Проверяет, является ли юнит сильным бойцом (разворачивая декораторы)
        /// </summary>
        private bool IsStrongFighter(IUnit unit)
        {
            var realUnit = UnwrapToStrongFighter(unit);
            return realUnit != null;
        }

        /// <summary>
        /// Разворачивает декораторы до StrongFighter
        /// </summary>
        private IUnit? UnwrapToStrongFighter(IUnit unit)
        {
            while (unit is BuffDecorator decorator)
            {
                unit = decorator.GetInnerUnit();
            }
            return unit is StrongFighter ? unit : null;
        }

        /// <summary>
        /// Проверка, может ли боец надеть баф (рядом слабый боец в порядке боя)
        /// </summary>
        private bool CanEquipBuff(IUnit unit, IArmy army)
        {
            var realUnit = UnwrapToStrongFighter(unit);
            if (realUnit == null) return false;

            int index = army.AliveFightersInBattleOrder.IndexOf(unit);
            if (index == -1) return false;

            // Проверить слева
            if (index > 0 && army.AliveFightersInBattleOrder[index - 1] is WeakFighter wf1 && wf1.IsAlive
                && wf1 != currentFighter1 && wf1 != currentFighter2)
                return true;

            // Проверить справа
            if (index < army.AliveFightersInBattleOrder.Count - 1
                && army.AliveFightersInBattleOrder[index + 1] is WeakFighter wf2 && wf2.IsAlive
                && wf2 != currentFighter1 && wf2 != currentFighter2)
                return true;

            return false;
        }

        // Перегруженный метод EquipBuff для IUnit
        private void EquipBuff(IUnit unit)
        {
            IUnit buffedUnit = BuffFactory.ApplyRandomBuff(unit);
            ReplaceUnitInArmy(unit, buffedUnit);

            Console.WriteLine($"{buffedUnit.GetDisplayName(buffedUnit.Army?.Name ?? "")} надевает бафф!");
            Console.WriteLine($"Атака {buffedUnit.EffectiveAttack}, Защита {buffedUnit.EffectiveDefence}");
        }

        /// <summary>
        /// Очищает сохранённое состояние колонн
        /// </summary>
        public void ClearSavedColumns()
        {
            for (int i = 0; i < 3; i++)
            {
                _savedColumnsArmy1[i] = null;
                _savedColumnsArmy2[i] = null;
            }
            _savedBackupArmy1.Clear();
            _savedBackupArmy2.Clear();
        }

    }
}