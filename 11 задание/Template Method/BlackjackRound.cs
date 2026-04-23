namespace TemplateMethod;

public sealed class BlackjackRound : CardGameRound
{
    private readonly BlackjackAi _dealerAi = new("AI_Bot_Dealer", stopAt: 18);
    private int _humanScore;
    private int _dealerScore;

    protected override void DealCards()
    {
        _humanScore = 11;
        _dealerScore = 10;
        Console.WriteLine("2) Раздача: Human_Player=11, AI_Bot_Dealer=10");
    }

    protected override void PlaceBets()
    {
        Console.WriteLine("3) Ставки: анте перед раздачей");
        Bets["Human_Player"] = 50;
        Bets["AI_Bot_1"] = 50;
        Bets["AI_Bot_2"] = 50;
        RecalculatePot();
        Console.WriteLine($"   Банк после ставок: {Pot}");
    }

    protected override bool ShouldOfferDoubleDown() => _humanScore is 9 or 10 or 11;

    protected override void PlayTurn()
    {
        Console.WriteLine("4) Ход: Human_Player берет карту (+9), затем дилер решает по AI");
        _humanScore += 9; // 20
        while (_dealerAi.ShouldHit(_dealerScore))
        {
            _dealerScore += 4;
            Console.WriteLine($"   AI_Bot_Dealer берет карту, счет={_dealerScore}");
        }

        Console.WriteLine($"   Итоговые очки: Human_Player={_humanScore}, Dealer={_dealerScore}");
    }

    protected override void DetermineWinner()
    {
        Winner = (_humanScore <= 21 && (_dealerScore > 21 || _humanScore >= _dealerScore))
            ? "Human_Player"
            : "AI_Bot_Dealer";
        Console.WriteLine($"5) Победитель: {Winner}");
    }
}
