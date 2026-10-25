namespace Benchmark.Services;

public class DiskMetricsCollector
{
    public string BasePath { get; }
    public string BasePathRoot { get; }

    public DiskMetricsCollector()
    {
        BasePath = Directory.GetCurrentDirectory();
        BasePathRoot = Path.GetPathRoot(BasePath)!;
    }

    public Metrics Collect()
    {
        DriveInfo drive = new DriveInfo(BasePathRoot);
        return new(drive.TotalSize, drive.AvailableFreeSpace);
    }
}
