using System.Diagnostics;

namespace Benchmark.Utils;

public static class ProcessStarter
{
    public static Process Start(string fileName, params string[] args)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (string item in args)
        {
            startInfo.ArgumentList.Add(item);
        }

        return new() { StartInfo = startInfo }; 
    }
}
