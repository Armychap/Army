using System;
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
        private IUnit currentFighter1;
        private IUnit currentFighter2;
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
            Console.Write($"{army1.Name} {currentFighter1.FighterNumber}");
            Console.ResetColor();
            Console.Write($" ({currentFighter1.PowerLevel}) vs ");
            Console.ForegroundColor = army2.Color;
            Console.Write($"{army2.Name} {currentFighter2.FighterNumber}");
            Console.ResetColor();
            Console.WriteLine($" ({currentFighter2.PowerLevel})");
            Console.WriteLine(new string('=', 40));
        }

        private void DisplayHealthInfo()
        {
            Console.WriteLine($"  Здоровье {currentFighter1.GetDisplayName(army1.Name)}: {Math.Max(0, currentFighter1.Health)}/{currentFighter1.MaxHealth}");
            Console.WriteLine($"  Здоровье {currentFighter2.GetDisplayName(army2.Name)}: {Math.Max(0, currentFighter2.Health)}/{currentFighter2.MaxHealth}");
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
            Console.Write(defender.GetDisplayName(defendingArmy.Name));
            Console.ResetColor();
            Console.WriteLine();

            int healthBefore = defender.Health;
            attacker.AttackUnit(defender);
            int damage = healthBefore - defender.Health;

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

                defendingArmy.RemoveDeadFighter(defender);
                needNewRoundHeader = true;
                defender = defendingArmy.GetNextFighterInBattleOrder();
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
            Console.WriteLine("     БИТВА ЗАВЕРШЕНА");
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
            Console.WriteLine($"  Выжило бойцов: {army1.AliveCount()}/{army1.Units.Count}");
            
            Console.WriteLine($"\n{army2.Name}:");
            Console.WriteLine($"  Выжило бойцов: {army2.AliveCount()}/{army2.Units.Count}");
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

        // Выполнить один полный раунд (оба удара + проверка специальных способностей)
        public bool DoSingleRound()
        {
            if (!battleInitialized)
            {
                InitializeBattle();
            }
            
            if (!(army1.HasAliveUnits() && army2.HasAliveUnits()))
            {
                return false; // Битва закончена
            }

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
            
            // Проверка специальных способностей после второго удара (когда attackTurn возвращается на 0)
            if (attackTurn == 0 && !needNewRoundHeader && currentFighter1 != null && currentFighter2 != null && currentFighter1.IsAlive && currentFighter2.IsAlive)
            {
                CheckAndExecuteSpecialAbilities();
            }
            
            // Увеличиваем номер раунда только при смене бойца
            if (needNewRoundHeader && (currentFighter1 != null || currentFighter2 != null))
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

        private void CheckAndExecuteSpecialAbilities()
        {
            if (!currentFighter1.IsAlive || !currentFighter2.IsAlive)
                return;

            Console.WriteLine();
            Console.WriteLine("ПРОВЕРКА СПЕЦИАЛЬНЫХ СПОСОБНОСТЕЙ");

            ExecuteSpecialAbilitiesForArmy(army1, army2);
            if (currentFighter1.IsAlive && currentFighter2.IsAlive)
                ExecuteSpecialAbilitiesForArmy(army2, army1);

            Console.WriteLine();
        }
        
        private void ExecuteSpecialAbilitiesForArmy(IArmy attackingArmy, IArmy defendingArmy)
        {
            foreach (var unit in attackingArmy.Units)
            {
                if (!unit.IsAlive)
                    continue;

                if (unit == currentFighter1 || unit == currentFighter2)
                    continue;

                if (unit.SpecialAbility == null)
                    continue;

                IUnit target = attackingArmy == army1 ? currentFighter2 : currentFighter1;
                if (target == null || !target.IsAlive)
                    continue;

                if (unit.CanUseSpecialAbility(target))
                {
                    Console.ForegroundColor = attackingArmy.Color;
                    Console.Write(unit.GetDisplayName(attackingArmy.Name));
                    Console.ResetColor();
                    Console.Write($" использует {unit.SpecialAbility.Name} на ");
                    Console.ForegroundColor = defendingArmy.Color;
                    Console.Write(target.GetDisplayName(defendingArmy.Name));
                    Console.ResetColor();
                    Console.WriteLine();

                    int healthBefore = target.Health;
                    unit.UseSpecialAbility(target);
                    int damage = healthBefore - target.Health;

                    Console.WriteLine($"  Урон: -{damage}");
                    Console.WriteLine($"  Здоровье {target.GetDisplayName(defendingArmy.Name)}: {Math.Max(0, target.Health)}/{target.MaxHealth}");

                    if (!target.IsAlive)
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = attackingArmy.Color;
                        Console.Write(unit.GetDisplayName(attackingArmy.Name));
                        Console.ResetColor();
                        Console.Write($" убивает ");
                        Console.ForegroundColor = defendingArmy.Color;
                        Console.Write(target.GetDisplayName(defendingArmy.Name));
                        Console.ResetColor();
                        Console.WriteLine(" специальной способностью!");

                        defendingArmy.RemoveDeadFighter(target);

                        if (defendingArmy == army1)
                            currentFighter1 = army1.GetNextFighterInBattleOrder();
                        else
                            currentFighter2 = army2.GetNextFighterInBattleOrder();

                        needNewRoundHeader = true;
                        return;
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}