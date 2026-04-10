namespace GameFacade;

/// <summary>Представление таблицы лидеров в текст (без знания о подсистемах).</summary>
public static class LeaderboardPresenter
{
    public static void Write(TextWriter output, string title, IReadOnlyList<(int PlayerId, int Score)> rows)
    {
        output.WriteLine("{0} (топ-10)", title);
        if (rows.Count == 0)
        {
            output.WriteLine("  (пока нет записей)");
            return;
        }
        int place = 1;
        foreach (var (playerId, score) in rows)
            output.WriteLine("  {0}. Игрок #{1}: {2} очков", place++, playerId, score);
    }
}
