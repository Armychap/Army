namespace GameFacade;

/// <summary>Программа «после»: клиент опирается на фасад игровой платформы.</summary>
public static class ProgramAfterFacade
{
    public static void Run(TextWriter output)
    {
        var log = new DemoLog(output);

        var players = new PlayerManager();
        var levels = new LevelManager();
        var achievements = new AchievementManager();
        var leaderboard = new LeaderboardManager();
        var store = new InGamePurchaseService();

        var platform = new GamePlatformFacade(players, levels, achievements, leaderboard, store);

        log.Line("=== С фасадом: высокоуровневые StartNewGame / EndGame / данные лидерборда ===");

        platform.StartNewGame("Алексей");
        log.Line("1) StartNewGame — сессия, уровень 1, монеты.");
        log.LineFormatted(
            "   Сессия: {0}, игрок id={1}, уровень={2}, монеты={3}",
            players.IsSessionActive,
            players.CurrentPlayerId,
            levels.CurrentLevel,
            store.GetCoins(players.CurrentPlayerId!.Value));

        log.Line();
        log.Line("2) Имитация раундов через фасад.");
        if (!platform.PlayDemoRounds(4))
        {
            log.Line("   Нет активной сессии — пропуск остальных шагов.");
            return;
        }

        log.LineFormatted("   Очки забега: {0}, уровень: {1}", levels.RunScore, levels.CurrentLevel);

        int pid = players.CurrentPlayerId!.Value;
        bool bought = store.TryPurchase(pid, "skin_basic", 15);
        log.Line();
        log.Line("3) Покупка напрямую через магазин (как отдельная подсистема).");
        log.LineFormatted(
            "   Покупка skin_basic за 15 монет: {0}, монеты: {1}",
            bought ? "успех" : "недостаточно монет",
            store.GetCoins(pid));

        log.Line();
        log.Line("4) EndGame — фасад обновляет лидерборд, достижения, монеты, закрывает сессию.");
        platform.EndGame();
        log.LineFormatted("   Сессия активна: {0}", players.IsSessionActive);
        log.LineFormatted("   Достижения игрока #{0}: {1}", pid, string.Join(", ", achievements.GetUnlocked(pid)));

        log.Line();
        log.Line("5) Лидерборд: данные через фасад, формат — у Presenter.");
        LeaderboardPresenter.Write(output, "Таблица лидеров", platform.GetLeaderboardTop(10));
    }
}
