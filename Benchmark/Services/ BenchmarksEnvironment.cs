using Benchmarks.Data;

namespace Benchmark.Services;

public static class BenchmarksEnvironment
{
    public static IDisposable InitializeSession()
    {
        string solutionDirectory = Directory.GetCurrentDirectory();
        Environment.SetEnvironmentVariable(Consts.SolutionDirectoryEnv, solutionDirectory);

        string dataDirectory = Path.Combine(solutionDirectory, Consts.DataDirectoryEnv);
        Directory.CreateDirectory(dataDirectory);
        Environment.SetEnvironmentVariable(Consts.DataDirectoryEnv, dataDirectory);

        return new AppEnvironmentSession(new() {
            {Consts.DataDirectoryEnv, dataDirectory},
            {Consts.SolutionDirectoryEnv, solutionDirectory},
        });
    }

    public static string? GetSolutionDirectory() =>
        Environment.GetEnvironmentVariable(Consts.SolutionDirectoryEnv);

    public static string? GetDataDirectory() =>
        Environment.GetEnvironmentVariable(Consts.DataDirectoryEnv);

    private sealed class AppEnvironmentSession : IDisposable
    {
        private Dictionary<string, string> _environment;
        private bool _disposed = false;

        public AppEnvironmentSession(Dictionary<string, string> environment)
        {
            _environment = environment;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (var item in _environment)
            {
                if (Environment.GetEnvironmentVariable(item.Key) == item.Value)
                {
                    Environment.SetEnvironmentVariable(item.Key, null);
                }   
            }
        }
    }
}
