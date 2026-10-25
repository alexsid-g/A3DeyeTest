using Benchmark.Services.SystemCollectors;

namespace Benchmark.Services;

public class MemoryMetricsCollector
{
    private readonly ISystemCollector _collector;

    public MemoryMetricsCollector()
    {
#if WINDOWS
        _collector = new WindowsMemoryCollector();
#elif LINUX
        _collector = new LinuxMemoryCollector();
#elif MACOS
        _collector = new MacMemoryCollector();
#else
        _collector = () => Metrics.Empty;
#endif
    }

    public Metrics Collect() => _collector.Collect();
}
