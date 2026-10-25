using System.Text.Json;
using Benchmark.Utils;
using Benchmarks.Data;

namespace Benchmark.Services;

public static class MeasureStorage
{
    private static readonly JsonSerializerOptions _jsonOptions = new();

    public static IDisposable InitializeSession()
    {
        string directory = Path.Combine(Path.GetTempPath(), Consts.MetricsDirectoryEnv);
        Directory.CreateDirectory(directory);

        Environment.SetEnvironmentVariable(Consts.MetricsDirectoryEnv, directory);
        return new MetricsSession(directory);
    }

    public static Task SaveAsync(string benchmarkId, Dictionary<string, MetricsSummary> categories)
    {
        string path = GetFilePath(benchmarkId);
        StoredMetrics metrics = new(benchmarkId, categories);
        return File.WriteAllTextAsync(path, JsonSerializer.Serialize(metrics, _jsonOptions));
    }

    public static MetricsSummary LoadSummary(string benchmarkId, string category)
    {
        string path = GetFilePath(benchmarkId);
        if (!File.Exists(path))
        {
            return MetricsSummary.NA;
        }

        string content = File.ReadAllText(path);
        StoredMetrics? stored = JsonSerializer.Deserialize<StoredMetrics>(content, _jsonOptions);
        if (stored is not null && stored.Categories.TryGetValue(category, out MetricsSummary? summary))
        {
            return summary;
        }

        return MetricsSummary.NA;
    }

    private static string GetFilePath(string benchmarkId)
    {
        string directory = Environment.GetEnvironmentVariable(Consts.MetricsDirectoryEnv)
            ?? Path.Combine(Path.GetTempPath(), Consts.MetricsDirectoryEnv);

        return Path.Combine(directory, $"{Uri.EscapeDataString(benchmarkId)}.json");
    }

    private sealed record StoredMetrics(
        string BenchmarkId, Dictionary<string, MetricsSummary> Categories);

    private sealed class MetricsSession : IDisposable
    {
        private string? _directory;
        private bool _disposed = false;

        public MetricsSession(string directory)
        {
            _directory = directory;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (Environment.GetEnvironmentVariable(Consts.MetricsDirectoryEnv) == _directory)
            {
                Environment.SetEnvironmentVariable(Consts.MetricsDirectoryEnv, null);
            }

            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }
    }
}
