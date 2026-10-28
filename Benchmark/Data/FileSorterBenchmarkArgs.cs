using Benchmark.Services;
using Benchmark.Utils;

namespace Benchmark.Data;

public class FileSorterBenchmarkArgs : IBenchmarkArgs
{
    private static int _nextId;

    public int Id { get; }

    public string Size { get; init; }

    public string InputFile { get; init; }

    public string OutputFile { get; init; }
    public bool IsBig { get; init; }
    /// <summary>
    /// Valid for big files only. Default is 0 (SortAndSave).
    /// 0 - SortAndSave - simple sort and save. Duckdb decides how to proceed.
    /// 1 - ImportSortAndSave - import to a single DUCKDB file, then sort and save from it.
    /// 2 - SplitSortAndSave - import to multiple PARQUET files, then sort and save from it
    /// </summary>
    public int SortAlgorithm { get; init; }

    public string BenchmarkId => $"FS_{Id}_{Size}";

    public FileSorterBenchmarkArgs(string size) : this(size, false, 0)
    {
        
    }

    public FileSorterBenchmarkArgs(string size, bool isBig, int sortAlgorithm)
    {
        Id = Interlocked.Increment(ref _nextId);
        Size = size;
        IsBig = isBig;
        SortAlgorithm = sortAlgorithm;

        if (SortAlgorithm < 0 && 2 < SortAlgorithm)
        {
            throw new ArgumentException("Invalid sort algorithm", nameof(sortAlgorithm));
        }

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

    public int CompareTo(object? obj) => obj is FileSorterBenchmarkArgs other
        ? CompareTo(other)
        : throw new ArgumentException();

    public int CompareTo(FileSorterBenchmarkArgs? other)
    {
        if (other is null)
        {
            return 1;
        }

        long size = SizeParser.Parse(Size);
        return size.CompareTo(SizeParser.Parse(other.Size));
    }
}
