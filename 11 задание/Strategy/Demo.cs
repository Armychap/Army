using System;

namespace Strategy
{
    public class Demo
    {
        private readonly PasswordGenerator _generator;
        private readonly PasswordStrengthChecker _checker;
        private bool _running;

        public Demo()
        {
            _generator = new PasswordGenerator(new SimplePasswordStrategy());
            _checker = new PasswordStrengthChecker();
            _running = true;
        }

        public void Run()
        {
            while (_running)
            {
                ShowMenu();
                ProcessChoice();
            }
        }

        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("   ГЕНЕРАТОР ПАРОЛЕЙ   ");
            Console.WriteLine();
            Console.WriteLine($"Текущая стратегия: {_generator.GetCurrentPolicyDescription()}");
            Console.WriteLine();
            Console.WriteLine("1. Простой пароль (только буквы)");
            Console.WriteLine("2. Сложный пароль (буквы + цифры + символы)");
            Console.WriteLine("3. Запоминаемый пароль (слова + цифры)");
            Console.WriteLine("4. PIN-код (только цифры)");
            Console.WriteLine("5. Демонстрация надёжности паролей");
            Console.WriteLine("0. Выход");
            Console.WriteLine();
            Console.Write("\nВыберите пункт меню: ");
        }

        private void ProcessChoice()
        {
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    UseSimpleStrategy();
                    break;
                case "2":
                    UseStrongStrategy();
                    break;
                case "3":
                    UseMemorableStrategy();
                    break;
                case "4":
                    UsePinCodeStrategy();
                    break;
                case "5":
                    ShowStrengthDemo();
                    break;
                case "0":
                    _running = false;
                    break;
                default:
                    Console.WriteLine("Неверный выбор.");
                    Console.ReadKey();
                    break;
            }
        }

        private void UseStrategy(IPasswordStrategy strategy, int defaultLength)
        {
            _generator.SetStrategy(strategy);

            Console.Clear();

            Console.WriteLine("  Информация о стратегии  ");
            Console.WriteLine();
            Console.WriteLine($"Описание: {_generator.GetCurrentPolicyDescription()}");

            int minLength = _generator.GetMinLength();
            Console.WriteLine($"Минимальная длина: {minLength} символов");

            Console.Write($"\nВведите длину пароля (мин. {strategy.GetMinLength()}): ");
            if (!int.TryParse(Console.ReadLine(), out int length) || length < strategy.GetMinLength())
            {
                length = defaultLength;
                Console.WriteLine($"Будет использована длина по умолчанию: {length}");
            }

            var password = _generator.GeneratePassword(length, out int score, out string strength);

            Console.WriteLine("\n  Результат генерации  ");
            Console.WriteLine($"Пароль:     {password}");
            Console.WriteLine($"Длина:      {password.Length} символов");
            Console.WriteLine($"Надёжность: {strength}");
            Console.WriteLine($"Оценка:     {score}");

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }

        private void UseSimpleStrategy()
        {
            UseStrategy(new SimplePasswordStrategy(), 10);
        }

        private void UseStrongStrategy()
        {
            UseStrategy(new StrongPasswordStrategy(), 12);
        }

        private void UseMemorableStrategy()
        {
            UseStrategy(new MemorablePasswordStrategy(), 15);
        }

        private void UsePinCodeStrategy()
        {
            UseStrategy(new PinCodeStrategy(), 4);
        }

        private void ShowStrengthDemo()
        {
            Console.Clear();
            Console.WriteLine("Проверка надёжности паролей\n");

            var testPasswords = new[]
            {
                "abc",
                "password123",
                "P@ssw0rd!",
                "K#9mP$2vL&7xQ@1nR"
            };

            Console.WriteLine("Пароль                    | Надёжность     | Оценка");
            Console.WriteLine(new string('─', 60));

            foreach (var pwd in testPasswords)
            {
                var score = _checker.CalculateStrength(pwd);
                var level = _checker.GetStrengthLevel(score);
                Console.WriteLine($"{pwd,-25} | {level,-14} | {score}");
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }
    }
}