using System.Buffers.Binary;
using System.Text;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class BfsFormattingTests
{
    [Theory]
    [InlineData(8)] [InlineData(256)] [InlineData(512)]
    public void InodesRootExtentAndCompactionStateDescribeACleanEmptyVolume(int inodeCount)
    {
        using var volume = new SparseMemoryStream();
        volume.SetLength(1L << 20);
        BfsVolumeFormatter.Format(volume, "SYSTEM", inodeCount, "BOOT");
        volume.Position = 0;
        var super = new byte[512]; volume.ReadExactly(super);
        uint Read32(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset));
        Assert.Equal(0x1badfaceU, Read32(super, 0));
        Assert.Equal(volume.Length - 1, Read32(super, 8));
        for (var offset = 12; offset <= 24; offset += 4) Assert.Equal(uint.MaxValue, Read32(super, offset));
        Assert.Equal("BOOT\0\0", Encoding.ASCII.GetString(super, 28, 6));
        Assert.Equal("SYSTEM", Encoding.ASCII.GetString(super, 34, 6));
        var dataStart = Read32(super, 4);
        Assert.Equal((uint)inodeCount, (dataStart - 512) / 64);
        var inodes = new byte[dataStart - 512]; volume.ReadExactly(inodes);
        Assert.Equal(2, BinaryPrimitives.ReadUInt16LittleEndian(inodes));
        Assert.All(inodes.Skip(64), value => Assert.Equal(0, value));
        var rootStart = Read32(inodes, 4);
        var rootEnd = Read32(inodes, 8);
        Assert.Equal(dataStart, rootStart * 512);
        Assert.Equal(32U, Read32(inodes, 12) + 1 - rootStart * 512);
        Assert.True((rootEnd - rootStart + 1) * 512 >= inodeCount * 16);
        Assert.Equal(2U, Read32(inodes, 16));
        Assert.Equal(0x41edU, Read32(inodes, 20));
        Assert.Equal(2U, Read32(inodes, 32));
        volume.Position = dataStart;
        var entries = new byte[(rootEnd - rootStart + 1) * 512]; volume.ReadExactly(entries);
        Assert.Equal(2, BinaryPrimitives.ReadUInt16LittleEndian(entries));
        Assert.Equal(".", Encoding.ASCII.GetString(entries, 2, 14).TrimEnd('\0'));
        Assert.Equal(2, BinaryPrimitives.ReadUInt16LittleEndian(entries.AsSpan(16)));
        Assert.Equal("..", Encoding.ASCII.GetString(entries, 18, 14).TrimEnd('\0'));
        Assert.All(entries.Skip(32), value => Assert.Equal(0, value));
        Assert.True(rootEnd < volume.Length / 512);
    }

    [Fact]
    public void FourGiBBoundaryAndComposedFormatterWorkWithoutARealImage()
    {
        using var image = new SparseMemoryStream();
        DiskImageBuilder.Write(image, new(BfsVolumeFormatter.MaximumCapacity, "raw", "none",
            [new(0, BfsVolumeFormatter.MaximumCapacity, "bfs", "BOOT")]));
        image.Position = 8;
        Span<byte> lastByte = stackalloc byte[4]; image.ReadExactly(lastByte);
        Assert.Equal(uint.MaxValue, BinaryPrimitives.ReadUInt32LittleEndian(lastByte));
        Assert.True(image.Extents.Sum(value => value.Length) < 64 * 1024);
        Assert.Throws<ArgumentOutOfRangeException>(() => BfsVolumeFormatter.Validate(image.Length + 512, "BOOT"));
    }

    [Fact]
    public void InvalidNamesSizesAndInodeCountsDoNotChangeTheVolume()
    {
        using var volume = new MemoryStream(new byte[64 * 1024]);
        volume.WriteByte(0xa5);
        Assert.Throws<ArgumentException>(() => BfsVolumeFormatter.Format(volume, "TOO-LONG"));
        Assert.Throws<ArgumentException>(() => BfsVolumeFormatter.Format(volume, "Données"));
        Assert.Throws<ArgumentOutOfRangeException>(() => BfsVolumeFormatter.Format(volume, "BOOT", 9));
        Assert.Throws<ArgumentOutOfRangeException>(() => BfsVolumeFormatter.Validate(512, "BOOT"));
        volume.Position = 0; Assert.Equal(0xa5, volume.ReadByte());
    }
}
