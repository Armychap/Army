namespace GameFacade;

/// <summary>Подсистема: учёт игроков и текущей сессии.</summary>
public class PlayerManager
{
    private readonly Dictionary<string, int> _playerIds = new();
    private int _nextId = 1;
    private int? _currentPlayerId;
    private bool _sessionActive;

    public int? CurrentPlayerId => _currentPlayerId;
    public bool IsSessionActive => _sessionActive;

    public int RegisterOrGetPlayerId(string displayName)
    {
        if (!_playerIds.TryGetValue(displayName, out int id))
        {
            id = _nextId++;
            _playerIds[displayName] = id;
        }
        return id;
    }

    public void BeginSession(int playerId)
    {
        _currentPlayerId = playerId;
        _sessionActive = true;
    }

    public void EndSession()
    {
        _sessionActive = false;
        _currentPlayerId = null;
    }
}
