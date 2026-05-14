namespace PlaylistIterator;

/// <summary>Итератор по коллекции песен (паттерн Iterator).</summary>
public interface IPlaylistIterator
{
    /// <summary>Сбросить обход к началу (для повторного прохода).</summary>
    void Reset();

    /// <summary>Перейти к следующему треку. false — больше нет элементов.</summary>
    bool MoveNext();

    /// <summary>Текущий трек после успешного MoveNext.</summary>
    Song Current { get; }
}
