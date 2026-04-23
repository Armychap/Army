using System;
using System.Text;

namespace Strategy
{
    public class SimplePasswordStrategy : IPasswordStrategy
    {
        private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private readonly Random _random = new Random();

        public string Generate(int length)
        {
            if (length < GetMinLength())
                throw new ArgumentException($"Минимальная длина пароля: {GetMinLength()}");

            var password = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                password.Append(Letters[_random.Next(Letters.Length)]);
            }
            return password.ToString();
        }

        public string GetPolicyDescription()
        {
            return "Только буквы (строчные и заглавные)";
        }

        public int GetMinLength()
        {
            return 6;
        }

        public string GetName()
        {
            return "Простой пароль";
        }
    }
}