namespace TemplateMethod;

public static class ProgramBeforeTemplateMethod
{
    public static void Run()
    {
        RunPokerRoundWithoutTemplate();
        Console.WriteLine(new string('-', 70));
        RunBlackjackRoundWithoutTemplate();
    }

    // До паттерна: шаги раунда прописаны вручную и повторяются в каждой игре.
    private static void RunPokerRoundWithoutTemplate()
    {
        Console.WriteLine("[Poker: до] Начинается раунд");
        Console.WriteLine("1) Перетасовали колоду");
        Console.WriteLine("2) Раздали по 2 карты");
        Console.WriteLine("3) Игроки сделали ставки");
        Console.WriteLine("4) Ход: флоп, терн, ривер");
        Console.WriteLine("5) Победил AI_Bot_1 по старшей комбинации");
        Console.WriteLine("6) Выплатили выигрыш из банка\n");
    }

    private static void RunBlackjackRoundWithoutTemplate()
    {
        Console.WriteLine("[Blackjack: до] Начинается раунд");
        Console.WriteLine("1) Перетасовали колоду");
        Console.WriteLine("2) Раздали по 2 карты");
        Console.WriteLine("3) Игроки сделали ставки");
        Console.WriteLine("4) Игрок взял карту, AI остановился на 18");
        Console.WriteLine("5) Победил Human_Player: 20 очков против 18");
        Console.WriteLine("6) Выплатили выигрыш из банка");
    }
}
