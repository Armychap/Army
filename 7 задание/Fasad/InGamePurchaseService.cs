namespace GameFacade;

/// <summary>Подсистема: внутриигровые покупки (каталог и баланс монет).</summary>
public class InGamePurchaseService
{
    private readonly Dictionary<int, int> _coinsByPlayer = new();

    public int GetCoins(int playerId) =>
        _coinsByPlayer.TryGetValue(playerId, out int c) ? c : 0;

    public void AddCoins(int playerId, int amount)
    {
        _coinsByPlayer[playerId] = GetCoins(playerId) + amount;
    }

    /// <summary>Покупка по идентификатору товара из каталога.</summary>
    public bool TryPurchase(int playerId, string itemId, int priceCoins)
    {
        if (GetCoins(playerId) < priceCoins)
            return false;
        _coinsByPlayer[playerId] = GetCoins(playerId) - priceCoins;
        return true;
    }
}
