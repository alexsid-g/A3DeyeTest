using FileSorter.Data;

namespace FileSorter.Strategies;

public static class SortStrategyFactory
{
    public static ISortStrategy Create(FileSortingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.InputFilePath))
        {
            throw new ArgumentException("An input file path is required.", nameof(options));
        }

        long memoryLimit = FileSortingOptions.ParseMemoryLimit(options.MemoryLimit);
        if (memoryLimit <= 0)
        {
            return new SimpleSortStrategy();
        }

        FileInfo inputFile = new(options.InputFilePath);
        return SimpleSortStrategy.EstimateRequiredMemory(inputFile) <= memoryLimit
            ? new SimpleSortStrategy()
            : new LargeFileSortStrategy();
    }
}