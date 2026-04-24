using System;
using System.Text;

namespace Strategy
{
    public class MemorablePasswordStrategy : IPasswordStrategy
    {
        private readonly string[] _words = { "sun", "moon", "star", "tree", "book", "cat", "dog",
                                              "bird", "fish", "home", "blue", "red", "green", "gold" };
        private readonly Random _random = new Random();

        public string Generate(int length)
        {
            if (length < GetMinLength())
                throw new ArgumentException($"Минимальная длина пароля: {GetMinLength()}");

            var password = new StringBuilder();

            while (password.Length < length - 3)
            {
                if (password.Length > 0)
                    password.Append("-");

                var word = _words[_random.Next(_words.Length)];
                if (password.Length + word.Length <= length - 2)
                {
                    password.Append(word);
                }
                else
                {
                    if (password.Length > 0 && password[password.Length - 1] == '-')
                        password.Length--;
                    break;
                }
            }

            int remainingLength = length - password.Length;

            int digitsToAdd = Math.Max(2, remainingLength);

            if (password.Length + digitsToAdd > length)
            {
                digitsToAdd = length - password.Length;
            }

            for (int i = 0; i < digitsToAdd; i++)
            {
                password.Append(_random.Next(0, 10));
            }

            while (password.Length < length)
            {
                password.Append(_random.Next(0, 10));
            }

            if (password.Length > length)
            {
                password.Length = length;
            }

            return password.ToString();
        }

        public string GetPolicyDescription()
        {
            return "Слова + дефисы + цифры в конце";
        }

        public int GetMinLength()
        {
            return 6;
        }
    }
}