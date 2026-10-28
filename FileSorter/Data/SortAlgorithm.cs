namespace FileSorter.Data;

public enum SortAlgorithm
{
    /// <summary>
    /// The best one. Single-path sorting.
    /// </summary>
    SortAndSave = 0,
    /// <summary>
    /// First import to duckdb then sort and save.
    /// </summary>
    ImportSortAndSave,
    /// <summary>
    /// First split to parquet files then sort and save.
    /// </summary>
    SplitSortAndSave,
}
