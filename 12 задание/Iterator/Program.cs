using System.Text;

namespace PlaylistIterator;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var playlist = new Playlist();
        playlist.Add(new Song("Звёздная ночь", "Луна", isFavorite: true));
        playlist.Add(new Song("Дождь за окном", "Город", isFavorite: false));
        playlist.Add(new Song("Утро", "Кофе", isFavorite: true));
        playlist.Add(new Song("Ночной поезд", "Рельсы", isFavorite: false));
        playlist.Add(new Song("Тихая комната", "Соло", isFavorite: true));

        Console.WriteLine("Плейлист (все треки в порядке добавления):");
        for (var i = 0; i < playlist.Songs.Count; i++)
            Console.WriteLine($"  {i + 1}. {playlist.Songs[i]}");

        Console.WriteLine("\n--- Режим 1: последовательное воспроизведение (Sequential Iterator) ---");
        PlayAll(playlist.CreateSequentialIterator());

        Console.WriteLine("\n--- Режим 2: случайный порядок (Shuffle Iterator, фиксированный seed для демо) ---");
        var shuffle = playlist.CreateShuffleIterator(new Random(2026));
        PlayAll(shuffle);

        Console.WriteLine("\n--- Режим 3: только избранное (Favorites Iterator) ---");
        PlayAll(playlist.CreateFavoritesIterator());

        Console.WriteLine("\nГотово. Нажмите любую клавишу для выхода.");
        Console.ReadKey();
    }

    static void PlayAll(IPlaylistIterator iterator)
    {
        iterator.Reset();
        var n = 0;
        while (iterator.MoveNext())
        {
            n++;
            Console.WriteLine($"  ▶ {n}. {iterator.Current}");
        }

        if (n == 0)
            Console.WriteLine("  (нет треков для воспроизведения)");
    }
}
