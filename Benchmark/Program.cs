using Benchmark.Data;
using Benchmark.Services;
using Benchmark.Utils;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Reflection;

IHostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
using IDisposable environmentSession = BenchmarksEnvironment.InitializeSession();
using IDisposable metricsSession = MeasureStorage.InitializeSession();

ConsoleUtils.WriteInfoLine($"Solution directory: {BenchmarksEnvironment.GetSolutionDirectory()}");
ConsoleUtils.WriteInfoLine($"Data directory: {BenchmarksEnvironment.GetDataDirectory()}");

Dictionary<string, string> switchMappings = new()
{
    ["-t"] = nameof(BenchmarkOptions.TestToRun)
};
builder.Configuration.AddCommandLine(args, switchMappings);

Console.WriteLine(Assembly.GetExecutingAssembly().FullName);
Console.WriteLine();

List<Type> benchmarkTypes = Assembly.GetExecutingAssembly().GetTypes()
    .Where(t => t.GetMethods().Any(p => p.GetCustomAttribute<BenchmarkAttribute>() != null))
    .ToList();

BenchmarkOptions? options = builder.Configuration.Get<BenchmarkOptions>();
if (options is null || string.IsNullOrEmpty(options.TestToRun))
{
    Console.WriteLine("---------------------------------------");
    Console.WriteLine("Start Benchmarking, select test to run:");

    int idx = 0;
    benchmarkTypes.ForEach(x =>
    {
        Console.WriteLine($"{idx++} - {x}");
    });

    Console.Write("Your input is :> ");

    string? input = Console.ReadLine();
    if (int.TryParse(input, out var x) && x >=0 && x < benchmarkTypes.Count)
    {
        BenchmarkRunner.Run(benchmarkTypes[x]);
    }
    else
    {
        ConsoleUtils.WriteErrorLine("Bad test number, exit...");
    }
}
else
{
    Console.WriteLine($"Your input is :>  {options.TestToRun}");

    Type? benchmarkToRun = benchmarkTypes.FirstOrDefault(
        x => x.Name.Equals(options.TestToRun, StringComparison.InvariantCultureIgnoreCase));
    
    if (benchmarkToRun is not null)
    {
        BenchmarkRunner.Run(benchmarkToRun);
    }
    else
    {
        ConsoleUtils.WriteErrorLine("Bad test number, exit...");
    }
}
