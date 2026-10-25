using System.Text;
using DuckDB.NET.Data;
using FileSorter.Data;
using FileSorter.Extensions;

namespace FileSorter.Strategies;

public class SimpleSortStrategy : ISortStrategy
{
    public static long EstimateRequiredMemory(FileInfo fileInfo)
    {
        // assume index size is 30% of file size
        return Convert.ToInt64(fileInfo.Length  * 1.3);
    }

    public void Run(FileSortingOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.InputFilePath);
        ArgumentException.ThrowIfNullOrEmpty(options.OutputFilePath);

        string tempDirectory = Environment.CurrentDirectory;
        using DuckDBConnection connection = CreateAndInitializeDb(tempDirectory);

        string? outputFile = options.OutputFilePath?.Trim();
        if (string.IsNullOrEmpty(outputFile))
        {
            outputFile = $"sorted_{options.InputFilePath}";
        }

        SortAndSaveData(connection, options.InputFilePath, outputFile);
    }

    private static DuckDBConnection CreateAndInitializeDb(string tempDirectory)
    {
        DuckDBConnection conn = new($"DataSource=:memory:");
        conn.Open();

        StringBuilder sql = new();

        int threads = Environment.ProcessorCount;
        sql.AppendLine($"SET threads = {threads};");
        sql.AppendLine($"SET preserve_insertion_order = false;");
        sql.AppendLine($"SET temp_directory = '{tempDirectory}';");
        sql.AppendLine($"SET max_temp_directory_size = '150GB';");

        conn.ExecuteNonQuery(sql.ToString());

        return conn;
    }

    private static void SortAndSaveData(
        DuckDBConnection connection, string inputFile, string outputFile)
    {
        string sql = @$"
            CREATE TEMP TABLE temp_sorting (
                Number BIGINT,
                Value VARCHAR
            );

            COPY temp_sorting FROM '{inputFile}' 
            (FORMAT CSV, HEADER false, DELIMITER '.');

            COPY (
                SELECT Number, Value 
                FROM temp_sorting
                ORDER BY Value, Number
            ) TO '{outputFile}' 
            (FORMAT CSV, HEADER false, DELIMITER '.');
        ";
        
        connection.ExecuteNonQuery(sql.ToString());
    }
}
