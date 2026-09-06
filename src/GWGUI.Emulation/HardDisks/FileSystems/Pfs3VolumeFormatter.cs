using Hst.Amiga.FileSystems.Pfs3;
using Hst.Amiga.RigidDiskBlocks;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>PFS3 with 512-byte data sectors and 1024-byte reserved blocks, including super-index mode.</summary>
public static class Pfs3VolumeFormatter
{
    public const long MaximumCapacity = Hst.Amiga.FileSystems.Pfs3.Constants.MAXDISKSIZE1K * 512;

    public static void Validate(long capacity, string label)
    {
        if (capacity < 8L << 20 || capacity > MaximumCapacity || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (label.Length is < 1 or > 31 || label.Any(c => c < 32 || c > 255 || c is ':' or '/'))
            throw new ArgumentException("PFS3 labels require 1 to 31 Latin-1 characters, excluding controls, ':' and '/'.", nameof(label));
    }

    public static void Format(Stream volume, string label)
    {
        Validate(volume.Length, label);
        if (!volume.CanRead || !volume.CanWrite || !volume.CanSeek)
            throw new ArgumentException("A readable writable seekable volume is required.");
        // The integrated asynchronous formatter may capture a caller's synchronization context.
        // Run it on the pool for the synchronous composition API, leaving ownership with the caller.
        Task.Run(() => Pfs3Formatter.FormatPartition(volume, DescribeVolume(volume.Length), label)).GetAwaiter().GetResult();
        volume.Flush();
    }

    internal static PartitionBlock DescribeVolume(long capacity) => new()
    {
        LowCyl = 0, HighCyl = checked((uint)(capacity / 512 - 1)),
        Surfaces = 1, BlocksPerTrack = 1, Sectors = 1, SizeBlock = 128,
        BlockSize = 512, FileSystemBlockSize = 512, PartitionSize = capacity,
        NumBuffer = 30, Mask = 0x7ffffffe, DosType = [0x50, 0x46, 0x53, 3], DriveName = "VOL"
    };
}
