#if MACOS
using System.Runtime.InteropServices;
#endif

namespace Benchmark.Services.SystemCollectors;

internal class MacMemoryCollector : ISystemCollector
{
    private readonly uint _machHost = 0;
    private readonly uint _pageSize = 0;
    private readonly long _totalMemory = 0;

    public MacMemoryCollector()
    {
#if MACOS
        _machHost = MachHostSelf();
    
        int result = HostPageSize(_machHost, out uint pageSize);
        if (result != 0)
        {
            throw new InvalidOperationException($"HostPageSize failed with code {result}");
        }

        _pageSize = pageSize;

        _totalMemory = GetTotalMemory();
        if (_totalMemory <= 0)
        {
            throw new InvalidOperationException("GetTotalMemory failed");
        }
#endif
    }

    public Metrics Collect()
    {
#if MACOS
        const int HostVmInfo64 = 4;

        uint count = (uint)(Marshal.SizeOf<VmStatistics64>() / sizeof(int));
        int result = HostStatistics64(
            _machHost, HostVmInfo64, out VmStatistics64 stats, ref count);

        if (result != 0)
        {
            throw new InvalidOperationException($"HostStatistics64 failed with code {result}");
        }

        long availablePages = (long)stats.FreeCount + stats.InactiveCount + stats.PurgeableCount;
        return new Metrics(_totalMemory, availablePages * _pageSize);
#else
        return Metrics.Empty;
#endif
    }

#if MACOS
    private static long GetTotalMemory()
    {
        try
        {
            long memSize = 0;
            nuint len = sizeof(long);

            int result = SysCtlByName("hw.memsize", out memSize, ref len, IntPtr.Zero, 0);
            return result == 0 ? memSize : -1;
        }
        catch
        {
            return -1;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct VmStatistics64
    {
        public uint FreeCount;
        public uint ActiveCount;
        public uint InactiveCount;
        public uint WireCount;
        public ulong ZeroFillCount;
        public ulong Reactivations;
        public ulong PageIns;
        public ulong PageOuts;
        public ulong Faults;
        public ulong CowFaults;
        public ulong Lookups;
        public ulong Hits;
        public ulong Purges;
        public uint PurgeableCount;
        public uint SpeculativeCount;
        public ulong Decompressions;
        public ulong Compressions;
        public ulong SwapIns;
        public ulong SwapOuts;
        public uint CompressorPageCount;
        public uint ThrottledCount;
        public uint ExternalPageCount;
        public uint InternalPageCount;
        public ulong TotalUncompressedPagesInCompressor;
    }

    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "mach_host_self")]
    private static extern uint MachHostSelf();

    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "host_page_size")]
    private static extern int HostPageSize(uint host, out uint pageSize);

    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "sysctlbyname", CharSet = CharSet.Ansi)]
    private static extern int SysCtlByName(
        string name, out long oldValue, ref nuint oldLen, IntPtr newValue, nuint newLen);

    [DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "host_statistics64")]
    private static extern int HostStatistics64(
        uint host, int flavor, out VmStatistics64 info, ref uint count);
#endif
}
