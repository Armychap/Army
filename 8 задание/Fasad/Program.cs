using GameFacade;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Игровая платформа: паттерн Facade ===\n");

// Подсистемы создаются явно (как в «настоящем» приложении их инициализирует композиция корня).
var players = new PlayerManager();
var levels = new LevelManager();
var achievements = new AchievementManager();
var leaderboard = new LeaderboardManager();
var store = new InGamePurchaseService();

var platform = new GamePlatformFacade(players, levels, achievements, leaderboard, store);

// --- Клиентский код знает только о фасаде: три высокоуровневых шага ---
Console.WriteLine("1) StartNewGame — фасад сам выставляет сессию, уровень 1, стартовые монеты.");
platform.StartNewGame("Алексей");
Console.WriteLine($"   Сессия активна: {players.IsSessionActive}, игрок id={players.CurrentPlayerId}, уровень={levels.CurrentLevel}, монеты={store.GetCoins(players.CurrentPlayerId!.Value)}");

Console.WriteLine("\n2) Имитация игры (подсистема уровней вызывается только через фасад в демо).");
platform.PlayDemoRounds(4);
Console.WriteLine($"   После раундов: очки забега={levels.RunScore}, текущий уровень={levels.CurrentLevel}");

Console.WriteLine("\n3) Покупка через подсистему магазина (в коде клиента ниже — для сравнения с фасадом).");
int pid = players.CurrentPlayerId!.Value;
bool bought = store.TryPurchase(pid, "skin_basic", 15);
Console.WriteLine($"   Покупка skin_basic за 15 монет: {(bought ? "успех" : "недостаточно монет")}, монеты осталось: {store.GetCoins(pid)}");

Console.WriteLine("\n4) EndGame — фасад отправляет счёт в лидерборд, проверяет достижения, начисляет монеты за забег, закрывает сессию.");
platform.EndGame();
Console.WriteLine($"   Сессия активна: {players.IsSessionActive}");
Console.WriteLine($"   Достижения игрока #{pid}: {string.Join(", ", achievements.GetUnlocked(pid))}");

Console.WriteLine("\n5) ShowLeaderboard — один вызов вместо обращения к LeaderboardManager напрямую.");
platform.ShowLeaderboard();

Console.WriteLine("\n--- Сравнение: «вручную» без фасада для второго игрока ---");
var pid2 = players.RegisterOrGetPlayerId("Мария");
players.BeginSession(pid2);
levels.ResetRun();
levels.LoadLevel(1);
store.AddCoins(pid2, 25);
levels.SimulateLevelProgress();
levels.SimulateLevelProgress();
leaderboard.SubmitScore(pid2, levels.RunScore);
achievements.EvaluateAfterRun(pid2, levels.RunScore, levels.CurrentLevel);
store.AddCoins(pid2, levels.RunScore / 10);
players.EndSession();
Console.WriteLine($"Тот же результат по шагам требует вызовов к PlayerManager, LevelManager, LeaderboardManager, AchievementManager, InGamePurchaseService.");
platform.ShowLeaderboard();

Console.WriteLine("\nГотово.");
