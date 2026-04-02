namespace GameFacade;

/// <summary>
/// Фасад: одна точка входа для клиента. Скрывает порядок вызовов подсистем
/// (игроки, уровни, достижения, лидерборд, магазин).
/// </summary>
public class GamePlatformFacade
{
    private readonly PlayerManager _players;
    private readonly LevelManager _levels;
    private readonly AchievementManager _achievements;
    private readonly LeaderboardManager _leaderboard;
    private readonly InGamePurchaseService _store;
    private string? _currentPlayerName;

    public GamePlatformFacade(
        PlayerManager players,
        LevelManager levels,
        AchievementManager achievements,
        LeaderboardManager leaderboard,
        InGamePurchaseService store)
    {
        _players = players;
        _levels = levels;
        _achievements = achievements;
        _leaderboard = leaderboard;
        _store = store;
    }

    /// <summary>Новая игра: регистрация/вход, сессия, старт уровня, начисление стартовых монет.</summary>
    public void StartNewGame(string playerDisplayName)
    {
        _currentPlayerName = playerDisplayName;
        int pid = _players.RegisterOrGetPlayerId(playerDisplayName);
        _players.BeginSession(pid);
        _levels.ResetRun();
        _levels.LoadLevel(1);
        _store.AddCoins(pid, 25);
    }

    /// <summary>Конец забега: очки в лидерборд, проверка достижений, награда монетами, закрытие сессии.</summary>
    public void EndGame()
    {
        if (_players.CurrentPlayerId is not int pid || !_players.IsSessionActive)
            return;

        int score = _levels.RunScore;
        int level = _levels.CurrentLevel;
        _leaderboard.SubmitScore(pid, score);
        _achievements.EvaluateAfterRun(pid, score, level);
        _store.AddCoins(pid, score / 10);
        _players.EndSession();
    }

    public void ShowLeaderboard()
    {
        var top = _leaderboard.GetTop(10);
        Console.WriteLine("--- Таблица лидеров (топ-10) ---");
        int place = 1;
        foreach (var (playerId, score) in top)
        {
            Console.WriteLine($"  {place++}. Игрок #{playerId}: {score} очков");
        }
        if (top.Count == 0)
            Console.WriteLine("  (пока нет записей)");
    }

    /// <summary>Для демо: имитация прохождения уровней без отдельного UI.</summary>
    public void PlayDemoRounds(int rounds)
    {
        if (_players.CurrentPlayerId is null || !_players.IsSessionActive)
        {
            Console.WriteLine("Сначала вызовите StartNewGame.");
            return;
        }
        for (int i = 0; i < rounds; i++)
        {
            _levels.SimulateLevelProgress();
            if (i % 2 == 1)
                _levels.LoadLevel(_levels.CurrentLevel + 1);
        }
    }
}
