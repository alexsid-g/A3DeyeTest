#if WINDOWS
using System.Runtime.InteropServices;
#endif

namespace Benchmark.Services.SystemCollectors;

internal class WindowsMemoryCollector : ISystemCollector
{
    public Metrics Collect()
    {
#if WINDOWS
        return CollectWindowsMetrics();
#else
        return Metrics.Empty;
#endif
    }

#if WINDOWS
    private static long GetWindowsAvailableMemory()
    {
        var status = new MemoryStatusEx
        {
            Length = (uint)Marshal.SizeOf<MemoryStatusEx>()
        };

        if (!GlobalMemoryStatusEx(ref status))
        {
            throw new InvalidOperationException("GlobalMemoryStatusEx failed.");
        }

        return checked((long)status.AvailablePhysical);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);
#endif
}