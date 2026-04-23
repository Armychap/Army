using Observer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Observer
{
    public static class Demo
    {
        public static void Run()
        {
            Console.WriteLine("== БИРЖЕВОЙ МОНИТОРИНГ ==");

            var ticker = new StockTicker();

            var traders = new[]
            {
                new Trader("Николай"),
                new Trader("Александра"),
                new Trader("Дмитрий", verbose: true)
            };

            var botTrader = new BotTrader("001", buyThreshold: 100m);
            var logger = new Logger(consoleOutput: true);
            var alertSystem = new AlertSystem();

            Console.WriteLine("ПОДПИСКА НАБЛЮДАТЕЛЕЙ");

            foreach (var trader in traders)
            {
                ticker.Attach(trader);
            }
            ticker.Attach(botTrader);
            ticker.Attach(logger);
            ticker.Attach(alertSystem);

            Console.WriteLine($"\nПодписано наблюдателей: {traders.Length + 3}\n");

            SimulateTrading(ticker);

            Console.WriteLine("\nОТПИСКА НАБЛЮДАТЕЛЯ");
            ticker.Detach(traders[1]); // отписываем
            Console.WriteLine();

            SimulateMoreTrading(ticker);
        }

        private static void SimulateTrading(StockTicker ticker)
        {
            Console.WriteLine("НАЧАЛО ТОРГОВ\n");

            ticker.UpdateStockPrice("APPLE", 150.50m);
            Thread.Sleep(200);

            ticker.UpdateStockVolume("APPLE", 1_000_000m);
            Thread.Sleep(200);

            ticker.UpdateStockPrice("NVIDIA", 95.00m);
            Thread.Sleep(200);

            ticker.UpdateStockPrice("NVIDIA", 105.00m); // Пробитие порога бота
            Thread.Sleep(200);

            ticker.UpdateStockVolume("NVIDIA", 500_000m);
            Thread.Sleep(200);

            ticker.UpdateStockPrice("TESLA", 250.75m);
            Thread.Sleep(200);

            ticker.UpdateStockPrice("TESLA", 240.25m); // -4.2%
            Thread.Sleep(200);

            ticker.UpdateStockVolume("TESLA", 2_000_000m);
            Thread.Sleep(200);

            ticker.UpdateStockPrice("BTC-USD", 45000m);
            Thread.Sleep(200);

            ticker.UpdateStockPrice("BTC-USD", 42000m); // -6.7%, сработает alert
            Thread.Sleep(200);

            ticker.UpdateStockPrice("MEME", 25.00m);
            Thread.Sleep(200);

            ticker.UpdateStockVolume("MEME", 500_000m);
            Thread.Sleep(200);

            ticker.UpdateStockVolume("MEME", 2_000_000m); // Всплеск x4, сработает alert
        }

        private static void SimulateMoreTrading(StockTicker ticker)
        {
            Console.WriteLine("ПРОДОЛЖЕНИЕ (отписан 1 трейдер)\n");

            ticker.UpdateStockPrice("APPLE", 152.00m);
            Thread.Sleep(200);

            ticker.UpdateStockPrice("TESLA", 235.00m); // -6.2%, alert
            Thread.Sleep(200);

            ticker.UpdateStockPrice("NVDIA", 112.50m);
            Thread.Sleep(200);

            ticker.UpdateStockPrice("BTC-USD", 43800m);
            Thread.Sleep(200);

            ticker.UpdateStockVolume("APPLE", 1_500_000m);
        }
    }
}
