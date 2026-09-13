using DiscUtils;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using DiskImageBuilder = GWGUI.Emulation.HardDisks.DiskImageBuilder;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class PartitionTypeTests
{
    [Theory]
    [InlineData("fat16", 0x06)] [InlineData("ext2", 0x83)] [InlineData("hfsplus", 0xaf)]
    public void MbrTypeMatchesTheCreatedFilesystem(string fs, byte type)
    {
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(64L << 20, "raw", "mbr", [new(1L << 20, 32L << 20, fs, "DATA")]));
        var table = new BiosPartitionTable(disk, Geometry.FromCapacity(disk.Length));
        Assert.Equal(type, table[0].BiosType);
    }

    [Theory]
    [InlineData("fat16", "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7")]
    [InlineData("ext2", "0FC63DAF-8483-4772-8E79-3D69D8477DE4")]
    [InlineData("hfsplus", "48465300-0000-11AA-AA11-00306543ECAC")]
    public void GptTypeMatchesTheCreatedFilesystem(string fs, string type)
    {
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(64L << 20, "raw", "gpt", [new(1L << 20, 32L << 20, fs, "DATA")]));
        var table = new GuidPartitionTable(disk, Geometry.FromCapacity(disk.Length));
        Assert.Equal(new Guid(type), table[0].GuidType);
    }

    [Theory]
    [InlineData("mbr")] [InlineData("gpt")]
    public void UnknownPairingNeedsAnExplicitTypeBeforeAnyWrite(string table)
    {
        using var disk = new MemoryStream();
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(disk,
            new(64L << 20, "raw", table, [new(1L << 20, 2L << 20, "pascal", "DATA")])));
        Assert.Equal(0, disk.Length);
    }
}
