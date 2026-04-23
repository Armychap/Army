namespace TemplateMethod;

// Реализация Template Method находится здесь: метод PlayRound().
public abstract class CardGameRound
{
    protected readonly List<string> Players = new() { "Human_Player", "AI_Bot_1", "AI_Bot_2" };
    protected readonly Dictionary<string, int> Bets = new();
    protected string Winner = string.Empty;
    protected int Pot;

    // Template Method: фиксирует общий алгоритм раунда.
    public void PlayRound()
    {
        AnnounceStart();
        ShuffleDeck();
        DealCards();          // abstract
        PlaceBets();          // abstract

        if (ShouldOfferDoubleDown())
            OfferDoubleDown(); // hook

        PlayTurn();           // abstract
        DetermineWinner();    // abstract
        PayoutWinnings();
    }

    protected virtual void AnnounceStart() =>
        Console.WriteLine($"[{GetType().Name}] Старт раунда");

    // Общий шаг, одинаковый для всех карточных игр.
    protected virtual void ShuffleDeck() =>
        Console.WriteLine("1) Колода перетасована");

    protected abstract void DealCards();
    protected abstract void PlaceBets();
    protected abstract void PlayTurn();
    protected abstract void DetermineWinner();

    // Хук: по умолчанию не нужен; переопределяется в Blackjack.
    protected virtual bool ShouldOfferDoubleDown() => false;

    protected virtual void OfferDoubleDown()
    {
        Console.WriteLine("3.5) Опция Double Down доступна: Human_Player удваивает ставку");
        Bets["Human_Player"] *= 2;
        RecalculatePot();
    }

    // Общий шаг, одинаковый для всех карточных игр.
    protected virtual void PayoutWinnings() =>
        Console.WriteLine($"6) Выплата: {Winner} забирает банк {Pot} фишек");

    protected void RecalculatePot() => Pot = Bets.Values.Sum();
}
