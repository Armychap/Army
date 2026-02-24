using System;
using System.Threading;
using ArmyBattle.Models;
using ArmyBattle.Game;

namespace ArmyBattle
{
    // Главный класс программы
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Битва Армий";
            
            bool exit = false;
            Army redArmy = null;
            Army blueArmy = null;
            
            while (!exit)
            {
                ClearConsole();
                Console.WriteLine("          ГЛАВНОЕ МЕНЮ");
                Console.WriteLine("1 - Создать армии и начать битву");
                Console.WriteLine("2 - Создать армии вручную (пока нет)");
                Console.WriteLine("3 - Загрузить армии с диска (пока нет)");
                Console.WriteLine("4 - Сохранить армии на диск (пока нет)");
                Console.WriteLine("5 - Показать информацию об армиях (пока нет)");
                Console.WriteLine("0 - Выход");
                Console.Write("Выбор: ");
                
                string choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        CreateRandomBattle();
                        redArmy = null;
                        blueArmy = null;
                        break;
                        
                    case "2":
                        ClearConsole();
                        Console.WriteLine("пока нет");
                        Console.ReadKey();
                        break;
                        
                    case "3":
                        ClearConsole();
                        Console.WriteLine("пока нет");
                        Console.ReadKey();
                        break;
                        
                    case "4":
                        ClearConsole();
                        Console.WriteLine("пока нет");
                        Console.ReadKey();
                        break;
                        
                    case "5":
                        ShowArmiesInfo(redArmy, blueArmy);
                        break;
                        
                    case "0":
                        exit = true;
                        break;
                        
                    default:
                        Console.WriteLine("Неверный выбор!");
                        Console.ReadKey();
                        break;
                }
            }
        }
        
        static void ClearConsole()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                // Игнорируем ошибку, если консоль недоступна
            }
        }
        
        static void CreateRandomBattle()
        {
            ClearConsole();
            Console.Write("Введите бюджет для обеих армий (стандарт 200): ");
            if (!int.TryParse(Console.ReadLine(), out int budget))
            {
                budget = 200;
            }
            
            Random random = new Random();
            
            // Создание армий с префиксами
            Army redArmy = new Army("Красная Армия", "Красный", ConsoleColor.Red);
            Army blueArmy = new Army("Синяя Армия", "Синий", ConsoleColor.Blue);
            
            // Генерация армий с одинаковым бюджетом
            redArmy.GenerateArmyWithBudget(budget);
            blueArmy.GenerateArmyWithBudget(budget);
            
            // Вывод информации об армиях перед битвой
            Console.WriteLine(new string('=', 40));
            Console.WriteLine($"Бюджет каждой армии: {budget}");
            Console.WriteLine();
            
            redArmy.DisplayArmyInfo(true);
            Console.WriteLine();
            blueArmy.DisplayArmyInfo(true);
            
            Console.WriteLine("Нажмите Enter для начала битвы...");
            Console.ReadLine();
            
            // Создание и запуск движка битвы
            BattleEngine battle = new BattleEngine(redArmy, blueArmy, 400);
            battle.StartBattle();
            
            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }
        
        
        static void ShowArmiesInfo(Army army1, Army army2)
        {
            ClearConsole();
            
            if (army1 == null || army2 == null)
            {
                Console.WriteLine("Армии не созданы!");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine(new string('=', 50));
            army1.DisplayArmyInfo(true);
            Console.WriteLine();
            army2.DisplayArmyInfo(true);
            Console.WriteLine(new string('=', 50));
            
            Console.ReadKey();
        }
    }
}