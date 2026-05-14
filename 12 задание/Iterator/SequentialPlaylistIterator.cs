namespace PlaylistIterator;

/// <summary>Обход треков в порядке добавления в плейлист.</summary>
public sealed class SequentialPlaylistIterator : IPlaylistIterator
{
    private readonly Playlist _playlist;
    private int _index = -1;

    public SequentialPlaylistIterator(Playlist playlist) => _playlist = playlist;

    public void Reset() => _index = -1;

    public bool MoveNext()
    {
        if (_index + 1 < _playlist.Songs.Count)
        {
            _index++;
            return true;
        }

        return false;
    }

    public Song Current
    {
        get
        {
            if (_index < 0 || _index >= _playlist.Songs.Count)
                throw new InvalidOperationException("Сначала вызовите MoveNext().");
            return _playlist.Songs[_index];
        }
    }
}
