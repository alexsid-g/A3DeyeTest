namespace Benchmark.Services;

public record Metrics(long TotalBytes, long AvailableBytes)
{
    public static readonly Metrics Empty = new(0, 0);
}

public record MetricsSummary(long TotalBytes, long MinBytes, long MaxBytes)
{
    public static readonly MetricsSummary NA = new(0, 0, 0); 
}
