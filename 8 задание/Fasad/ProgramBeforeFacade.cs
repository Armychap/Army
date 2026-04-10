namespace GameFacade;

/// <summary>Программа «до»: клиент сам вызывает все подсистемы в нужном порядке.</summary>
public static class ProgramBeforeFacade
{
    public static void Run(TextWriter output)
    {
        var log = new DemoLog(output);

        var players = new PlayerManager();
        var levels = new LevelManager();
        var achievements = new AchievementManager();
        var leaderboard = new LeaderboardManager();
        var store = new InGamePurchaseService();

        log.Line("Без фасада");

        int pid = players.RegisterOrGetPlayerId("Мария");
        players.BeginSession(pid);
        levels.ResetRun();
        levels.LoadLevel(1);
        store.AddCoins(pid, 25);

        levels.SimulateLevelProgress();
        levels.SimulateLevelProgress();

        log.Line("1) Сессия, уровень, стартовые монеты, имитация раундов — отдельные вызовы.");
        log.LineFormatted("   Очки забега: {0}, уровень: {1}", levels.RunScore, levels.CurrentLevel);

        bool bought = store.TryPurchase(pid, "skin_basic", 15);
        log.LineFormatted(
            "2) Покупка: {0}, монеты: {1}",
            bought ? "успех" : "недостаточно монет",
            store.GetCoins(pid));

        leaderboard.SubmitScore(pid, levels.RunScore);
        achievements.EvaluateAfterRun(pid, levels.RunScore, levels.CurrentLevel);
        store.AddCoins(pid, levels.RunScore / 10);
        players.EndSession();

        log.Line("3) Завершение: LeaderboardManager, AchievementManager, магазин, EndSession — по отдельности.");
        log.LineFormatted("   Достижения игрока #{0}: {1}", pid, string.Join(", ", achievements.GetUnlocked(pid)));

        LeaderboardPresenter.Write(output, "Таблица лидеров", leaderboard.GetTop(10));
    }
}
