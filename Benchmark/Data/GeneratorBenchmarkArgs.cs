using Benchmark.Services;
using Benchmark.Utils;

namespace Benchmark.Data;

public class GeneratorBenchmarkArgs : IComparable<GeneratorBenchmarkArgs>, IComparable
{
    private static int _nextId;

    public int Id { get; }

    public string Size { get; init; }

    public bool RunFast { get; init; }

    public string OutputFile { get; init; } 

    public string BenchmarkId => RunFast ? $"FGF_{Id}_{Size}" : $"FG_{Id}_{Size}";

    public GeneratorBenchmarkArgs(string size, bool runFast)
    {
        Id = Interlocked.Increment(ref _nextId);
        Size = size;
        RunFast = runFast;

        string? dataDirectory = BenchmarksEnvironment.GetDataDirectory();
        if (string.IsNullOrEmpty(dataDirectory) || !Directory.Exists(dataDirectory))
        {
            throw new InvalidOperationException("Data directory not found");
        }

        OutputFile = Path.Combine(dataDirectory, $"test-data-{size}.txt");
    }

    public override string ToString() => RunFast ? $"[{Size}]:F" : $"[{Size}]";

    public int CompareTo(GeneratorBenchmarkArgs? other)
    {
        if (other is null)
        {
            return 1;
        }

        long size = SizeParser.Parse(Size);
        return size.CompareTo(SizeParser.Parse(other.Size));
    }

    public int CompareTo(object? obj) => obj is GeneratorBenchmarkArgs other
        ? CompareTo(other)
        : throw new ArgumentException();
}
