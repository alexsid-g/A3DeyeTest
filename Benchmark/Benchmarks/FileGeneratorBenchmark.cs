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
[Config(typeof(FileGeneratorBenchmarkConfig))]
public class FileGeneratorBenchmark
{
    [ParamsSource(nameof(ValuesForArgs))]
    public GeneratorBenchmarkArgs Args { get; set; } = null!;

    public IEnumerable<GeneratorBenchmarkArgs> ValuesForArgs => [
        new("10MB", false), new("100MB", false), new("1GB", false),
        //new("10MB", true), new("100MB", true), new("1GB", true),
    ];

    [Benchmark]
    public async Task SortFile()
    {
        try
        {
            MetricsService metrics = new(Args.BenchmarkId);
            await RunTestFileGenerator(Args, metrics);
            await metrics.SaveAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            throw;
        }
    }

    private static async Task RunTestFileGenerator(GeneratorBenchmarkArgs args, MetricsService metrics)
    {
        string generatorDll = Path.GetFullPath(
            "./TestFileGenerator/bin/Release/net8.0/TestFileGenerator.dll",
            BenchmarksEnvironment.GetSolutionDirectory() ?? throw new Exception("Solution directory not found"));

        string[] options = args.RunFast 
            ? [generatorDll, "-o", args.OutputFile, "-s", args.Size, "--fast"]
            : [generatorDll, "-o", args.OutputFile, "-s", args.Size];

        Stopwatch stopwatch = Stopwatch.StartNew(); 
        using Process process = ProcessStarter.Start("dotnet", options);
        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start TestFileGenerator");
        }

        // 1 time per sec
        TimeSpan period = TimeSpan.FromMilliseconds(1000);
        await metrics.CollectMetricsWhileProcessRunAsync(process, period);
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"TestFileGenerator exited with code {process.ExitCode}.");
        }

        stopwatch.Stop();
        ConsoleUtils.WriteErrorLine($"{args} run for {stopwatch.Elapsed}");
    }
}

public sealed class FileGeneratorBenchmarkConfig : ManualConfig
{
    public FileGeneratorBenchmarkConfig()
    {
        AddColumn(new CustomMeasureColumn(Measures.Memory));
        AddColumn(new CustomMeasureColumn(Measures.Disk));

        HideColumns("Allocated");
    }
}
