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
            int targetLength = length;

            while (password.Length < targetLength - 2)
            {
                if (password.Length > 0)
                    password.Append("-");

                var word = _words[_random.Next(_words.Length)];
                if (password.Length + word.Length <= targetLength - 2)
                {
                    password.Append(word);
                }
                else
                {
                    break;
                }
            }

            password.Append(_random.Next(10, 99));

            return password.ToString();
        }

        public string GetPolicyDescription()
        {
            return "Слова + дефисы + две цифры в конце";
        }

        public int GetMinLength()
        {
            return 6;
        }

        public string GetName()
        {
            return "Запоминаемый пароль";
        }
    }
}