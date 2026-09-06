using System.Buffers.Binary;
using DiscUtils;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;
using DiskImageBuilder = GWGUI.Emulation.HardDisks.DiskImageBuilder;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ComposedDiskTests
{
    [Theory]
    [InlineData("ahdi", "fat16-adapted")]
    [InlineData("rdb", "ffs")]
    public void LegacyPartitionTablesComposeMultipleVolumes(string tableId, string fs)
    {
        using var image = new SparseMemoryStream();
        DiskImageBuilder.Write(image, new(64L << 20, "raw", tableId,
            [new(1L << 20, 16L << 20, fs, "FIRST"), new(32L << 20, 16L << 20, fs, "SECOND")]));
        var root = new byte[512]; image.Position = 0; image.ReadExactly(root);
        if (tableId == "ahdi")
        {
            Assert.Equal(2048u, BinaryPrimitives.ReadUInt32BigEndian(root.AsSpan(0x1ca)));
            Assert.Equal(65536u, BinaryPrimitives.ReadUInt32BigEndian(root.AsSpan(0x1ca + 12)));
        }
        else
        {
            var part = new byte[512]; image.Position = 512; image.ReadExactly(part);
            Assert.Equal(2u, BinaryPrimitives.ReadUInt32BigEndian(part.AsSpan(16)));
            image.ReadExactly(part); Assert.Equal(uint.MaxValue, BinaryPrimitives.ReadUInt32BigEndian(part.AsSpan(16)));
            Assert.Equal(2048u, BinaryPrimitives.ReadUInt32BigEndian(part.AsSpan(164)));
        }
    }

    [Theory]
    [InlineData("mbr")]
    [InlineData("gpt")]
    public void TwoVolumesCanBeReopenedAndWrittenIndependently(string tableId)
    {
        using var image = new SparseMemoryStream();
        DiskImageBuilder.Write(image, new(128L << 20, "raw", tableId,
            [new(1L << 20, 32L << 20, "fat", "DATA", 0x06), new(40L << 20, 64L << 20, "ntfs", "SECOND")]));
        PartitionTable table = tableId == "mbr"
            ? new BiosPartitionTable(image, Geometry.FromCapacity(image.Length))
            : new GuidPartitionTable(image, Geometry.FromCapacity(image.Length));
        Assert.Equal(2, table.Count);
        using var first = table[0].Open(); using var second = table[1].Open();
        using var fat = new DiscUtils.Fat.FatFileSystem(first);
        using var ntfs = new DiscUtils.Ntfs.NtfsFileSystem(second);
        using (var file = fat.OpenFile("first.txt", FileMode.Create, FileAccess.Write)) file.WriteByte(42);
        using (var file = ntfs.OpenFile("second.txt", FileMode.Create, FileAccess.Write)) file.WriteByte(43);
        using var one = fat.OpenFile("first.txt", FileMode.Open); Assert.Equal(42, one.ReadByte());
        using var two = ntfs.OpenFile("second.txt", FileMode.Open); Assert.Equal(43, two.ReadByte());
        Assert.False(fat.FileExists("second.txt")); Assert.False(ntfs.FileExists("first.txt"));
    }

    [Fact]
    public void InvalidLayoutsAreRejectedBeforeWriting()
    {
        DiskImagePlan[] invalid = [
            new(64L << 20, "raw", "mbr", [new(512, 32L << 20), new(1024, 32L << 20)]),
            new(64L << 20, "raw", "gpt", [new(512, 32L << 20)]),
            new(64L << 20, "raw", "none", [new(0, 32L << 20)]),
            new(64L << 20, "raw", "mbr", [new(1L << 20, 64L << 20)]),
            new(64L << 20, "raw", "mbr", [new(1L << 20, 32L << 20, "absent")])];
        foreach (var plan in invalid)
        {
            using var image = new MemoryStream();
            Assert.ThrowsAny<Exception>(() => DiskImageBuilder.Write(image, plan));
            Assert.Equal(0, image.Length);
        }
    }

    [Fact]
    public void ExternalFormatterCanBeRegisteredWithoutAddingAnEnumValue()
    {
        var registry = DiskFormatRegistry.CreateDefault();
        registry.Register(new DiskFormatRegistry.FileSystem("test-format", _ => { }, (stream, _) => stream.Write("TEST"u8)));
        using var image = new MemoryStream();
        DiskImageBuilder.Write(image, new(4096, "twoimg", "none", [new(0, 4096, "test-format")]), registry);
        var bytes = image.ToArray();
        Assert.Equal("2IMG"u8.ToArray(), bytes[..4]);
        Assert.Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(12)));
        Assert.Equal(8u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(20)));
        Assert.Equal(4096u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(28)));
        Assert.Equal("TEST"u8.ToArray(), bytes[64..68]);
        Assert.Equal(4160, bytes.Length);
        Assert.DoesNotContain("test-format", DiskFormatRegistry.CreateDefault().FileSystemIds);
    }

    [Fact]
    public void QedResolvesDataAcrossTwoSecondLevelTables()
    {
        const long boundary = 512L << 20;
        using var image = new MemoryStream();
        QedImageWriter.Write(image, boundary + 512, content =>
        {
            content.Position = boundary - 1; content.WriteByte(42);
            content.Position = boundary; content.WriteByte(43);
            content.Position = boundary + 511; content.WriteByte(44);
        });
        var bytes = image.ToArray();
        Assert.Equal(0x00444551u, BinaryPrimitives.ReadUInt32LittleEndian(bytes));
        Assert.Equal((ulong)(boundary + 512), BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(48)));
        Assert.Equal(42, ReadQed(bytes, boundary - 1)); Assert.Equal(43, ReadQed(bytes, boundary));
        Assert.Equal(44, ReadQed(bytes, boundary + 511)); Assert.Equal(0, ReadQed(bytes, 0));
        Assert.True(image.Length < 1024 * 1024);
    }
    private static byte ReadQed(byte[] bytes, long offset)
    {
        var cluster = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4));
        var entries = cluster / 8;
        var l1 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(40));
        var l2 = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(checked((int)(l1 + (ulong)(offset / cluster / entries) * 8))));
        if (l2 == 0) return 0;
        var data = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(checked((int)(l2 + (ulong)(offset / cluster % entries) * 8))));
        return data == 0 ? (byte)0 : bytes[checked((int)(data + (ulong)(offset % cluster)))];
    }
}
