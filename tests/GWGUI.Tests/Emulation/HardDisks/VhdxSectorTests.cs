using DiscUtils.Fat;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class VhdxSectorTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FixedAndDynamic4KnContainersReopenTheirGptAndFatVolume(bool fixedSize)
    {
        using var image = new SparseMemoryStream();
        DiskImageBuilder.Write(image, new(32L << 20, "vhdx-4kn", "gpt-4096",
            [new(1L << 20, 24L << 20, "fat16", "DATA", SectorBytes: 4096)], fixedSize));
        using var layer = new DiscUtils.Vhdx.DiskImageFile(image, Ownership.None);
        Assert.Equal(4096, layer.LogicalSectorSize);
        using var logical = layer.OpenContent(null!, Ownership.None);
        var table = new GuidPartitionTable(logical, layer.Geometry);
        using var volume = Assert.Single(table.Partitions).Open();
        using var fs = new FatFileSystem(volume);
        Assert.Equal("DATA", fs.VolumeLabel.Trim());
        using (var file = fs.OpenFile("TEST.BIN", FileMode.Create, FileAccess.Write)) file.Write(new byte[] { 1, 2, 3 });
        using var read = fs.OpenFile("TEST.BIN", FileMode.Open, FileAccess.Read);
        Assert.Equal(new byte[] { 1, 2, 3 }, read.ReadExactly(3));
    }

    [Fact]
    public void FourKnPayloadCrossesTheBitmapChunkBoundaryInMemory()
    {
        const long boundary = 32L << 30;
        using var image = new SparseMemoryStream();
        VhdxImageWriter.Write(image, boundary + 4096, initialize: content =>
        {
            content.Position = boundary - 1; content.WriteByte(23);
            content.WriteByte(24); content.Position = boundary + 4095; content.WriteByte(25);
        }, logicalSectorBytes: 4096);
        using var layer = new DiscUtils.Vhdx.DiskImageFile(image, Ownership.None);
        using var logical = layer.OpenContent(null!, Ownership.None);
        logical.Position = boundary - 1; Assert.Equal(23, logical.ReadByte()); Assert.Equal(24, logical.ReadByte());
        logical.Position = boundary + 4095; Assert.Equal(25, logical.ReadByte());
    }

    [Fact]
    public void InvalidLogicalSectorsAndMixedDirectVolumesAreRejectedBeforeOutput()
    {
        using var image = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => VhdxImageWriter.Write(image, 65536, logicalSectorBytes: 1024));
        Assert.Throws<ArgumentOutOfRangeException>(() => VhdxImageWriter.Write(image, 66048, logicalSectorBytes: 4096));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(image,
            new(32L << 20, "vhdx-4kn", "none", [new(0, 32L << 20, "fat16", SectorBytes: 512)])));
        Assert.Throws<ArgumentException>(() => DiskImageBuilder.Write(image,
            new(32L << 20, "vhdx-4kn", "gpt", [new(1L << 20, 24L << 20, "fat16", SectorBytes: 4096)])));
        Assert.Equal(0, image.Length);
    }

    [Fact]
    public void DifferentialChildRetainsTheParentsFourKnMetadataAndContent()
    {
        using var parent = new MemoryStream();
        VhdxImageWriter.Write(parent, 8L << 20, initialize: stream => stream.WriteByte(7), logicalSectorBytes: 4096);
        var parentBytes = parent.ToArray();
        using var child = new MemoryStream();
        VhdxDifferencingImageWriter.Write(child, parent, Path.GetFullPath("parent.vhdx"), "parent.vhdx",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), stream =>
            {
                Assert.Equal(7, stream.ReadByte()); stream.Position = 4096; stream.WriteByte(9);
            });
        Assert.Equal(parentBytes, parent.ToArray());
        using var parentInput = new MemoryStream(parentBytes, writable: false);
        using var childInput = new MemoryStream(child.ToArray(), writable: false);
        using var parentLayer = new DiscUtils.Vhdx.DiskImageFile(parentInput, Ownership.None);
        using var childLayer = new DiscUtils.Vhdx.DiskImageFile(childInput, Ownership.None);
        Assert.Equal(4096, childLayer.LogicalSectorSize);
        Assert.Equal(parentLayer.UniqueId, childLayer.ParentUniqueId);
        using var inherited = parentLayer.OpenContent(null!, Ownership.None);
        using var logical = childLayer.OpenContent(inherited, Ownership.None);
        Assert.Equal(7, logical.ReadByte()); logical.Position = 4096; Assert.Equal(9, logical.ReadByte());
    }
}
