using System.Buffers.Binary;
using DiscUtils;
using DiscUtils.Fat;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Partitioning;
using DiskImageBuilder = GWGUI.Emulation.HardDisks.DiskImageBuilder;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class GptSectorTests
{
    [Theory]
    [InlineData(512)] [InlineData(1024)] [InlineData(2048)] [InlineData(4096)]
    public void BothHeadersAndArraysReopenWithTheirActualSectorSize(int sectorBytes)
    {
        const long capacity = 32L << 20;
        using var image = new SparseMemoryStream();
        var tableId = sectorBytes == 512 ? "gpt" : $"gpt-{sectorBytes}";
        DiskImageBuilder.Write(image, new(capacity, "raw", tableId,
            [new(1L << 20, 24L << 20, "fat16", "DATA", SectorBytes: sectorBytes, PartitionName: "Documents")]));
        var geometry = Geometry.FromCapacity(capacity, sectorBytes);
        var table = new GuidPartitionTable(image, geometry);
        Assert.Single(table.Partitions);
        using (var volume = table.Partitions[0].Open())
        using (var fs = new FatFileSystem(volume))
        {
            Assert.Equal("DATA", fs.VolumeLabel.Trim());
            var payload = Enumerable.Range(0, 10000).Select(value => (byte)value).ToArray();
            using (var file = fs.OpenFile("DATA.BIN", FileMode.Create, FileAccess.Write)) file.Write(payload);
            using var read = fs.OpenFile("DATA.BIN", FileMode.Open, FileAccess.Read);
            var actual = new byte[payload.Length]; read.ReadExactly(actual); Assert.Equal(payload, actual);
        }
        byte[] Read(long offset, int count)
        {
            var result = new byte[count]; image.Position = offset; image.ReadExactly(result); return result;
        }
        var first = Read(sectorBytes, sectorBytes);
        var last = Read(capacity - sectorBytes, sectorBytes);
        Assert.All(first.Skip(92), value => Assert.Equal(0, value));
        Assert.All(last.Skip(92), value => Assert.Equal(0, value));
        Assert.Equal(first[56..72], last[56..72]);
        var entries = Read(sectorBytes * 2L, 16384);
        Assert.Equal(entries, Read(capacity - sectorBytes - 16384, 16384));
        Assert.All(entries.Skip(128), value => Assert.Equal(0, value));
        var mbr = Read(0, sectorBytes);
        Assert.Equal(0xee, mbr[450]);
        Assert.Equal(1U, BinaryPrimitives.ReadUInt32LittleEndian(mbr.AsSpan(454)));
        Assert.Equal((uint)(capacity / sectorBytes - 1), BinaryPrimitives.ReadUInt32LittleEndian(mbr.AsSpan(458)));
        Assert.All(mbr.Skip(512), value => Assert.Equal(0, value));
        // A read-only reader must recover from the independently valid backup without rewriting either header.
        image.Position = sectorBytes; image.WriteByte(0);
        using var readOnly = new GWGUI.Emulation.HardDisks.Containers.DifferencingImageStreams.ReadOnlyParent(image);
        var recovered = new GuidPartitionTable(readOnly, geometry);
        Assert.Single(recovered.Partitions);
        image.Position = sectorBytes; Assert.Equal(0, image.ReadByte());
    }

    [Fact]
    public void IncompatibleSectorCombinationsAndMalformedNamesAreRejectedBeforeOutput()
    {
        using var image = new MemoryStream();
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(image,
            new(32L << 20, "vhd", "gpt-4096", [new(1L << 20, 24L << 20, "fat16", SectorBytes: 4096)])));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(image,
            new(32L << 20, "raw", "gpt-4096", [new(1L << 20, 24L << 20, "fat16", SectorBytes: 512)])));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(image,
            new(32L << 20, "raw", "gpt", [new(1L << 20, 1L << 20, PartitionName: "A\0B")])));
        Assert.Equal(0, image.Length);
    }

    [Fact]
    public void EmptyTableClearsBothArraysAndAllReservedHeaderBytes()
    {
        using var image = new MemoryStream(Enumerable.Repeat((byte)0xa5, 128 * 1024).ToArray());
        GptPartitionWriter.Write(image, [], 4096);
        var bytes = image.ToArray();
        Assert.All(bytes[(2 * 4096)..(2 * 4096 + 16384)], value => Assert.Equal(0, value));
        Assert.All(bytes[(bytes.Length - 4096 - 16384)..(bytes.Length - 4096)], value => Assert.Equal(0, value));
        Assert.All(bytes[(4096 + 92)..8192], value => Assert.Equal(0, value));
        Assert.All(bytes[(bytes.Length - 4096 + 92)..], value => Assert.Equal(0, value));
        Assert.Empty(new GuidPartitionTable(image, Geometry.FromCapacity(image.Length, 4096)).Partitions);
    }

    [Fact]
    public void LegacyAutomaticLayoutReturnsTheDataPartitionAndClearsHeaderTails()
    {
        using var disk = new SparseMemoryStream(); disk.SetLength(1L << 30);
        var data = GptPartitionWriter.Create(disk, WellKnownPartitionType.WindowsFat);
        Assert.True(data.SectorCount > 0);
        foreach (var offset in new[] { 512L + 92, disk.Length - 512 + 92 })
        {
            disk.Position = offset;
            var reserved = new byte[420]; disk.ReadExactly(reserved);
            Assert.All(reserved, value => Assert.Equal(0, value));
        }
        var table = new GuidPartitionTable(disk, Geometry.FromCapacity(disk.Length));
        Assert.Contains(table.Partitions, partition => partition.FirstSector == data.FirstSector);
    }
}
