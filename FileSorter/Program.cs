using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using FileSorter.Data;
using FileSorter.Strategies;

IHostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

Dictionary<string, string> switchMappings = new()
{
    ["-i"] = nameof(FileSortingOptions.InputFilePath),
    ["-o"] = nameof(FileSortingOptions.OutputFilePath),
    ["-m"] = nameof(FileSortingOptions.MemoryLimit),
    ["-d"] = nameof(FileSortingOptions.TempDirectory)
};
builder.Configuration.AddCommandLine(args, switchMappings);

FileSortingOptions? options = builder.Configuration.Get<FileSortingOptions>();
if (options is null || string.IsNullOrEmpty(options.InputFilePath) || !File.Exists(options.InputFilePath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Invalid options. Please provide valid input file path.");
    Console.ResetColor();

    PrintUsage();
    return;
}

Console.Write($"File sorting started");
Stopwatch stopwatch = Stopwatch.StartNew();

ISortStrategy strategy = SortStrategyFactory.Create(options);
strategy.Run(options);

stopwatch.Stop();

FileInfo fileInfo = new(options.InputFilePath);
string memoryLimit = FileSortingOptions.ParseMemoryLimit(options.MemoryLimit) > 0
    ? options.MemoryLimit!
    : "no limit";

Console.ForegroundColor = ConsoleColor.Green;
Console.Write($"File sorting [{strategy.GetType().Name}]: ");
Console.WriteLine($"{fileInfo.Length/1_000_000} MB in {stopwatch.Elapsed.TotalSeconds:F2} sec");
Console.WriteLine($"Memory limit: {memoryLimit}");
Console.ResetColor();

static void PrintUsage()
{
    Console.WriteLine("Usage: FileSorter -i <input_file_path> [-o <output_file_path>] [-m <memory_limit>] [-d <temp_directory>]");
    Console.WriteLine("Example: TestFileGenerator -i input.txt -m 10MB");
}
