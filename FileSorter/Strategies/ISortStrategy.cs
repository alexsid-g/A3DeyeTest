using FileSorter.Data;

namespace FileSorter.Strategies;

public interface ISortStrategy
{
    void Run(FileSortingOptions options);
}
