namespace GameFacade;

/// <summary>Подсистема: достижения игрока.</summary>
public class AchievementManager
{
    private readonly Dictionary<int, HashSet<string>> _unlockedByPlayer = new();

    public IReadOnlyCollection<string> GetUnlocked(int playerId) =>
        _unlockedByPlayer.TryGetValue(playerId, out var set)
            ? set.ToList()
            : Array.Empty<string>();

    public void UnlockIfNotYet(int playerId, string achievementId)
    {
        if (!_unlockedByPlayer.TryGetValue(playerId, out var set))
        {
            set = new HashSet<string>();
            _unlockedByPlayer[playerId] = set;
        }
        set.Add(achievementId);
    }

    public void EvaluateAfterRun(int playerId, int totalScore, int levelReached)
    {
        if (totalScore >= 100)
            UnlockIfNotYet(playerId, "first_100_points");
        if (levelReached >= 2)
            UnlockIfNotYet(playerId, "reached_level_2");
    }
}
