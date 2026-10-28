using Benchmark.Data;
using Benchmark.Services;
using Benchmark.Utils;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using System.Diagnostics;

namespace Benchmark.Benchmarks;

[SimpleJob(
    RunStrategy.ColdStart,
    launchCount: 1,
    warmupCount: 0,
    iterationCount: 1,
    invocationCount: 1)]
[MemoryDiagnoser(false)]
[Config(typeof(FileSorterBenchmarkConfig))]
public class FileSorterBenchmark
{
    [ParamsSource(nameof(ValuesForArgs))]
    public FileSorterBenchmarkArgs Args { get; set; } = null!;

    public IEnumerable<FileSorterBenchmarkArgs> ValuesForArgs => [
        new("10MB"), new("100MB"), new("1GB"),
        // Use sort algorithms for a big files
        new("1GB", true, 0), new("1GB", true, 1), new("1GB", true, 2),
    ];

    [Benchmark]
    public async Task GenerateFile()
    {
        try
        {
            MetricsService metrics = new(Args.BenchmarkId);
            await RunTestFileSorter(Args, metrics);
            await metrics.SaveAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            throw;
        }
    }

    private static async Task RunTestFileSorter(FileSorterBenchmarkArgs args, MetricsService metrics)
    {
        string sorterDll = Path.GetFullPath(
            "./FileSorter/bin/Release/net8.0/FileSorter.dll",
            BenchmarksEnvironment.GetSolutionDirectory() ?? throw new Exception("Solution directory not found"));

        // No use memory limit for now
        string[] options = [sorterDll, "-i", args.InputFile, "-o", args.OutputFile, "-m", "2GB"];
        if (args.IsBig)
        {
            FileInfo file = new(args.InputFile);
            string memory = file.Length switch
            {
                _ when file.Length > 1_000_000_000 => $"{file.Length / 1_000_000_000}GB",
                _ when file.Length > 500_000_000 => $"{file.Length / 1_000_000}MB",
                _ => throw new InvalidDataException("File is not big (less than 500MB)")
            };

            options =[sorterDll, "-i", args.InputFile, "-o", args.OutputFile, "-m", memory,"-sa", args.SortAlgorithm.ToString()];
        }

        using Process process = ProcessStarter.Start("dotnet", options);
        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start TestFileGenerator");
        }

        // 1 time per sec
        TimeSpan period = TimeSpan.FromMilliseconds(1000);
        await metrics.CollectMetricsWhileProcessRunAsync(process, period);

        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        ConsoleUtils.WriteInfoLine(sorterDll);
        ConsoleUtils.WriteErrorLine(stdout);
        ConsoleUtils.WriteErrorLine(stderr);

        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"TestFileGenerator exited with code {process.ExitCode}.");
        }
    }
}

public sealed class FileSorterBenchmarkConfig : ManualConfig
{
    public FileSorterBenchmarkConfig()
    {
        AddColumn(new CustomMeasureColumn(Measures.Memory));
        AddColumn(new CustomMeasureColumn(Measures.Disk));

        HideColumns("Allocated");
    }
}
