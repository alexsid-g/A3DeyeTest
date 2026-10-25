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

    public string Legend => "[Total]: Min-Max Available, MB";

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
        if (benchmarkCase.HasParameters && benchmarkCase.Parameters[0].Value is GeneratorBenchmarkArgs args)
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
            return $"[{k * value.TotalBytes:F1}]: {k * value.MinBytes:F1}-{k * value.MaxBytes:F1}";
        }

        return "NA";
    }

    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => true;
    
    public bool IsAvailable(Summary summary) => true;
}
