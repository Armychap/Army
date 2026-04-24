using System;
using System.Linq;
using System.Text;

namespace Strategy
{
    public class StrongPasswordStrategy : IPasswordStrategy
    {
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Digits = "0123456789";
        private const string Symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        private readonly Random _random = new Random();

        public string Generate(int length)
        {
            if (length < GetMinLength())
                throw new ArgumentException($"Минимальная длина пароля: {GetMinLength()}");

            var allChars = Lowercase + Uppercase + Digits + Symbols;
            var password = new StringBuilder();

            password.Append(Lowercase[_random.Next(Lowercase.Length)]);
            password.Append(Uppercase[_random.Next(Uppercase.Length)]);
            password.Append(Digits[_random.Next(Digits.Length)]);
            password.Append(Symbols[_random.Next(Symbols.Length)]);

            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[_random.Next(allChars.Length)]);
            }

            return new string(password.ToString().OrderBy(x => _random.Next()).ToArray());
        }

        public string GetPolicyDescription()
        {
            return "Буквы (строчные и заглавные) + цифры + специальные символы";
        }

        public int GetMinLength()
        {
            return 8;
        }
    }
}