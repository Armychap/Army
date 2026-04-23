using System;

namespace Strategy
{
    public class PasswordGenerator
    {
        private IPasswordStrategy _strategy;
        private readonly PasswordStrengthChecker _strengthChecker;

        public PasswordGenerator(IPasswordStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            _strengthChecker = new PasswordStrengthChecker();
        }

        public void SetStrategy(IPasswordStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        public string GeneratePassword(int length, out int score, out string strength)
        {
            var password = _strategy.Generate(length);
            score = _strengthChecker.CalculateStrength(password);
            strength = _strengthChecker.GetStrengthLevel(score);

            return password;
        }

        public void ShowCurrentPolicy()
        {
            Console.WriteLine($"Тип стратегии: {_strategy.GetName()}");
            Console.WriteLine($"Описание: {_strategy.GetPolicyDescription()}");
            Console.WriteLine($"Минимальная длина: {_strategy.GetMinLength()} символов");
        }
    }
}