using System.Diagnostics;
using System.Text;

namespace Benchmark.Services;

public class MetricsService
{
    private const int MetricsDataSize = 16;

    private readonly DiskMetricsCollector _diskMetrics;
    private readonly List<Metrics> _diskMetricsData;
    private readonly MemoryMetricsCollector _memoryMetrics;
    private readonly List<Metrics> _memoryMetricsData;

    public string BenchmarkId { get; }

    public MetricsService(string benchmarkId)
    {
        BenchmarkId = benchmarkId;

        _diskMetrics = new();
        _diskMetricsData = new(MetricsDataSize);

        _memoryMetrics = new();
        _memoryMetricsData = new(MetricsDataSize);
    }

    public async Task CollectMetricsWhileProcessRunAsync(
        Process process, TimeSpan period, CancellationToken cancellation = default)
    {
        using PeriodicTimer timer = new(period);

        try
        {
            while (!process.HasExited)
            {
                cancellation.ThrowIfCancellationRequested();

                _diskMetricsData.Add(_diskMetrics.Collect());
                _memoryMetricsData.Add(_memoryMetrics.Collect());

                if (!await timer.WaitForNextTickAsync(cancellation).ConfigureAwait(false))
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // Normal cancellation, supress
        }
    }

    public string GetString()
    {
        const double k = 1.0 / 1_000_000;
        StringBuilder result = new("{\n", 128);

        result.AppendLine("  Disk:");
        foreach (Metrics item in _diskMetricsData)
        {
            result.AppendFormat("    {0:F1} of {1:F1} MB\n",
                item.AvailableBytes * k, item.TotalBytes * k);
        }

        result.AppendLine("  Memory:");
        foreach (Metrics item in _memoryMetricsData)
        {
            result.AppendFormat("    {0:F1} of {1:F1} MB\n",
                item.AvailableBytes * k, item.TotalBytes * k);
        }

        result.AppendLine("}");
        return result.ToString();
    }

    public Task SaveAsync()
    {
        Dictionary<string, MetricsSummary> categories = new();

        AddSummary(categories, Measures.Memory, _memoryMetricsData);
        AddSummary(categories, Measures.Disk, _diskMetricsData);

        return MeasureStorage.SaveAsync(BenchmarkId, categories);
    }

    private static void AddSummary(
        Dictionary<string, MetricsSummary> categories,
        string category,
        List<Metrics> samples)
    {
        if (samples.Count == 0)
        {
            return;
        }

        long min = samples[0].AvailableBytes;
        long max = samples[0].AvailableBytes;;
        samples.ForEach(x => 
        {
            min = Math.Min(x.AvailableBytes, min);
            max = Math.Min(x.AvailableBytes, max);
        });

        categories[category] = new MetricsSummary(
            samples[0].TotalBytes,
            samples.Min(sample => sample.AvailableBytes),
            samples.Max(sample => sample.AvailableBytes));
    }
}
