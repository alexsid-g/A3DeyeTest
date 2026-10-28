namespace Benchmark.Data;

public interface IBenchmarkArgs : IComparable
{
    public int Id { get; }

    public string BenchmarkId { get; }
}
