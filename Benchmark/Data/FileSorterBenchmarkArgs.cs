using Benchmark.Services;
using Benchmark.Utils;

namespace Benchmark.Data;

public class FileSorterBenchmarkArgs : IComparable<FileSorterBenchmarkArgs>, IComparable
{
    private static int _nextId;

    public int Id { get; }

    public string Size { get; init; }

    public string InputFile { get; init; }

    public string OutputFile { get; init; }

    public string BenchmarkId => $"FS_{Id}_{Size}";

    public FileSorterBenchmarkArgs(string size, bool isLarge)
    {
        Id = Interlocked.Increment(ref _nextId);
        Size = size;

        string? dataDirectory = BenchmarksEnvironment.GetDataDirectory();
        if (string.IsNullOrEmpty(dataDirectory) || !Directory.Exists(dataDirectory))
        {
            throw new InvalidOperationException("Data directory not found");
        }

        InputFile = Path.Combine(dataDirectory, $"test-data-{size}.txt");
        if (!File.Exists(InputFile))
        {
            throw new InvalidOperationException($"Input file not found {InputFile}");
        }

        OutputFile = Path.Combine(dataDirectory, $"test-data-{size}-sorted.txt");
    }

    public override string ToString() => $"[{Size}]";

    public int CompareTo(FileSorterBenchmarkArgs? other)
    {
        if (other is null)
        {
            return 1;
        }

        long size = SizeParser.Parse(Size);
        return size.CompareTo(SizeParser.Parse(other.Size));
    }

    public int CompareTo(object? obj) => obj is FileSorterBenchmarkArgs other
        ? CompareTo(other)
        : throw new ArgumentException();
}
