namespace TemplateMethod;

// Небольшой AI для дилера в Blackjack.
public sealed class BlackjackAi
{
    private readonly string _name;
    private readonly int _stopAt;

    public BlackjackAi(string name, int stopAt)
    {
        _name = name;
        _stopAt = stopAt;
    }

    public bool ShouldHit(int currentScore)
    {
        return currentScore < _stopAt;
    }

    public override string ToString() => _name;
}
