using System;
using System.Linq;
using System.Threading;
using ArmyBattle.Models;

namespace ArmyBattle.Game
{
    /// <summary>
    /// Логика проведения боя между двумя армиями.
    ///  занимается только пошаговым выполнением поединка
    ///принимает IArmy, IUnit, не зависит от конкретных реализаций
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
        private const int maxNoHealthChangeActions = 10;
        private FormationType currentFormation = FormationType.OneColumn;

        private IUnit?[] currentFightersArmy1 = new IUnit?[3];
        private IUnit?[] currentFightersArmy2 = new IUnit?[3];

        // Единые очереди резерва для обеих армий
        private List<IUnit> army1BackupQueue = new();
        private List<IUnit> army2BackupQueue = new();

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
                Console.WriteLine(new string('=', 40));
            }
            else
            {
                Console.WriteLine($"\nРАУНД {round} (ТРИ КОЛОННЫ)");
                Console.WriteLine(new string('═', 40));
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

        public void DisplayBattleOrder()
        {
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
                Console.WriteLine("Порядок боя");
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
                Console.WriteLine($"Резерв {army1.Name}: {string.Join("→", army1BackupQueue.Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})"))}");
                Console.WriteLine($"Резерв {army2.Name}: {string.Join("←", army2BackupQueue.Select(u => $"{u.FighterNumber}({u.PowerLevel.Substring(0, 3)})"))}");
                Console.WriteLine();
            }
        }

        private void DisplayHealthInfo()
        {
            Console.WriteLine($"Здоровье {currentFighter1?.FighterNumber}: {Math.Max(0, currentFighter1?.Health ?? 0)}/{currentFighter1?.MaxHealth ?? 0}");
            Console.WriteLine($"Здоровье {currentFighter2?.FighterNumber}: {Math.Max(0, currentFighter2?.Health ?? 0)}/{currentFighter2?.MaxHealth ?? 0}");
            if (currentFighter1 is StrongFighter sf1 && sf1.Buffs.Count > 0)
            {
                Console.WriteLine($"Бафы {sf1.FighterNumber}: {string.Join(", ", sf1.Buffs.Select(b => b.Name))}");
            }
            if (currentFighter2 is StrongFighter sf2 && sf2.Buffs.Count > 0)
            {
                Console.WriteLine($"Бафы {sf2.FighterNumber}: {string.Join(", ", sf2.Buffs.Select(b => b.Name))}");
            }
            Console.WriteLine();
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

                // Раунд продолжается, так как никто не умер
                needNewRoundHeader = false;

                // Пропускаем атаку, продолжаем с другим боецом
                attackTurn = 1 - attackTurn;
                return;
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
                defender = defendingArmy.GetNextFighterInBattleOrder();

                noLethalActions = 0;
                noHealthChangeCount = 0;

                // Конец текущего раунда (переход на новый после смерти бойца)
                needNewRoundHeader = true;
            }
            else
            {
                // Раунд продолжается
                needNewRoundHeader = false;
                noLethalActions++;
                if (noLethalActions >= maxNoLethalActions)
                {
                    stalemateReached = true;
                    Console.WriteLine("Патовая ситуация: слишком много ходов без смертей. Битва объявлена ничьей.");
                }
            }

            attackTurn = 1 - attackTurn;
        }

        // Проверка, может ли сильный боец надеть баф (рядом слабый боец в порядке боя)
        private bool CanEquipBuff(StrongFighter sf, IArmy army)
        {
            int index = army.AliveFightersInBattleOrder.IndexOf(sf);
            if (index == -1) return false;
            // Проверить слева
            if (index > 0 && army.AliveFightersInBattleOrder[index - 1] is WeakFighter wf1 && wf1.IsAlive && wf1 != currentFighter1 && wf1 != currentFighter2) return true;
            // Проверить справа
            if (index < army.AliveFightersInBattleOrder.Count - 1 && army.AliveFightersInBattleOrder[index + 1] is WeakFighter wf2 && wf2.IsAlive && wf2 != currentFighter1 && wf2 != currentFighter2) return true;
            return false;
        }

        // Надеть баф на сильного бойца
        private void EquipBuff(StrongFighter sf)
        {
            // Рандомный выбор бафа
            int choice = random.Next(1, 5); // 1-4

            Buff? buff = choice switch
            {
                1 => Buffs.Horse,
                2 => Buffs.Shield,
                3 => Buffs.Helmet,
                4 => Buffs.Spear,
                _ => null
            };

            if (buff != null)
            {
                sf.Buffs.Add(buff);
                Console.WriteLine($"{sf.GetDisplayName(sf.Army?.Name ?? "")} надевает баф: {buff.Name} (+{buff.AttackBonus} атаки, +{buff.DefenceBonus} защиты)");
                Console.WriteLine($"Атака {sf.EffectiveAttack}, Защита {sf.EffectiveDefence}, Здоровье {sf.Health}/{sf.MaxHealth}");
            }
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

            DisplayBattleStats();
        }

        // Вывод статистики битвы
        private void DisplayBattleStats()
        {
            Console.WriteLine("\nСТАТИСТИКА БИТВЫ:");

            Console.WriteLine($"Всего раундов: {round}");
            Console.WriteLine($"Всего ходов: {moveCount}");

            // Статистика по армиям
            Console.WriteLine($"\n{army1.Name}:");
            Console.WriteLine($"Выжило бойцов: {army1.AliveCount()}/{army1.Units.Count}");

            Console.WriteLine($"\n{army2.Name}:");
            Console.WriteLine($"Выжило бойцов: {army2.AliveCount()}/{army2.Units.Count}");
        }

        // Инициализация без вывода текста
        public void InitializeBattle(FormationType formation = FormationType.OneColumn)
        {
            if (battleInitialized)
                return;

            currentFormation = formation;

            if (formation == FormationType.ThreeColumns)
            {
                InitializeThreeColumnBattle();
            }
            else
            {
                // Оригинальная логика для одной колонны
                army1.ShuffleAliveFighters();
                army2.ShuffleAliveFighters();
                currentFighter1 = army1.GetNextFighterInBattleOrder();
                currentFighter2 = army2.GetNextFighterInBattleOrder();
            }

            round = 0;
            moveCount = 0;
            noHealthChangeCount = 0;
            needNewRoundHeader = true;
            attackTurn = 0;
            battleInitialized = true;
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
            }

            if (stalemateReached)
            {
                Console.WriteLine("Битва прекращена: патовая ситуация.");
                return false;
            }

            // Проверка окончания битвы для обоих режимов
            if (currentFormation == FormationType.OneColumn)
            {
                if (!(army1.HasAliveUnits() && army2.HasAliveUnits())) return false;
            }
            else
            {
                if (!HasActiveColumnPair()) return false;
            }

            if (needNewRoundHeader)
            {
                round++;
                DisplayRoundHeader();
                firstAttackerIsArmy1 = random.Next(2) == 0;
                attackTurn = 0;
            }

            moveCount++;
            Console.WriteLine($"Ход {moveCount}");

            // === ЛОГИКА БАФФОВ (из оригинального кода) ===
            var army1StrongFighters = army1.Units
                .Where(u => u is StrongFighter sf && sf.IsAlive && sf.Army != null && sf != currentFighter1 && sf != currentFighter2 && CanEquipBuff(sf, u.Army))
                .Cast<StrongFighter>()
                .ToList();
            if (army1StrongFighters.Any())
            {
                var chosen = army1StrongFighters[random.Next(army1StrongFighters.Count)];
                EquipBuff(chosen);
            }

            var army2StrongFighters = army2.Units
                .Where(u => u is StrongFighter sf && sf.IsAlive && sf.Army != null && sf != currentFighter1 && sf != currentFighter2 && CanEquipBuff(sf, u.Army))
                .Cast<StrongFighter>()
                .ToList();
            if (army2StrongFighters.Any())
            {
                var chosen = army2StrongFighters[random.Next(army2StrongFighters.Count)];
                EquipBuff(chosen);
            }

            // === ПРОВЕРКА ИЗМЕНЕНИЯ ЗДОРОВЬЯ (оригинальная логика) ===
            allUnitsHealthBefore.Clear();
            foreach (var unit in army1.Units.Concat(army2.Units).Where(u => u.IsAlive))
            {
                allUnitsHealthBefore[unit] = unit.Health;
            }

            bool anyAction = false;

            if (currentFormation == FormationType.OneColumn)
            {
                // === ОРИГИНАЛЬНАЯ ЛОГИКА 1-на-1 ===
                bool currentAttackerIsArmy1 = attackTurn == 0 ? firstAttackerIsArmy1 : !firstAttackerIsArmy1;
                if (currentAttackerIsArmy1)
                    PerformAttack(army1, army2, ref currentFighter1, ref currentFighter2);
                else
                    PerformAttack(army2, army1, ref currentFighter2, ref currentFighter1);
                anyAction = true;
            }
            else
            {
                // === НОВАЯ ЛОГИКА 3-на-3 ===
                anyAction = ProcessThreeColumnMove();
            }

            // Проверка специальных способностей
            if (currentFighter1?.IsAlive == true && currentFighter2?.IsAlive == true)
                CheckAndExecuteSpecialAbilities();
            else if (currentFormation == FormationType.ThreeColumns)
                CheckSpecialAbilitiesThreeColumns();

            // === ПРОВЕРКА СТАГНАЦИИ (оригинальная логика) ===
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

        private bool HasActiveColumnPair()
        {
            for (int col = 0; col < 3; col++)
            {
                if (currentFightersArmy1[col]?.IsAlive == true && currentFightersArmy2[col]?.IsAlive == true)
                    return true;
            }
            return false;
        }

        private bool ProcessThreeColumnMove()
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
                // noLethalActions проверка на стагнацию уже есть в основном цикле
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

            // Упрощённый вывод здоровья
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

                // Заменяем погибшего из общего резерва
                bool isArmy1Dead = defendingArmy == army1;
                ReplaceDeadFighterInColumn(column, defendingArmy == army1);

                // Обновляем ссылку для продолжения цикла
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
                    // Временная установка для совместимости с существующей логикой
                    var oldF1 = currentFighter1; var oldF2 = currentFighter2;
                    currentFighter1 = f1; currentFighter2 = f2;

                    CheckAndExecuteSpecialAbilities();

                    currentFighter1 = oldF1; currentFighter2 = oldF2;
                }
            }
        }

        /// <summary>
        /// Вывод здоровья для одного бойца (упрощённая версия для колонн)
        /// </summary>
        private void DisplayHealthInfoSingle(IUnit unit)
        {
            Console.WriteLine($"Здоровье {unit.FighterNumber}: {Math.Max(0, unit.Health)}/{unit.MaxHealth}");
            if (unit is StrongFighter sf && sf.Buffs.Count > 0)
            {
                Console.WriteLine($"Бафы {sf.FighterNumber}: {string.Join(", ", sf.Buffs.Select(b => b.Name))}");
            }
        }

        /// <summary>
        /// Применение баффов перед ходом (адаптировано для трёх колонн)
        /// </summary>
        private void ApplyBuffsBeforeMove()
        {
            var strongFighters = army1.Units.Concat(army2.Units)
                .Where(u => u is StrongFighter sf && sf.IsAlive && CanEquipBuffForThreeColumns(sf))
                .Cast<StrongFighter>().ToList();

            foreach (var sf in strongFighters.Take(2))
            { // Макс 2 баффа за ход
                EquipBuff(sf);
            }
        }

        private bool CanEquipBuffForThreeColumns(StrongFighter sf)
        {
            // Упрощённая проверка: можно надеть бафф, если боец не в активном бою
            for (int col = 0; col < 3; col++)
            {
                if (currentFightersArmy1[col] == sf || currentFightersArmy2[col] == sf)
                    return false;
            }
            return true;
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
            // Создаём копию списка, чтобы избежать ошибки при изменении коллекции во время итерации
            var unitsCopy = attackingArmy.Units.ToList();
            foreach (var unit in unitsCopy)
            {
                if (!unit.IsAlive)
                    continue;

                if (unitType != null && unit.GetRootType() != unitType)
                    continue;

                // Бойцы, которые участвовали в раунде, не могут использовать специальные способности
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
                    // Лучник может стрелять в любого противника в пределах дальности
                    int range = random.Next(1, defendingArmy.AliveCount() + 1);
                    var possibleTargets = defendingArmy.AliveFightersInBattleOrder.Where((u, index) => index < range && u.IsAlive).ToList();
                    if (possibleTargets.Count == 0) continue;
                    target = possibleTargets[random.Next(possibleTargets.Count)];
                }
                else if (isCloning)
                {
                    // Маг клонирует случайного союзника (кроме лекаря, тяжелого бойца, себя и другого мага)
                    var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && u != unit && !u.Is<Healer>() && !u.Is<StrongFighter>() && !u.Is<Wizard>()).ToList();
                    if (possibleTargets.Count == 0) continue;
                    target = possibleTargets[random.Next(possibleTargets.Count)];
                }
                else if (isHealing)
                {
                    // Лекарь лечит случайного союзника, кроме StrongFighter.
                    var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && !u.Is<StrongFighter>()).ToList();
                    if (possibleTargets.Count == 0) continue;

                    // Попробуем выбрать другого союзника, но если никого нет, может лечить себя.
                    var filtered = possibleTargets.Where(u => u != unit).ToList();
                    if (filtered.Count > 0)
                    {
                        target = filtered[random.Next(filtered.Count)];
                    }
                    else
                    {
                        target = unit; // себе
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

                    // Для мага сначала выполняем способность, чтобы узнать, кого он клонирует
                    if (isCloning)
                    {
                        unit.UseSpecialAbility(target);
                    }
                    // Выполняем способность для лучников и лекарей ДО вывода сообщение о урона
                    else if (realUnit is Archer)
                    {
                        unit.UseSpecialAbility(target);
                    }

                    ConsoleColor abilityColor = GetAbilityColor(realUnit.GetType());

                    if (isHealing)
                    {
                        // Вывод лечения будет после выполнения способности
                    }
                    else
                    {
                        string unitTypeName = realUnit is Archer ? "лучник" : "маг";

                        if (realUnit is Wizard)
                        {
                            // Для мага показываем, кого он клонирует
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
                                // Если нет доступных кандидатов для клонирования, не выводим строку вообще.
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
                            // Убираем return, чтобы продолжить выполнение других способностей
                        }
                    }

                    // Выполняем способность для лекарей (после вывода сообщения о лучнике)
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
        /// Бойцы распределяются по колоннам циклически: 0→кол1, 1→кол2, 2→кол3, 3→кол1...
        /// </summary>
        private void InitializeThreeColumnBattle()
        {
            currentFormation = FormationType.ThreeColumns;
            army1BackupQueue.Clear(); army2BackupQueue.Clear();
            for (int i = 0; i < 3; i++) { currentFightersArmy1[i] = null; currentFightersArmy2[i] = null; }

            var alive1 = army1.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();
            var alive2 = army2.AliveFightersInBattleOrder.Where(u => u.IsAlive).ToList();

            for (int i = 0; i < Math.Min(3, alive1.Count); i++) currentFightersArmy1[i] = alive1[i];
            for (int i = 0; i < Math.Min(3, alive2.Count); i++) currentFightersArmy2[i] = alive2[i];

            army1BackupQueue.AddRange(alive1.Skip(3));
            army2BackupQueue.AddRange(alive2.Skip(3));
        }

        /// <summary>
        /// Пересоздаёт боевой порядок при смене построения во время боя.
        /// Сохраняет текущих бойцов и их здоровье, перераспределяет очереди.
        /// </summary>
        public void ReinitializeFormation(FormationType newFormation)
        {
            if (currentFormation == newFormation) return;

            // Сохраняем текущих активных бойцов
            var activeFighters1 = new List<IUnit>();
            var activeFighters2 = new List<IUnit>();

            if (currentFormation == FormationType.OneColumn)
            {
                if (currentFighter1?.IsAlive == true) activeFighters1.Add(currentFighter1);
                if (currentFighter2?.IsAlive == true) activeFighters2.Add(currentFighter2);
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (currentFightersArmy1[i]?.IsAlive == true) activeFighters1.Add(currentFightersArmy1[i]);
                    if (currentFightersArmy2[i]?.IsAlive == true) activeFighters2.Add(currentFightersArmy2[i]);
                }
            }

            // Очищаем структуры трёх колонн
            army1BackupQueue.Clear(); army2BackupQueue.Clear();
            for (int i = 0; i < 3; i++) { currentFightersArmy1[i] = null; currentFightersArmy2[i] = null; }

            currentFormation = newFormation;

            if (newFormation == FormationType.ThreeColumns)
            {
                var allAlive1 = army1.Units.Where(u => u.IsAlive).ToList();
                var allAlive2 = army2.Units.Where(u => u.IsAlive).ToList();

                for (int i = 0; i < Math.Min(3, allAlive1.Count); i++) currentFightersArmy1[i] = allAlive1[i];
                for (int i = 0; i < Math.Min(3, allAlive2.Count); i++) currentFightersArmy2[i] = allAlive2[i];

                army1BackupQueue.AddRange(allAlive1.Skip(3));
                army2BackupQueue.AddRange(allAlive2.Skip(3));
            }
            else // Возврат к одной колонне
            {
                army1.AliveFightersInBattleOrder.Clear();
                foreach (var f in currentFightersArmy1) if (f?.IsAlive == true) army1.AliveFightersInBattleOrder.Add(f);
                army1.AliveFightersInBattleOrder.AddRange(army1BackupQueue.Where(u => u.IsAlive));
                army1BackupQueue.Clear();

                army2.AliveFightersInBattleOrder.Clear();
                foreach (var f in currentFightersArmy2) if (f?.IsAlive == true) army2.AliveFightersInBattleOrder.Add(f);
                army2.AliveFightersInBattleOrder.AddRange(army2BackupQueue.Where(u => u.IsAlive));
                army2BackupQueue.Clear();

                // ✅ Исправлено: используем те же переменные activeFighters1/2, что объявлены выше
                currentFighter1 = activeFighters1.FirstOrDefault() ?? army1.AliveFightersInBattleOrder.FirstOrDefault();
                currentFighter2 = activeFighters2.FirstOrDefault() ?? army2.AliveFightersInBattleOrder.FirstOrDefault();

                army1.CurrentFighterIndex = 0;
                army2.CurrentFighterIndex = 0;
            }

            needNewRoundHeader = true;
        }


    }
}