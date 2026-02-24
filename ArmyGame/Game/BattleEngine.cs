using System;
using System.Threading;
using ArmyBattle.Models;

namespace ArmyBattle.Game
{
    // Движок битвы (перенесён сюда из Models/Game)
    public class BattleEngine
    {
        private Army army1;
        private Army army2;
        private int round;
        private Random random;
        private int battleSpeed;
        private Unit currentFighter1;
        private Unit currentFighter2;
        private bool needNewRoundHeader;
        private bool firstAttackerIsArmy1;
        private int attackTurn; // 0 = первый атакующий, 1 = второй атакующий

        private bool battleInitialized = false;
        
        public BattleEngine(Army army1, Army army2, int speed = 400)
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

        // Запуск битвы
        public void StartBattle()
        {
            try
            {
                Console.Clear();
            }
            catch { }
            Console.WriteLine("           НАЧАЛО БИТВЫ");
            Console.WriteLine($"{army1.Name} против {army2.Name}");
            Console.WriteLine($"Бюджет каждой команды: {army1.TotalCost}");
            Console.WriteLine();
            Thread.Sleep(1000);

            // Перемешиваем бойцов в случайном порядке
            army1.ShuffleAliveFighters();
            army2.ShuffleAliveFighters();

            // Получаем первых бойцов в случайном порядке
            currentFighter1 = army1.GetNextFighterInBattleOrder();
            currentFighter2 = army2.GetNextFighterInBattleOrder();

            while (army1.HasAliveUnits() && army2.HasAliveUnits() && 
                   currentFighter1 != null && currentFighter2 != null)
            {
                // Показываем заголовок раунда только если нужен новый раунд
                if (needNewRoundHeader)
                {
                    DisplayRoundHeader();
                    
                    // В начале каждого раунда случайно определяем, кто бьет первым
                    firstAttackerIsArmy1 = random.Next(2) == 0;
                    attackTurn = 0;
                }
                
                // Определяем, кто атакует в этот раз
                bool currentAttackerIsArmy1;
                if (attackTurn == 0)
                {
                    // Первый удар в раунде
                    currentAttackerIsArmy1 = firstAttackerIsArmy1;
                }
                else
                {
                    // Чередуем удары
                    currentAttackerIsArmy1 = !firstAttackerIsArmy1;
                }
                
                if (currentAttackerIsArmy1)
                {
                    // Атакует первый боец
                    Console.ForegroundColor = army1.Color;
                    Console.Write(currentFighter1.GetDisplayName(army1.Prefix));
                    Console.ResetColor();
                    Console.Write($" бьет ");
                    Console.ForegroundColor = army2.Color;
                    Console.Write(currentFighter2.GetDisplayName(army2.Prefix));
                    Console.ResetColor();
                    Console.WriteLine();
                    
                    int healthBefore = currentFighter2.Health;
                    currentFighter1.AttackUnit(currentFighter2);
                    int damage = healthBefore - currentFighter2.Health;
                    
                    DisplayHealthInfo();
                    
                    // Проверяем, не убил ли первый боец второго
                    if (!currentFighter2.IsAlive)
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = army1.Color;
                        Console.Write(currentFighter1.GetDisplayName(army1.Prefix));
                        Console.ResetColor();
                        Console.Write($" убивает ");
                        Console.ForegroundColor = army2.Color;
                        Console.Write(currentFighter2.GetDisplayName(army2.Prefix));
                        Console.ResetColor();
                        Console.WriteLine();
                        
                        // Удаляем убитого бойца из списка живых
                        army2.RemoveDeadFighter(currentFighter2);
                        
                        // Второй боец мертв, нужен новый раунд
                        needNewRoundHeader = true;
                        
                        // Получаем следующего бойца из второй армии
                        currentFighter2 = army2.GetNextFighterInBattleOrder();
                    }
                    else
                    {
                        // Переключаем очередь атаки
                        attackTurn = 1 - attackTurn;
                        needNewRoundHeader = false;
                    }
                }
                else
                {
                    // Атакует второй боец
                    Console.ForegroundColor = army2.Color;
                    Console.Write(currentFighter2.GetDisplayName(army2.Prefix));
                    Console.ResetColor();
                    Console.Write($" бьет ");
                    Console.ForegroundColor = army1.Color;
                    Console.Write(currentFighter1.GetDisplayName(army1.Prefix));
                    Console.ResetColor();
                    Console.WriteLine();
                    
                    int healthBefore = currentFighter1.Health;
                    currentFighter2.AttackUnit(currentFighter1);
                    int damage = healthBefore - currentFighter1.Health;
                    
                    DisplayHealthInfo();
                    
                    // Проверяем, не убил ли второй боец первого
                    if (!currentFighter1.IsAlive)
                    {
                        Console.WriteLine();
                        Console.ForegroundColor = army2.Color;
                        Console.Write(currentFighter2.GetDisplayName(army2.Prefix));
                        Console.ResetColor();
                        Console.Write($" убивает ");
                        Console.ForegroundColor = army1.Color;
                        Console.Write(currentFighter1.GetDisplayName(army1.Prefix));
                        Console.ResetColor();
                        Console.WriteLine();
                        
                        // Удаляем убитого бойца из списка живых
                        army1.RemoveDeadFighter(currentFighter1);
                        
                        // Первый боец мертв, нужен новый раунд
                        needNewRoundHeader = true;
                        
                        // Получаем следующего бойца из первой армии
                        currentFighter1 = army1.GetNextFighterInBattleOrder();
                    }
                    else
                    {
                        // Переключаем очередь атаки
                        attackTurn = 1 - attackTurn;
                        needNewRoundHeader = false;
                    }
                }
                
                // Пауза перед продолжением
                Thread.Sleep(battleSpeed);
                
                // Увеличиваем номер раунда только при смене бойца
                if (needNewRoundHeader && (currentFighter1 != null || currentFighter2 != null))
                {
                    round++;
                }
                
                // Делаем небольшую паузу между действиями
                Thread.Sleep(200);
            }
            
