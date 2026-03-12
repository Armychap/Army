using System;
using System.Collections.Generic;

namespace LazyInitializationReports
{
    public enum ReportType
    {
        Daily,
        Weekly,
        Monthly
    }

    public sealed record ReportRow(string Metric, double Value);

    public sealed record Report(
        ReportType Type,
        DateOnly Date,
        IReadOnlyList<ReportRow> Rows,
        TimeSpan GenerationTime
    );
}

