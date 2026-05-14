namespace PlaylistIterator;

/// <summary>Обход в случайном порядке (Shuffle). Порядок фиксируется при Reset.</summary>
public sealed class ShufflePlaylistIterator : IPlaylistIterator
{
    private readonly Playlist _playlist;
    private readonly Random _random;
    private int[] _order = Array.Empty<int>();
    private int _position = -1;

    public ShufflePlaylistIterator(Playlist playlist, Random random)
    {
        _playlist = playlist;
        _random = random;
    }

    public void Reset()
    {
        _position = -1;
        var n = _playlist.Songs.Count;
        _order = new int[n];
        for (var i = 0; i < n; i++)
            _order[i] = i;

        // Fisher–Yates
        for (var i = n - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (_order[i], _order[j]) = (_order[j], _order[i]);
        }
    }

    public bool MoveNext()
    {
        if (_order.Length == 0)
            return false;

        if (_position + 1 < _order.Length)
        {
            _position++;
            return true;
        }

        return false;
    }

    public Song Current
    {
        get
        {
            if (_position < 0 || _position >= _order.Length)
                throw new InvalidOperationException("Сначала вызовите MoveNext() после Reset().");
            return _playlist.Songs[_order[_position]];
        }
    }
}
