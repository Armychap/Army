namespace TemplateMethod;

public sealed class DurakRound : CardGameRound
{
    private readonly DurakAi _defenderAi = new("AI_Bot_2", riskTolerance: 0.6);

    protected override void DealCards()
    {
        Console.WriteLine("2) Раздача: каждому по 6 карт, открыт козырь (пики)");
    }

    protected override void PlaceBets()
    {
        Console.WriteLine("3) Ставки: фиксированный взнос за раунд");
        Bets["Human_Player"] = 20;
        Bets["AI_Bot_1"] = 20;
        Bets["AI_Bot_2"] = 20;
        RecalculatePot();
        Console.WriteLine($"   Банк после ставок: {Pot}");
    }

    protected override void PlayTurn()
    {
        Console.WriteLine("4) Ход: AI_Bot_1 атакует, AI_Bot_2 защищается");
        bool successfulDefense = _defenderAi.ShouldDefend(highAttackPower: 7);
        Console.WriteLine(successfulDefense
            ? "   AI_Bot_2 отбился и перевел ход"
            : "   AI_Bot_2 не отбился и забрал карты");
    }

    protected override void DetermineWinner()
    {
        Winner = "Human_Player";
        Console.WriteLine("5) Победитель: Human_Player (раньше всех остался без карт)");
    }
}
