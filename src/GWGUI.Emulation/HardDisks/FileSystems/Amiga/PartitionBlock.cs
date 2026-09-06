namespace Hst.Amiga.RigidDiskBlocks;

/// <summary>
/// Geometry passed to the integrated Amiga filesystem engines.
/// This deliberately contains only the fields used by the PFS3 implementation.
/// </summary>
public sealed class PartitionBlock
{
    public uint LowCyl { get; init; }
    public uint HighCyl { get; init; }
    public uint Surfaces { get; init; }
    public uint BlocksPerTrack { get; init; }
    public uint Sectors { get; init; } = 1;
    public uint SizeBlock { get; init; } = 128;
    public uint Reserved { get; init; } = 2;
    public uint NumBuffer { get; init; } = 30;
    public uint Mask { get; init; } = 0x7ffffffe;
    public byte[] DosType { get; init; } = [];
    public string DriveName { get; init; } = string.Empty;
    public long PartitionSize { get; init; }
    public uint BlockSize { get; init; } = 512;
    public uint FileSystemBlockSize { get; init; } = 512;
}
