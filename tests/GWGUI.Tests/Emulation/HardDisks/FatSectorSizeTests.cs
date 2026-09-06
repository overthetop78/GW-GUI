using System.Buffers.Binary;
using DiscUtils.Fat;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class FatSectorSizeTests
{
    public static IEnumerable<object[]> Profiles()
    {
        foreach (var sector in new[] { 512, 1024, 2048, 4096 })
        foreach (var (id, size) in new[] { ("fat12", 2L << 20), ("fat16", 32L << 20), ("fat32", 512L << 20) })
            yield return new object[] { id, size, sector };
    }

    [Theory]
    [MemberData(nameof(Profiles))]
    public void SectorGeometrySurvivesFileAllocation(string id, long size, int sector)
    {
        const long offset = 1 << 20;
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(offset + size, "raw", "mbr",
            [new(offset, size, id, "DATA", MbrType: id == "fat32" ? (byte)0x0c : (byte)0x06, SectorBytes: sector)]));
        using var volume = new SubStream(disk, Ownership.None, offset, size);
        var boot = new byte[sector]; volume.ReadExactly(boot);
        Assert.Equal(sector, BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(11)));
        Assert.Equal((uint)(offset / sector), BinaryPrimitives.ReadUInt32LittleEndian(boot.AsSpan(28)));
        Assert.Equal(0xaa55, BinaryPrimitives.ReadUInt16LittleEndian(boot.AsSpan(510)));
        if (id == "fat32")
        {
            var backup = new byte[sector]; volume.Position = 6L * sector; volume.ReadExactly(backup);
            Assert.Equal(boot, backup);
            volume.Position = sector; volume.ReadExactly(boot);
            volume.Position = 7L * sector; volume.ReadExactly(backup);
            Assert.Equal(boot, backup);
            Assert.Equal(0x41615252u, BinaryPrimitives.ReadUInt32LittleEndian(boot));
        }
        volume.Position = 0;
        using var fs = new FatFileSystem(volume);
        Assert.Equal("DATA", fs.VolumeLabel.Trim());
        var payload = Enumerable.Range(0, 100_000).Select(i => (byte)(i * 31)).ToArray();
        using (var file = fs.OpenFile("CHECK.BIN", FileMode.Create, FileAccess.ReadWrite))
        {
            file.Write(payload); file.Position = 0;
            var actual = new byte[payload.Length]; file.ReadExactly(actual); Assert.Equal(payload, actual);
        }
        Assert.Equal(payload.Length, fs.GetFileLength("CHECK.BIN"));
    }

    [Theory]
    [InlineData("fat16", 256)]
    [InlineData("fat16", 8192)]
    [InlineData("ext2", 4096)]
    public void UnsupportedSectorsAreRejectedBeforeWriting(string id, int sector)
    {
        using var disk = new MemoryStream();
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(disk,
            new(32L << 20, "raw", "none", [new(0, 32L << 20, id, "DATA", SectorBytes: sector)])));
        Assert.Equal(0, disk.Length);
    }
}