            EndBattle();
        }

        // Отображение заголовка раунда
        private void DisplayRoundHeader()
        {
            Console.WriteLine();
            Console.Write($"РАУНД {round}: ");
            Console.ForegroundColor = army1.Color;
            Console.Write(currentFighter1.GetDisplayName(army1.Prefix));
            Console.ResetColor();
            Console.Write($" ({currentFighter1.PowerLevel}) vs ");
            Console.ForegroundColor = army2.Color;
            Console.Write(currentFighter2.GetDisplayName(army2.Prefix));
            Console.ResetColor();
            Console.WriteLine($" ({currentFighter2.PowerLevel})");
            Console.WriteLine(new string('=', 40));
        }

        // Отображение здоровья бойцов
        private void DisplayHealthInfo()
        {
            Console.WriteLine($"  Здоровье {currentFighter1.GetDisplayName(army1.Prefix)}: {Math.Max(0, currentFighter1.Health)}/{currentFighter1.MaxHealth}");
            Console.WriteLine($"  Здоровье {currentFighter2.GetDisplayName(army2.Prefix)}: {Math.Max(0, currentFighter2.Health)}/{currentFighter2.MaxHealth}");
            Console.WriteLine(); // Пустая строка для разделения
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
            Console.WriteLine(new string('-', 40));
            
            Console.WriteLine($"Всего раундов: {round - 1}");
            
            // Статистика по армиям
            Console.WriteLine($"\n{army1.Name}:");
            Console.WriteLine($"  Выжило бойцов: {army1.AliveCount()}/{army1.Units.Count}");
            
            Console.WriteLine($"\n{army2.Name}:");
            Console.WriteLine($"  Выжило бойцов: {army2.AliveCount()}/{army2.Units.Count}");
        }

