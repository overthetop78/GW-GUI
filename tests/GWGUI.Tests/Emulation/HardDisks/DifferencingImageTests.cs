using DiscUtils;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class DifferencingImageTests
{
    private static readonly DateTime Modified = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void ChildOverridesSectorsAndKeepsUnmodifiedParentData(bool vhdx, bool fixedParent)
    {
        const long capacity = 32L << 20;
        using var parent = new MemoryStream();
        void Seed(Stream content)
        {
            content.Position = 0; content.WriteByte(31);
            content.Position = (2 << 20) - 1; content.WriteByte(41); content.WriteByte(42);
            content.Position = capacity - 1; content.WriteByte(51);
        }
        if (vhdx) VhdxImageWriter.Write(parent, capacity, fixedParent, Seed);
        else VhdImageWriter.Write(parent, capacity, fixedParent, Seed);
        var original = parent.ToArray(); parent.Position = 17;
        using var child = new MemoryStream();
        Write(vhdx, child, parent, content =>
        {
            content.Position = (2 << 20) - 1; Assert.Equal(41, content.ReadByte()); Assert.Equal(42, content.ReadByte());
            content.Position = (2 << 20) - 1; content.WriteByte(0); content.WriteByte(99);
        });
        Assert.Equal(17, parent.Position); Assert.Equal(original, parent.ToArray());
        using var parentBytes = new MemoryStream(original, writable: false);
        using var childBytes = new MemoryStream(child.ToArray(), writable: false);
        using var parentLayer = Open(vhdx, parentBytes);
        using var childLayer = Open(vhdx, childBytes);
        Assert.True(childLayer.NeedsParent); Assert.Equal(parentLayer.Capacity, childLayer.Capacity);
        Assert.Contains(vhdx ? @"C:\Images\parent.vhdx" : @"C:\Images\parent.vhd", childLayer.GetParentLocations());
        if (!vhdx)
        {
            var bytes = child.ToArray();
            Assert.Equal(original.AsSpan(original.Length - 512 + 68, 16).ToArray(), bytes.AsSpan(512 + 40, 16).ToArray());
            var header = bytes.AsSpan(512, 1024).ToArray();
            var checksum = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(36));
            header.AsSpan(36, 4).Clear(); Assert.Equal(checksum, ~header.Aggregate(0u, (sum, value) => sum + value));
            Assert.All(header.AsSpan(768).ToArray(), b => Assert.Equal(0, b));
        }
        using var parentData = parentLayer.OpenContent(null!, Ownership.None);
        using var childData = childLayer.OpenContent(parentData, Ownership.None);
        childData.Position = 0; Assert.Equal(31, childData.ReadByte());
        childData.Position = (2 << 20) - 1; Assert.Equal(0, childData.ReadByte()); Assert.Equal(99, childData.ReadByte());
        childData.Position = capacity - 1; Assert.Equal(51, childData.ReadByte());
        parentData.Position = (2 << 20) - 1; Assert.Equal(41, parentData.ReadByte()); Assert.Equal(42, parentData.ReadByte());
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void InitializationFailureAndParentChainsCannotPublishPartialChild(bool vhdx)
    {
        using var parent = new MemoryStream();
        if (vhdx) VhdxImageWriter.Write(parent, 32L << 20, false);
        else VhdImageWriter.Write(parent, 32L << 20, false);
        var original = parent.ToArray();
        using var failed = new MemoryStream();
        Assert.Throws<InvalidOperationException>(() => Write(vhdx, failed, parent, content =>
        { content.WriteByte(42); throw new InvalidOperationException("simulated"); }));
        Assert.Equal(0, failed.Length); Assert.Equal(original, parent.ToArray());
        using var child = new MemoryStream(); Write(vhdx, child, parent, null);
        Assert.Throws<NotSupportedException>(() => Write(vhdx, failed, child, null));
        Assert.Equal(0, failed.Length);
    }

    private static void Write(bool vhdx, Stream destination, Stream parent, Action<Stream>? initialize)
    {
        if (vhdx) VhdxDifferencingImageWriter.Write(destination, parent, @"C:\Images\parent.vhdx", "parent.vhdx", Modified, initialize);
        else VhdDifferencingImageWriter.Write(destination, parent, @"C:\Images\parent.vhd", "parent.vhd", Modified, initialize);
    }
    private static VirtualDiskLayer Open(bool vhdx, Stream image) => vhdx
        ? new DiscUtils.Vhdx.DiskImageFile(image, Ownership.None)
        : new DiscUtils.Vhd.DiskImageFile(image, Ownership.None);
}
