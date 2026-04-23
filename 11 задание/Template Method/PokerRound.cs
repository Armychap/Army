namespace TemplateMethod;

public sealed class PokerRound : CardGameRound
{
    protected override void DealCards()
    {
        Console.WriteLine("2) Раздача: каждому по 2 закрытые карты");
    }

    protected override void PlaceBets()
    {
        Console.WriteLine("3) Ставки: блайнды и первый круг");
        Bets["Human_Player"] = 30;
        Bets["AI_Bot_1"] = 40;
        Bets["AI_Bot_2"] = 20;
        RecalculatePot();
        Console.WriteLine($"   Банк после ставок: {Pot}");
    }

    protected override void PlayTurn()
    {
        Console.WriteLine("4) Игра: флоп, терн, ривер");
        Console.WriteLine("   AI_Bot_1 агрессивно повышает ставку на терне");
        Bets["AI_Bot_1"] += 20;
        RecalculatePot();
        Console.WriteLine($"   Банк после повышения: {Pot}");
    }

    protected override void DetermineWinner()
    {
        Winner = "AI_Bot_1";
        Console.WriteLine("5) Победитель: AI_Bot_1 (комбинация: стрит)");
    }
}
