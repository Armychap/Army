using Observer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Observer
{
    // Трейдер (получает уведомления о любых изменениях)
    public class Trader : IStockObserver
    {
        private readonly string _name;
        private readonly bool _verbose;

        public Trader(string name, bool verbose = true)
        {
            _name = name;
            _verbose = verbose;
        }

        public void Update(StockUpdateEventArgs e)
        {
            if (!_verbose) return;

            var message = e.UpdateType == StockUpdateType.PriceChanged
                ? $"[Трейдер {_name}] {e.StockData.Symbol}: {e.OldValue:F2}$ -> {e.NewValue:F2}$ ({(e.ChangePercent >= 0 ? "+" : "")}{e.ChangePercent:F2}%)"
                : $"[Трейдер {_name}] {e.StockData.Symbol}: объём {e.OldValue:F0} -> {e.NewValue:F0}";

            Console.WriteLine(message);
        }
    }

    // Бот-трейдер (реагирует только на цену выше порога)
    public class BotTrader : FilteredStockObserver
    {
        private readonly string _botId;
        private readonly decimal _buyThreshold;

        public BotTrader(string botId, decimal buyThreshold = 100m)
            : base(buyThreshold, notifyAboveThreshold: true, filterType: StockUpdateType.PriceChanged)
        {
            _botId = botId;
            _buyThreshold = buyThreshold;
        }

        protected override void OnUpdate(StockUpdateEventArgs e)
        {
            Console.WriteLine($"[Бот {_botId}] СИГНАЛ НА ПОКУПКУ: {e.StockData.Symbol} по {e.NewValue:F2}$ (выше порога {_buyThreshold:F2}$)");

            var quantity = CalculatePosition(e.NewValue);
            Console.WriteLine($"Исполнение: покупка {quantity} шт. на сумму {quantity * e.NewValue:F2}$");
        }

        private int CalculatePosition(decimal price) => (int)(10000 / price);
    }

    // Логгер
    public class Logger : IStockObserver
    {
        private readonly List<string> _logBuffer = new List<string>();
        private readonly bool _consoleOutput;

        public Logger(bool consoleOutput = true)
        {
            _consoleOutput = consoleOutput;
        }

        public void Update(StockUpdateEventArgs e)
        {
            var logEntry = $"[{e.StockData.Timestamp:HH:mm:ss}] {e.StockData.Symbol,-6} | " +
                          $"{e.UpdateType,-14} | " +
                          $"{e.OldValue,10:F4} -> {e.NewValue,-10:F4} | " +
                          $"{(e.NewValue - e.OldValue),8:F4}";

            _logBuffer.Add(logEntry);

            if (_consoleOutput)
            {
                Console.WriteLine($"{logEntry}");
            }

            if (_logBuffer.Count > 1000)
            {
                _logBuffer.RemoveRange(0, 100);
            }
        }

        public IReadOnlyList<string> GetLogs() => _logBuffer.AsReadOnly();

        public void ClearLogs() => _logBuffer.Clear();
    }

    // Система оповещений (реагирует на аномалии)
    public class AlertSystem : IStockObserver
    {
        private const decimal PriceChangeAlertThreshold = 5m;
        private const decimal VolumeSpikeMultiplier = 3m;
        private const decimal CriticalPriceChangeThreshold = 10m;

        public void Update(StockUpdateEventArgs e)
        {
            if (e.UpdateType == StockUpdateType.PriceChanged)
            {
                CheckPriceAlert(e);
            }
            else if (e.UpdateType == StockUpdateType.VolumeChanged)
            {
                CheckVolumeAlert(e);
            }
        }

        private void CheckPriceAlert(StockUpdateEventArgs e)
        {
            if (e.OldValue == 0) return;

            var absChangePercent = Math.Abs(e.ChangePercent);

            if (absChangePercent >= CriticalPriceChangeThreshold)
            {
                Console.WriteLine($"[КРИТИЧЕСКОЕ ПРЕДУПРЕЖДЕНИЕ] {e.StockData.Symbol}: " +
                                 $"{(e.IsIncrease ? "РЕЗКИЙ РОСТ" : "ОБВАЛ")} на {absChangePercent:F2}%!");
                Console.WriteLine($"Цена: {e.OldValue:F2}$ -> {e.NewValue:F2}$");
            }
            else if (absChangePercent >= PriceChangeAlertThreshold)
            {
                Console.WriteLine($"[ПРЕДУПРЕЖДЕНИЕ] {e.StockData.Symbol}: " +
                                 $"{(e.IsIncrease ? "рост" : "падение")} на {absChangePercent:F2}% " +
                                 $"({e.OldValue:F2}$ -> {e.NewValue:F2}$)");
            }
        }

        private void CheckVolumeAlert(StockUpdateEventArgs e)
        {
            if (e.OldValue == 0) return;

            var multiplier = e.NewValue / e.OldValue;

            if (multiplier >= VolumeSpikeMultiplier)
            {
                Console.WriteLine($"[АНОМАЛЬНЫЙ ОБЪЁМ] {e.StockData.Symbol}: всплеск в {multiplier:F1} раз!");
                Console.WriteLine($"Объём: {e.OldValue:F0} -> {e.NewValue:F0}");
            }
        }
    }
}
