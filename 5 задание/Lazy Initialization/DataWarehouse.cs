using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace LazyInitializationReports
{
    
    // Склад данных — тяжёлый объект
    public sealed class DataWarehouse
    {
        public Guid ConnectionId { get; } = Guid.NewGuid();
        public DateTime ConnectedAt { get; } = DateTime.Now;

        
        // Симулирует подключение к удалённому хранилищу
        public DataWarehouse()
        {
            Console.WriteLine("[DataWarehouse] Подключение к хранилищу данных...");
            Thread.Sleep(900);
            Console.WriteLine("[DataWarehouse] Прогрев кеша и загрузка справочников...");
            Thread.Sleep(700);
            Console.WriteLine("[DataWarehouse] Готово.");
        }

        
        // Возвращает набор строк отчёта, имитируя выполнение запроса к реальной БД
        // Метод вызывается через ленивый объект
        public IReadOnlyList<ReportRow> QueryMetrics(ReportType type, DateOnly date)
        {
            var sw = Stopwatch.StartNew();

            // Имитируем сетевой/базовый запрос разной длительности
            Thread.Sleep(type switch
            {
                ReportType.Daily => 250,
                ReportType.Weekly => 450,
                ReportType.Monthly => 700,
                _ => 300
            });

            // Чуть разнообразия, чтобы было видно, что объекты действительно разными
            var seed = HashCode.Combine((int)type, date.DayNumber, ConnectionId);
            var rnd = new Random(seed);

            var rows = new List<ReportRow>
            {
                new("Requests", rnd.Next(10_000, 40_000)),
                new("Errors", rnd.Next(0, 400)),
                new("Revenue", Math.Round(rnd.NextDouble() * 100_000, 2)),
                new("AvgLatencyMs", Math.Round(20 + rnd.NextDouble() * 180, 2)),
            };

            sw.Stop();
            Console.WriteLine($"[DataWarehouse] QueryMetrics({type}) выполнен за {sw.ElapsedMilliseconds}мс.");
            return rows;
        }
    }
}

