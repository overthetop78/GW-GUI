using System.Buffers.Binary;
using System.Text;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class PartitionNameTests
{
    [Fact]
    public void GptNameDoesNotRestrictTheFilesystemLabel()
    {
        using var disk = new SparseMemoryStream();
        var label = new string('A', 80);
        DiskImageBuilder.Write(disk, new(32L << 20, "raw", "gpt",
            [new(1L << 20, 16L << 20, "hfsplus", label, PartitionName: "Documents")]));
        var entry = new byte[128]; disk.Position = 1024; disk.ReadExactly(entry);
        Assert.Equal("Documents", Encoding.Unicode.GetString(entry, 56, 72).TrimEnd('\0'));
        using var view = new SubStream(disk, Ownership.None, 1L << 20, 16L << 20);
        using var fs = new DiscUtils.HfsPlus.HfsPlusFileSystem(view); Assert.Equal(label, fs.VolumeLabel);
    }

    [Fact]
    public void ApmCanNameAFileSystemThatHasNoLabelField()
    {
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(16L << 20, "raw", "apm",
            [new(1L << 20, 8L << 20, "minix2", "", PartitionType: "Linux_Minix", PartitionName: "Data")]));
        var entry = new byte[512]; disk.Position = 1024; disk.ReadExactly(entry);
        Assert.Equal("Data", Encoding.ASCII.GetString(entry, 16, 32).TrimEnd('\0'));
        disk.Position = (1L << 20) + 1024 + 16; var magic = new byte[2]; disk.ReadExactly(magic);
        Assert.Equal(0x2478, BinaryPrimitives.ReadUInt16LittleEndian(magic));
    }

    [Fact]
    public void RdbDeviceNameIsIndependentOfTheVolumeLabel()
    {
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(32L << 20, "raw", "rdb",
            [new(1L << 20, 16L << 20, "ffs", "Documents", PartitionName: "DH0")]));
        var part = new byte[512]; disk.Position = 512; disk.ReadExactly(part);
        Assert.Equal("DH0", Encoding.Latin1.GetString(part, 37, part[36]));
        var root = new byte[512]; disk.Position = (1L << 20) + (16L << 20) / 2; disk.ReadExactly(root);
        Assert.Equal("Documents", Encoding.Latin1.GetString(root, 433, root[432]));
    }
}
