using System.Linq;

namespace Strategy
{
    public class PasswordStrengthChecker
    {
        public int CalculateStrength(string password)
        {
            int score = 0;

            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Any(char.IsLower)) score++;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(c => !char.IsLetterOrDigit(c))) score++;

            var uniqueChars = password.Distinct().Count();
            if (uniqueChars >= password.Length * 0.7) score++;

            return score;
        }

        public string GetStrengthLevel(int score)
        {
            return score switch
            {
                <= 2 => "Слабый",
                <= 4 => "Средний",
                <= 6 => "Сильный",
                _ => "Очень сильный"
            };
        }
    }
}