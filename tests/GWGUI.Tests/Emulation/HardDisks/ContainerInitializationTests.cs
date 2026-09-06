using VirtualDisk = DiscUtils.VirtualDisk;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ContainerInitializationTests
{
    [Theory]
    [InlineData(DiskContainerKind.Raw)]
    [InlineData(DiskContainerKind.Gzip)]
    [InlineData(DiskContainerKind.Qed)]
    [InlineData(DiskContainerKind.TwoImg)]
    [InlineData(DiskContainerKind.Vhd)]
    [InlineData(DiskContainerKind.Vhdx)]
    [InlineData(DiskContainerKind.Vdi)]
    [InlineData(DiskContainerKind.Vmdk)]
    public void FailedOrResizedInitializationLeavesDestinationEmpty(DiskContainerKind kind)
    {
        using var image = new MemoryStream();
        Assert.Throws<IOException>(() => DiskContainerWriter.Write(image, 4L << 20, kind, content =>
        {
            content.WriteByte(42);
            throw new IOException("Simulated formatter failure");
        }));
        Assert.Equal(0, image.Length);
        Assert.Throws<InvalidOperationException>(() => DiskContainerWriter.Write(image, 4L << 20, kind,
            content => content.SetLength(8L << 20)));
        Assert.Equal(0, image.Length);
    }

    [Theory]
    [InlineData(DiskContainerKind.Vhd, false)]
    [InlineData(DiskContainerKind.Vhd, true)]
    [InlineData(DiskContainerKind.Vhdx, false)]
    [InlineData(DiskContainerKind.Vhdx, true)]
    [InlineData(DiskContainerKind.Vdi, false)]
    [InlineData(DiskContainerKind.Vdi, true)]
    [InlineData(DiskContainerKind.Vmdk, false)]
    public void StagedContentPreservesGapsBoundariesAndLastSector(DiskContainerKind kind, bool fixedSize)
    {
        const long capacity = 8L << 20;
        using var image = new SparseMemoryStream();
        long[] offsets = [0, (2L << 20) - 1, 2L << 20, capacity - 1];
        DiskContainerWriter.Write(image, capacity, kind, content =>
        {
            for (var i = 0; i < offsets.Length; i++)
            {
                content.Position = offsets[i];
                content.WriteByte((byte)(31 + i));
            }
        }, fixedSize);
        image.Position = 0;
        using VirtualDisk disk = kind switch
        {
            DiskContainerKind.Vhd => new DiscUtils.Vhd.Disk(image, Ownership.None),
            DiskContainerKind.Vhdx => new DiscUtils.Vhdx.Disk(image, Ownership.None),
            DiskContainerKind.Vdi => new DiscUtils.Vdi.Disk(image, Ownership.None),
            _ => new DiscUtils.Vmdk.Disk(image, Ownership.None)
        };
        Assert.Equal(capacity, disk.Capacity);
        for (var i = 0; i < offsets.Length; i++)
        {
            disk.Content.Position = offsets[i];
            Assert.Equal(31 + i, disk.Content.ReadByte());
        }
        disk.Content.Position = 4L << 20;
        var gap = new byte[512];
        disk.Content.ReadExactly(gap);
        Assert.All(gap, value => Assert.Equal(0, value));
    }

    [Theory]
    [InlineData("vhd", VhdImageWriter.MaximumCapacity)]
    [InlineData("vhdx", VhdxImageWriter.MaximumCapacity)]
    public void CapacityLimitsAreValidatedBeforeConstructingAnyImage(string id, long maximum)
    {
        DiskImageBuilder.Validate(new(maximum, id, "none", []));
        using var destination = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DiskImageBuilder.Write(destination, new(maximum + 512, id, "none", [])));
        Assert.Equal(0, destination.Length);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DiskContainerWriter.Write(destination, maximum + 512, Enum.Parse<DiskContainerKind>(id, true)));
        Assert.Equal(0, destination.Length);
    }
}