        // Инициализация битвы (без стартовых сообщений)
        public void InitializeBattle()
        {
            if (battleInitialized)
                return;
            
            // Перемешиваем бойцов в случайном порядке
            army1.ShuffleAliveFighters();
            army2.ShuffleAliveFighters();

            // Получаем первых бойцов в случайном порядке
            currentFighter1 = army1.GetNextFighterInBattleOrder();
            currentFighter2 = army2.GetNextFighterInBattleOrder();
            
            round = 1;
            needNewRoundHeader = true;
            attackTurn = 0;
            battleInitialized = true;
        }

        // Выполнить один ход в битве
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
            
            // Определяем, кто атакует в этот раз
            bool currentAttackerIsArmy1;
            if (attackTurn == 0)
            {
                // Первый удар в раунде
                currentAttackerIsArmy1 = firstAttackerIsArmy1;
            }
            else
            {
                // Чередуем удары
                currentAttackerIsArmy1 = !firstAttackerIsArmy1;
            }
            
            if (currentAttackerIsArmy1)
            {
                // Атакует первый боец
                Console.ForegroundColor = army1.Color;
                Console.Write(currentFighter1.GetDisplayName(army1.Prefix));
                Console.ResetColor();
                Console.Write($" бьет ");
                Console.ForegroundColor = army2.Color;
                Console.Write(currentFighter2.GetDisplayName(army2.Prefix));
                Console.ResetColor();
                Console.WriteLine();
                
                int healthBefore = currentFighter2.Health;
                currentFighter1.AttackUnit(currentFighter2);
                int damage = healthBefore - currentFighter2.Health;
                
                DisplayHealthInfo();
                
                // Проверяем, не убил ли первый боец второго
                if (!currentFighter2.IsAlive)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = army1.Color;
                    Console.Write(currentFighter1.GetDisplayName(army1.Prefix));
                    Console.ResetColor();
                    Console.Write($" убивает ");
                    Console.ForegroundColor = army2.Color;
                    Console.Write(currentFighter2.GetDisplayName(army2.Prefix));
                    Console.ResetColor();
                    Console.WriteLine();
                    
                    // Удаляем убитого бойца из списка живых
                    army2.RemoveDeadFighter(currentFighter2);
                    
                    // Второй боец мертв, нужен новый раунд
                    needNewRoundHeader = true;
                    
                    // Получаем следующего бойца из второй армии
                    currentFighter2 = army2.GetNextFighterInBattleOrder();
                }
                else
                {
                    // Переключаем очередь атаки
                    attackTurn = 1 - attackTurn;
                    needNewRoundHeader = false;
                }
            }
            else
            {
                // Атакует второй боец
                Console.ForegroundColor = army2.Color;
                Console.Write(currentFighter2.GetDisplayName(army2.Prefix));
                Console.ResetColor();
                Console.Write($" бьет ");
                Console.ForegroundColor = army1.Color;
                Console.Write(currentFighter1.GetDisplayName(army1.Prefix));
                Console.ResetColor();
                Console.WriteLine();
                
                int healthBefore = currentFighter1.Health;
                currentFighter2.AttackUnit(currentFighter1);
                int damage = healthBefore - currentFighter1.Health;
                
                DisplayHealthInfo();
                
                // Проверяем, не убил ли второй боец первого
                if (!currentFighter1.IsAlive)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = army2.Color;
                    Console.Write(currentFighter2.GetDisplayName(army2.Prefix));
                    Console.ResetColor();
                    Console.Write($" убивает ");
                    Console.ForegroundColor = army1.Color;
                    Console.Write(currentFighter1.GetDisplayName(army1.Prefix));
                    Console.ResetColor();
                    Console.WriteLine();
                    
                    // Удаляем убитого бойца из списка живых
                    army1.RemoveDeadFighter(currentFighter1);
                    
                    // Первый боец мертв, нужен новый раунд
                    needNewRoundHeader = true;
                    
                    // Получаем следующего бойца из первой армии
                    currentFighter1 = army1.GetNextFighterInBattleOrder();
                }
                else
                {
                    // Переключаем очередь атаки
                    attackTurn = 1 - attackTurn;
                    needNewRoundHeader = false;
                }
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
        }    }
}