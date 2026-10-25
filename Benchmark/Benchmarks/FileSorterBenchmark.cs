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
        new("10MB", false), new("100MB", false), new("1GB", false),
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
        using Process process = ProcessStarter.Start("dotnet", options);
        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start TestFileGenerator");
        }

        // 1 time per sec
        TimeSpan period = TimeSpan.FromMilliseconds(1000);
        await metrics.CollectMetricsWhileProcessRunAsync(process, period);

        // string stdout = process.StandardOutput.ReadToEnd();
        // string stderr = process.StandardError.ReadToEnd();
        // ConsoleUtils.WriteInfoLine(sorterDll);
        // ConsoleUtils.WriteErrorLine(stdout);
        // ConsoleUtils.WriteErrorLine(stderr);

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
