using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LazyInitializationReports
{
    /// <summary>
    /// Генератор отчётов. Использует DataWarehouse, но сам DataWarehouse создаётся лениво.
    /// </summary>
    public sealed class ReportGenerator
    {
        private readonly Lazy<DataWarehouse> _warehouse;

        public ReportGenerator()
        {
            _warehouse = new Lazy<DataWarehouse>(() => new DataWarehouse());
        }

        public bool IsWarehouseInitialized => _warehouse.IsValueCreated;

        public Report Generate(ReportType type, DateOnly date)
        {
            var sw = Stopwatch.StartNew();

            // Важный момент паттерна:
            // именно здесь (при первом Generate) мы впервые обращаемся к _warehouse.Value,
            // и DataWarehouse создаётся только сейчас, а не в конструкторе ReportGenerator.
            IReadOnlyList<ReportRow> rows = _warehouse.Value.QueryMetrics(type, date);

            sw.Stop();
            return new Report(type, date, rows, sw.Elapsed);
        }
    }
}

