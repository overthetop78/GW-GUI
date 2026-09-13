using System.Buffers.Binary;
using DiscUtils;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using DiskImageBuilder = GWGUI.Emulation.HardDisks.DiskImageBuilder;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ExtendedMbrTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(0, 6)]
    [InlineData(3, 6)]
    [InlineData(4, 0)]
    public void IndependentReaderOpensAllVolumes(int primaryCount, int logicalCount)
    {
        var volumes = Enumerable.Range(0, primaryCount + logicalCount)
            .Select(i => new DiskVolumePlan((1L + i * 4) << 20, 2L << 20, "fat12", $"VOL{i}", 1,
                Active: i == 0 && primaryCount > 0, MbrLogical: i >= primaryCount)).ToArray();
        using var image = new SparseMemoryStream();
        DiskImageBuilder.Write(image, new(64L << 20, "raw", "mbr", volumes.Reverse().ToArray()));
        var table = new BiosPartitionTable(image, Geometry.FromCapacity(image.Length));
        Assert.Equal(volumes.Length, table.Count);
        foreach (var volume in volumes)
        {
            var partition = Assert.Single(table.Partitions, p => p.FirstSector == volume.OffsetBytes / 512);
            Assert.Equal(volume.LengthBytes / 512, partition.SectorCount);
            using var content = partition.Open();
            using var fs = new DiscUtils.Fat.FatFileSystem(content);
            Assert.Equal(volume.Label, fs.VolumeLabel.Trim());
            using (var file = fs.OpenFile("CHECK.TXT", FileMode.Create, FileAccess.Write)) file.WriteByte((byte)(volume.OffsetBytes >> 20));
            using var read = fs.OpenFile("CHECK.TXT", FileMode.Open);
            Assert.Equal((byte)(volume.OffsetBytes >> 20), read.ReadByte());
        }
        var logical = volumes.Where(v => v.MbrLogical).ToArray();
        for (var i = 0; i < logical.Length; i++)
        {
            var bytes = new byte[512]; image.Position = logical[i].OffsetBytes - 512; image.ReadExactly(bytes);
            Assert.Equal(0xaa55, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(510)));
            Assert.Equal(1u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(454)));
            if (i + 1 == logical.Length) Assert.All(bytes[462..510], b => Assert.Equal(0, b));
            else Assert.Equal((uint)((logical[i + 1].OffsetBytes - logical[0].OffsetBytes) / 512),
                BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(470)));
        }
    }

    [Fact]
    public void InvalidExtendedLayoutsDoNotWrite()
    {
        DiskVolumePlan Logical(long start, long length = 1024) => new(start, length, MbrLogical: true);
        DiskImagePlan[] plans = [
            new(16384, "raw", "mbr", [Logical(512)]),
            new(16384, "raw", "mbr", [Logical(1024), Logical(2048)]),
            new(16384, "raw", "mbr", [Logical(1024), new(3072, 512), Logical(4096)]),
            new(16384, "raw", "mbr", [new(512, 512), Logical(1024)]),
            new(16384, "raw", "mbr", [Logical(1024) with { Active = true }]),
            new(16384, "raw", "mbr", [new(512,512),new(1024,512),new(1536,512),new(2048,512),Logical(4096)]),
            new(16384, "raw", "none", [Logical(0, 16384)]),
            new(1L << 20, "raw", "gpt", [Logical(32768)])];
        foreach (var plan in plans)
        {
            using var image = new MemoryStream(); image.WriteByte(42);
            Assert.ThrowsAny<ArgumentException>(() => DiskImageBuilder.Write(image, plan));
            Assert.Equal(new byte[] { 42 }, image.ToArray());
        }
    }
}
