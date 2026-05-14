namespace PlaylistIterator;

/// <summary>Коллекция песен (Aggregate). Создаёт разные итераторы без раскрытия внутренней структуры обхода.</summary>
public sealed class Playlist
{
    private readonly List<Song> _songs = new();

    public IReadOnlyList<Song> Songs => _songs;

    public void Add(Song song) => _songs.Add(song);

    public IPlaylistIterator CreateSequentialIterator() => new SequentialPlaylistIterator(this);

    public IPlaylistIterator CreateShuffleIterator(Random? random = null) =>
        new ShufflePlaylistIterator(this, random ?? new Random());

    public IPlaylistIterator CreateFavoritesIterator() => new FavoritesPlaylistIterator(this);
}
