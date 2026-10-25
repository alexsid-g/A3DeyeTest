namespace Benchmark.Services.SystemCollectors;

internal class LinuxMemoryCollector : ISystemCollector
{
#if LINUX
    private const string KeyTotal = "MemTotal:";
    private const string KeyAvailable = "MemAvailable:";

    private readonly long _totalMemory = 0;
#endif

    public LinuxMemoryCollector()
    {
#if LINUX
        _totalMemory = CollectMemoryMetrics(KeyTotal);
#endif
    }

    public Metrics Collect()
    {
#if LINUX
        long availableMemory = CollectMemoryMetrics(KeyAvailable);
        return new Metrics(_totalMemory, availableMemory);
#else
        return Metrics.Empty;
#endif
    }

#if LINUX
    private static long CollectMemoryMetrics(string key)
    {
        foreach (string line in File.ReadLines("/proc/meminfo"))
        {
            if (!line.StartsWith(key, StringComparison.Ordinal))
            {
                continue;
            }

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3 || !long.TryParse(parts[1], out long kBytes))
            {
                throw new InvalidOperationException($"Invalid memory value for {key}");
            }

            return kBytes * 1024;
        }

        throw new InvalidOperationException($"{key} was not found in /proc/meminfo");
    }
#endif
}