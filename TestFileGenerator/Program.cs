using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TestFileGenerator.Data;
using TestFileGenerator.Strategies;

IHostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

Dictionary<string, string> switchMappings = new()
{
    ["-o"] = nameof(GenerateOptions.OutputFilePath),
    ["-s"] = nameof(GenerateOptions.TargetFileSize),
};
builder.Configuration.AddCommandLine(args, switchMappings);

GenerateOptions? options = builder.Configuration.Get<GenerateOptions>();

long targetFileSizeBytes = GenerateOptions.ParseTargetFileSize(options?.TargetFileSize);
if (options is null || string.IsNullOrEmpty(options.OutputFilePath) || targetFileSizeBytes <= 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Invalid options. Please provide valid output file path and target file size.");
    Console.ResetColor();

    PrintUsage();
    return;
}

Console.Write($"File generation started");
Stopwatch stopwatch = Stopwatch.StartNew();

bool isRunFast = args.Contains("--fast", StringComparer.OrdinalIgnoreCase);
IStrategy strategy = isRunFast 
    ? new FastStrategy()
    : new SimpleStrategy();

strategy.Run(options);

stopwatch.Stop();
Console.ForegroundColor = ConsoleColor.Green;
Console.Write($"File generation [{strategy.GetType().Name}]: ");
Console.WriteLine($"{options.TargetFileSize} in {stopwatch.Elapsed.TotalSeconds:F2} sec");
Console.ResetColor();

static void PrintUsage()
{
    Console.WriteLine("Usage: TestFileGenerator -o <output_file_path> -s <target_file_size> [--fast]");
    Console.WriteLine("Example: TestFileGenerator -o output.txt -s 10MB -fast");
}
