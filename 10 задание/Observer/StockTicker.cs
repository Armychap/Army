using Observer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Observer
{
    // Издатель (управляет состоянием акций и списком подписчиков)
    public class StockTicker : IStockSubject
    {
        private readonly List<IStockObserver> _observers = new();
        private readonly Dictionary<string, StockData> _stocks = new();

        public void Attach(IStockObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                Console.WriteLine($"[StockTicker] Подписан новый наблюдатель: {observer.GetType().Name}");
            }
        }

        public void Detach(IStockObserver observer)
        {
            if (_observers.Remove(observer))
            {
                Console.WriteLine($"[StockTicker] Наблюдатель отписан: {observer.GetType().Name}");
            }
        }

        public void Notify(StockUpdateEventArgs e)
        {
            foreach (var observer in _observers)
            {
                observer.Update(e);
            }
        }

        public void UpdateStockPrice(string symbol, decimal newPrice)
        {
            var currentStock = GetOrCreateStock(symbol);
            var oldPrice = currentStock.Price;

            var updatedStock = new StockData(symbol, newPrice, currentStock.Volume);
            _stocks[symbol] = updatedStock;

            var args = new StockUpdateEventArgs(updatedStock, StockUpdateType.PriceChanged, oldPrice, newPrice);

            Console.WriteLine($"\n>>> ИЗМЕНЕНИЕ ЦЕНЫ: {symbol} {oldPrice:F2}$ -> {newPrice:F2}$ ({(args.ChangePercent >= 0 ? "+" : "")}{args.ChangePercent:F2}%)");
            Notify(args);
        }

        public void UpdateStockVolume(string symbol, decimal newVolume)
        {
            var currentStock = GetOrCreateStock(symbol);
            var oldVolume = currentStock.Volume;

            var updatedStock = new StockData(symbol, currentStock.Price, newVolume);
            _stocks[symbol] = updatedStock;

            var args = new StockUpdateEventArgs(updatedStock, StockUpdateType.VolumeChanged, oldVolume, newVolume);

            var multiplier = oldVolume != 0 ? newVolume / oldVolume : 0;
            Console.WriteLine($"\n>>> ИЗМЕНЕНИЕ ОБЪЁМА: {symbol} {oldVolume:F0} -> {newVolume:F0} (x{multiplier:F2})");
            Notify(args);
        }

        private StockData GetOrCreateStock(string symbol)
        {
            if (!_stocks.TryGetValue(symbol, out var stock))
            {
                stock = new StockData(symbol, 0, 0);
                _stocks[symbol] = stock;
            }
            return stock;
        }

        public StockData? GetStock(string symbol) =>
            _stocks.TryGetValue(symbol, out var stock) ? stock : null;

        public IReadOnlyList<string> GetTrackedSymbols() => _stocks.Keys.ToList();
    }
}