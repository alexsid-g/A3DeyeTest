using Benchmark.Services;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmark.Data;

public sealed class CustomMeasureColumn : IColumn
{
    private readonly Dictionary<string, MetricsSummary> _values = new();

    public string Measure { get; }

    public string Id => Measure;

    public string ColumnName => Measure;

    public string Legend => "[Total]: Min, Delta Consumed, MB";

    public UnitType UnitType => UnitType.Dimensionless;

    public bool IsNumeric => false;

    public bool AlwaysShow => true;
    
    public ColumnCategory Category => ColumnCategory.Custom;
    
    public int PriorityInCategory => 0;

    public bool IsGreaterBetter => false;

    public CustomMeasureColumn(string measure)
    {
        Measure = measure;
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase) =>
        GetValue(summary, benchmarkCase, summary.Style);

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
    {
        if (benchmarkCase.HasParameters && (benchmarkCase.Parameters[0].Value is IBenchmarkArgs args))
        {
            if (!_values.TryGetValue(args.BenchmarkId, out MetricsSummary? value))
            {
                value = MeasureStorage.LoadSummary(args.BenchmarkId, Measure);
                _values.Add(args.BenchmarkId, value);
            }

            if (value == MetricsSummary.NA)
            {
                return "NA";
            }

            double k = 1.0 / 1_000_000;
            double consumedMin = k * (value.TotalBytes - value.MaxBytes);
            double consumedDelta = k * (value.MaxBytes - value.MinBytes);
            return $"[{k * value.TotalBytes:F1}]: {consumedMin:F1}, +{consumedDelta:F1} MB";
        }

        return "NA";
    }

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => true;
    
    public bool IsAvailable(Summary summary) => true;
}
