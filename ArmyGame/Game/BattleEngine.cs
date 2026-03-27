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
        
        public BattleEngine(IArmy army1, IArmy army2, int speed = 400)
        {
            this.army1 = army1;
            this.army2 = army2;
            round = 1;
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
        public int AttackTurn => attackTurn;
        public bool FirstAttackerIsArmy1 => firstAttackerIsArmy1;
        public bool NeedNewRoundHeader => needNewRoundHeader;

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

        private void DisplayHealthInfo()
        {
            Console.WriteLine($"Здоровье {currentFighter1?.FighterNumber}: {Math.Max(0, currentFighter1?.Health ?? 0)}/{currentFighter1?.MaxHealth ?? 0}");
            Console.WriteLine($"Здоровье {currentFighter2?.FighterNumber}: {Math.Max(0, currentFighter2?.Health ?? 0)}/{currentFighter2?.MaxHealth ?? 0}");
            Console.WriteLine();
        }

        // Выполняет один удар от attacker к defender, выводит сообщения и обновляет очередь.
        private void PerformAttack(IArmy attackingArmy, IArmy defendingArmy, ref IUnit attacker, ref IUnit defender)
        {
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
                needNewRoundHeader = true;
                defender = defendingArmy?.GetNextFighterInBattleOrder();
            }
            else
            {
                attackTurn = 1 - attackTurn;
                needNewRoundHeader = false;
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
            
            // Ничьей быть не может - всегда должен быть победитель
            if (army1Wins)
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
            
            Console.WriteLine($"Всего раундов: {round - 1}");
            
            // Статистика по армиям
            Console.WriteLine($"\n{army1.Name}:");
            Console.WriteLine($"Выжило бойцов: {army1.AliveCount()}/{army1.Units.Count}");
            
            Console.WriteLine($"\n{army2.Name}:");
            Console.WriteLine($"Выжило бойцов: {army2.AliveCount()}/{army2.Units.Count}");
        }

        // Инициализация без вывода текста
        public void InitializeBattle()
        {
            if (battleInitialized)
                return;

            army1.ShuffleAliveFighters();
            army2.ShuffleAliveFighters();

            currentFighter1 = army1.GetNextFighterInBattleOrder();
            currentFighter2 = army2.GetNextFighterInBattleOrder();
            
            round = 1;
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
            
            // Запоминаем текущий номер раунда
            int startRound = round;
            
            // Выполняем ходы пока раунд не изменится (или битва не закончится)
            while (round == startRound)
            {
                if (!DoSingleMove())
                    return false;
            }
            
            return true;
        }

        // Выполнить один ход (один удар) в битве
        public bool DoSingleMove()
        {
            if (!battleInitialized)
            {
                InitializeBattle();
            }
            
            if (!(army1.HasAliveUnits() && army2.HasAliveUnits() && 
                   currentFighter1 != null && currentFighter2 != null))
            {
                return false; // Битва закончена
            }
            
            // Показываем заголовок раунда только если нужен новый раунд
            if (needNewRoundHeader)
            {
                DisplayRoundHeader();
                
                // В начале каждого раунда случайно определяем, кто бьет первым
                firstAttackerIsArmy1 = random.Next(2) == 0;
                attackTurn = 0;
            }
            
            bool currentAttackerIsArmy1 = attackTurn == 0 ? firstAttackerIsArmy1 : !firstAttackerIsArmy1;

            if (currentAttackerIsArmy1)
                PerformAttack(army1, army2, ref currentFighter1, ref currentFighter2);
            else
                PerformAttack(army2, army1, ref currentFighter2, ref currentFighter1);
            
            // Проверка специальных способностей после каждого удара, если оба бойца живы
            if (currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
            {
                CheckAndExecuteSpecialAbilities();
            }
            
            // Если после специальных способностей нужно начать новый раунд, увеличиваем номер раунда
            if (needNewRoundHeader)
            {
                round++;
            }
            
            return true; // Ход выполнен успешно
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

                if (unitType != null && unit.GetType() != unitType)
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
                    // Лекарь лечит случайного союзника, кроме себя и StrongFighter
                    var possibleTargets = attackingArmy.Units.Where(u => u.IsAlive && u != unit && !u.Is<StrongFighter>()).ToList();
                    if (possibleTargets.Count == 0) continue;
                    target = possibleTargets[random.Next(possibleTargets.Count)];
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

                    // Выполняем способность после вывода сообщения (для лучников и лекарей)
                    if (!isCloning)
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
    }
}