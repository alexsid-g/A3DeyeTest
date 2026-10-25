namespace Benchmark.Services.SystemCollectors;

internal interface ISystemCollector
{
    Metrics Collect();
}
