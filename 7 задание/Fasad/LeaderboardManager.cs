namespace GameFacade;

/// <summary>Подсистема: таблица лидеров.</summary>
public class LeaderboardManager
{
    private readonly List<(int PlayerId, int Score)> _entries = new();

    public void SubmitScore(int playerId, int score)
    {
        _entries.Add((playerId, score));
        _entries.Sort((a, b) => b.Score.CompareTo(a.Score));
    }

    public IReadOnlyList<(int PlayerId, int Score)> GetTop(int count)
    {
        return _entries.Take(count).ToList();
    }
}
