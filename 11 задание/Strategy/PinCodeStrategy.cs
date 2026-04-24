using System;
using System.Text;

namespace Strategy
{
    public class PinCodeStrategy : IPasswordStrategy
    {
        private const string Digits = "0123456789";
        private readonly Random _random = new Random();

        public string Generate(int length)
        {
            if (length < GetMinLength())
                throw new ArgumentException($"Минимальная длина PIN-кода: {GetMinLength()}");

            var password = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                password.Append(Digits[_random.Next(Digits.Length)]);
            }
            return password.ToString();
        }

        public string GetPolicyDescription()
        {
            return "Только цифры";
        }

        public int GetMinLength()
        {
            return 4;
        }
    }
}