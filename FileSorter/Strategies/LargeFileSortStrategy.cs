using DuckDB.NET.Data;
using FileSorter.Data;
using FileSorter.Extensions;

namespace FileSorter.Strategies;

public class LargeFileSortStrategy : ISortStrategy
{
    public void Run(FileSortingOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.InputFilePath);
        ArgumentException.ThrowIfNullOrEmpty(options.OutputFilePath);

        bool tempDirectoryIsCreated = false;
        string tempDirectory = string.IsNullOrEmpty(options.TempDirectory)
            ? Environment.CurrentDirectory
            : options.TempDirectory;

        if (!Directory.Exists(tempDirectory))
        {
            tempDirectoryIsCreated = true;
        }

        string tempFile = Path.Combine(tempDirectory, "duckdb.tmp");

        try
        {
            string? outputFile = options.OutputFilePath?.Trim();
            if (string.IsNullOrEmpty(outputFile))
            {
                outputFile = $"sorted_{options.InputFilePath}";
            }

            using DuckDBConnection connection = CreateAndInitializeDb(
                options, tempDirectory, tempFile);

            switch (options.LargeFileSortingAlgorithm)
            {
                case SortingAlgorithm.SortAndSave:
                    SortAndSaveData(connection, options.InputFilePath, outputFile);
                    break;
                case SortingAlgorithm.ImportSortAndSave:
                    ImportThenSortAndSaveData(connection, options.InputFilePath, outputFile);
                    break;
                case SortingAlgorithm.SplitSortAndSave:
                    SplitThenSortAndSaveData(connection, options.InputFilePath, outputFile, tempDirectory);
                    break;
                default: throw new InvalidOperationException(
                    $"Invalid sorting algorithm {options.LargeFileSortingAlgorithm}"); 
            }
        }
        finally
        {
            // Clear
            if (tempDirectoryIsCreated)
            {
                Directory.Delete(tempDirectory, true);
            }
            else if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private static DuckDBConnection CreateAndInitializeDb(
        FileSortingOptions options, string tempDirectory, string tempFilePath)
    {
        DuckDBConnection conn = new($"DataSource='{tempFilePath}'");
        conn.Open();

        long memoryLimitBytes = FileSortingOptions.ParseMemoryLimit(options.MemoryLimit);
        string memoryLimitSql = memoryLimitBytes > 0 
            ? $"SET memory_limit = '{options.MemoryLimit}';" 
            : "";

        int threads = Environment.ProcessorCount;
        string sql = @$"
            SET threads = {threads};
            SET preserve_insertion_order = false;
            SET temp_directory = '{tempDirectory}';
            SET max_temp_directory_size = '150GB';
            {memoryLimitSql}
        ";

        conn.ExecuteNonQuery(sql);

        return conn;
    }

    private static void SortAndSaveData(
        DuckDBConnection connection, string inputFile, string outputFile)
    {
        string sql = @$"
            COPY (
                SELECT Number, Value
                FROM read_csv(
                    '{inputFile}',
                    delim = '.',
                    header = false,
                    columns = {{
                        'Number': 'BIGINT',
                        'Value': 'VARCHAR'
                    }},
                    parallel = true
                )
                ORDER BY Value, Number
                
            ) TO '{outputFile}'
            (FORMAT CSV, HEADER false, DELIMITER '.');
        ";

        connection.ExecuteNonQuery(sql);
    }

    private static void ImportThenSortAndSaveData(
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
                SELECT *
                FROM temp_sorting
                ORDER BY Value, Number
            ) TO '{outputFile}' 
            (FORMAT CSV, HEADER false, DELIMITER '.');
        ";
        
        connection.ExecuteNonQuery(sql);
    }

    private static void SplitThenSortAndSaveData(
        DuckDBConnection connection, string inputFile, string outputFile, string tempDirectory)
    {
        string parquetsPath = Path.Combine(tempDirectory, "parquets");
        try
        {
            string format = GetFormatFor("PARQUET");
            string sql = @$"
                COPY (
                    SELECT Number, Value
                    FROM read_csv(
                        '{inputFile}',
                        delim = '.',
                        header = false,
                        columns = {{
                            'Number': 'BIGINT',
                            'Value': 'VARCHAR'
                        }},
                        parallel = true
                    )
                ) TO '{parquetsPath}'
                (FORMAT {format}, OVERWRITE true);
            ";
            
            connection.ExecuteNonQuery(sql);

            // sort and save
            sql = @$"
                COPY (
                    SELECT Number, Value
                    FROM read_parquet('{parquetsPath}/*.parquet')
                    ORDER BY Value, Number
                ) TO '{outputFile}'
                (FORMAT CSV, HEADER false, DELIMITER '.');
            ";
            
            connection.ExecuteNonQuery(sql);

        }
        finally
        {
            if (Directory.Exists(parquetsPath))
            {
                Directory.Delete(parquetsPath, true);
            }
        }
    }

    private static string GetFormatFor(string name)
    {
        name = name.Trim().ToUpperInvariant();
        if (name.Equals("PARQUET"))
        {
            return $"{name}, COMPRESSION snappy" +
                ", ROW_GROUP_SIZE 100000 " +
                ", ROW_GROUPS_PER_FILE 5" +
                ", PER_THREAD_OUTPUT true";
        }

        throw new InvalidOperationException($"Unsupported format: {name}");
    }
}
