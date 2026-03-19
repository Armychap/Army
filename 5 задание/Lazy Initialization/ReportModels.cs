using System;
using System.Collections.Generic;

namespace LazyInitializationReports
{
    
    // Типы отчётов
    public enum ReportType
    {
        Daily,
        Weekly,
        Monthly
    }

    //Строка отчёта с метрикой и её значением
    public sealed record ReportRow(string Metric, double Value);

    // Данные, возвращаемые генератором отчётов
    public sealed record Report(
        ReportType Type,
        DateOnly Date,
        IReadOnlyList<ReportRow> Rows,
        TimeSpan GenerationTime
    );
}

