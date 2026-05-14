namespace PlaylistIterator;

/// <summary>Обход только избранных треков, в порядке их появления в плейлисте.</summary>
public sealed class FavoritesPlaylistIterator : IPlaylistIterator
{
    private readonly Playlist _playlist;
    private readonly List<int> _favoriteIndices = new();
    private int _cursor = -1;

    public FavoritesPlaylistIterator(Playlist playlist)
    {
        _playlist = playlist;
        RebuildIndices();
    }

    private void RebuildIndices()
    {
        _favoriteIndices.Clear();
        for (var i = 0; i < _playlist.Songs.Count; i++)
        {
            if (_playlist.Songs[i].IsFavorite)
                _favoriteIndices.Add(i);
        }
    }

    public void Reset()
    {
        RebuildIndices();
        _cursor = -1;
    }

    public bool MoveNext()
    {
        if (_cursor + 1 < _favoriteIndices.Count)
        {
            _cursor++;
            return true;
        }

        return false;
    }

    public Song Current
    {
        get
        {
            if (_cursor < 0 || _cursor >= _favoriteIndices.Count)
                throw new InvalidOperationException("Сначала вызовите MoveNext().");
            return _playlist.Songs[_favoriteIndices[_cursor]];
        }
    }
}
