using Observer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Observer
{
    // Данные о биржевом инструменте
    public class StockData
    {
        public string Symbol { get; }
        public decimal Price { get; }
        public decimal Volume { get; }
        public DateTime Timestamp { get; }

        public StockData(string symbol, decimal price, decimal volume)
        {
            Symbol = symbol;
            Price = price;
            Volume = volume;
            Timestamp = DateTime.Now;
        }
    }

    // Тип обновления биржевых данных
    public enum StockUpdateType
    {
        PriceChanged,
        VolumeChanged
    }

    // Аргументы события обновления биржевых данных
    public class StockUpdateEventArgs
    {
        public StockData StockData { get; }
        public StockUpdateType UpdateType { get; }
        public decimal OldValue { get; }
        public decimal NewValue { get; }

        public StockUpdateEventArgs(StockData stockData, StockUpdateType updateType, decimal oldValue, decimal newValue)
        {
            StockData = stockData;
            UpdateType = updateType;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public decimal ChangePercent => OldValue != 0 ? (NewValue - OldValue) / OldValue * 100 : 0;
        public bool IsIncrease => NewValue > OldValue;
        public bool IsDecrease => NewValue < OldValue;
    }


    // Интерфейс наблюдателя (подписчика)
    public interface IStockObserver
    {
        void Update(StockUpdateEventArgs e);
    }

    // Интерфейс субъекта (издателя)
    public interface IStockSubject
    {
        void Attach(IStockObserver observer);
        void Detach(IStockObserver observer);
        void Notify(StockUpdateEventArgs e);
    }


    // Наблюдатель с возможностью фильтрации по условиям
    public abstract class FilteredStockObserver : IStockObserver
    {
        private readonly decimal? _priceThreshold;
        private readonly bool _notifyAboveThreshold;
        private readonly StockUpdateType? _filterType;

        protected FilteredStockObserver(
            decimal? priceThreshold = null,
            bool notifyAboveThreshold = true,
            StockUpdateType? filterType = null)
        {
            _priceThreshold = priceThreshold;
            _notifyAboveThreshold = notifyAboveThreshold;
            _filterType = filterType;
        }

        public void Update(StockUpdateEventArgs e)
        {
            if (ShouldNotify(e))
            {
                OnUpdate(e);
            }
        }

        private bool ShouldNotify(StockUpdateEventArgs e)
        {
            // Фильтр по типу обновления
            if (_filterType.HasValue && e.UpdateType != _filterType.Value)
                return false;

            // Фильтр по порогу цены
            if (_priceThreshold.HasValue && e.UpdateType == StockUpdateType.PriceChanged)
            {
                return _notifyAboveThreshold
                    ? e.NewValue > _priceThreshold.Value
                    : e.NewValue < _priceThreshold.Value;
            }

            return true;
        }

        protected abstract void OnUpdate(StockUpdateEventArgs e);
    }
}