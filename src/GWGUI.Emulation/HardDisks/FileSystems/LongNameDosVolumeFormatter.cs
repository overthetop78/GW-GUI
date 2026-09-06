using System.Buffers.Binary;
using Hst.Amiga.FileSystems.FastFileSystem;
using Hst.Amiga.RigidDiskBlocks;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>DOS6/DOS7 volumes with long names and 512-byte filesystem blocks. No executable boot code.</summary>
public static class LongNameDosVolumeFormatter
{
    public static void Validate(long capacity, string label)
        => AmigaDosVolumeFormatter.Validate(capacity, label, 0);

    public static void Format(Stream volume, string label, bool fastFileSystem)
    {
        Validate(volume.Length, label);
        if (!volume.CanRead || !volume.CanSeek || !volume.CanWrite)
            throw new ArgumentException("A readable writable seekable volume is required.");
        Task.Run(() => FastFileSystemFormatter.FormatPartition(volume, DescribeVolume(volume.Length, fastFileSystem), label))
            .GetAwaiter().GetResult();
        var boot = new byte[1024];
        volume.Position = 0;
        volume.ReadExactly(boot);
        BinaryPrimitives.WriteUInt32BigEndian(boot.AsSpan(8), checked((uint)(volume.Length / 1024)));
        BinaryPrimitives.WriteUInt32BigEndian(boot.AsSpan(4), 0);
        uint sum = 0;
        for (var offset = 0; offset < boot.Length; offset += 4)
        {
            var value = BinaryPrimitives.ReadUInt32BigEndian(boot.AsSpan(offset));
            var next = unchecked(sum + value);
            if (next < sum) next++;
            sum = next;
        }
        BinaryPrimitives.WriteUInt32BigEndian(boot.AsSpan(4), ~sum);
        volume.Position = 0;
        volume.Write(boot);
        volume.Flush();
    }

    internal static PartitionBlock DescribeVolume(long capacity, bool fastFileSystem) => new()
    {
        LowCyl = 0,
        HighCyl = checked((uint)(capacity / 512 - 1)),
        Surfaces = 1,
        BlocksPerTrack = 1,
        Sectors = 1,
        SizeBlock = 128,
        BlockSize = 512,
        FileSystemBlockSize = 512,
        PartitionSize = capacity,
        Reserved = 2,
        DosType = [0x44, 0x4f, 0x53, fastFileSystem ? (byte)7 : (byte)6]
    };
}
