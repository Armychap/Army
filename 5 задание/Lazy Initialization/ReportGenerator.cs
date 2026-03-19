using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LazyInitializationReports
{
    
    // Генератор отчётов, использует DataWarehouse, но сам DataWarehouse создаётся лениво
    public sealed class ReportGenerator
    {
        private readonly Lazy<DataWarehouse> _warehouse;

        // Создаёт ленивый обёрток над DataWarehouse, чтобы фактический объект создавался при первом обращении
        public ReportGenerator()
        {
            _warehouse = new Lazy<DataWarehouse>(() => new DataWarehouse());
        }

        // Хотим проверить, инициализировалось ли хранилище
        public bool IsWarehouseInitialized => _warehouse.IsValueCreated;

        // Сбрасывает ленивое хранилище, чтобы оно создалось заново при следующем обращении
        public void ResetWarehouse()
        {
            _warehouse.Reset();
        }
        
        // Генерирует отчёт, обращаясь к ленивому складу только когда это необходимо
        
        public Report Generate(ReportType type, DateOnly date)
        {
            var sw = Stopwatch.StartNew();

            //  здесь (при первом Generate) мы впервые обращаемся к _warehouse.Value, и DataWarehouse создаётся  сейчас, а не в конструкторе ReportGenerator
            IReadOnlyList<ReportRow> rows = _warehouse.Value.QueryMetrics(type, date);

            sw.Stop();
            return new Report(type, date, rows, sw.Elapsed);
        }
    }
}

