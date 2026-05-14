namespace PlaylistIterator;

/// <summary>Трек в плейлисте.</summary>
public sealed class Song
{
    public string Title { get; }
    public string Artist { get; }
    public bool IsFavorite { get; set; }

    public Song(string title, string artist, bool isFavorite = false)
    {
        Title = title;
        Artist = artist;
        IsFavorite = isFavorite;
    }

    public override string ToString() =>
        $"{Title} — {Artist}" + (IsFavorite ? " ★" : "");
}
