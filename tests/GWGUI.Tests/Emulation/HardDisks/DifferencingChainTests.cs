using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class DifferencingChainTests
{
    private static string ParentPath(bool vhdx) => Path.GetFullPath(vhdx ? "parent.vhdx" : "parent.vhd");
    private static readonly DateTime Modified = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static void Base(Stream destination, bool vhdx)
    {
        void Initialize(Stream stream)
        { stream.WriteByte(1); stream.Position = 512; stream.WriteByte(2); }
        if (vhdx) VhdxImageWriter.Write(destination, 4L << 20, initialize: Initialize);
        else VhdImageWriter.Write(destination, 4L << 20, initialize: Initialize);
    }
    private static void Child(Stream destination, IReadOnlyList<Stream> parents, bool vhdx, Action<Stream>? initialize = null)
    {
        if (vhdx) VhdxDifferencingImageWriter.WriteChain(destination, parents, ParentPath(true), "parent.vhdx", Modified, initialize);
        else VhdDifferencingImageWriter.WriteChain(destination, parents, ParentPath(false), "parent.vhd", Modified, initialize);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void GrandchildInheritsAllAncestorsAndCanOverrideWithZero(bool vhdx)
    {
        using var basis = new MemoryStream(); Base(basis, vhdx);
        using var parent = new MemoryStream();
        Child(parent, [basis], vhdx, stream => { stream.Position = 0; stream.WriteByte(3); });
        var baseBytes = basis.ToArray(); var parentBytes = parent.ToArray();
        basis.Position = 17; parent.Position = 29;
        using var child = new MemoryStream();
        Child(child, [parent, basis], vhdx, stream =>
        {
            stream.Position = 0; Assert.Equal(3, stream.ReadByte());
            stream.Position = 512; Assert.Equal(2, stream.ReadByte());
            stream.Position = 512; stream.WriteByte(0);
            stream.Position = 1024; stream.WriteByte(4);
        });
        Assert.Equal(baseBytes, basis.ToArray()); Assert.Equal(parentBytes, parent.ToArray());
        Assert.Equal(17, basis.Position); Assert.Equal(29, parent.Position);
        using var baseInput = new MemoryStream(baseBytes, writable: false);
        using var parentInput = new MemoryStream(parentBytes, writable: false);
        using var childInput = new MemoryStream(child.ToArray(), writable: false);
        using DiscUtils.VirtualDiskLayer baseLayer = vhdx ? new DiscUtils.Vhdx.DiskImageFile(baseInput, Ownership.None) : new DiscUtils.Vhd.DiskImageFile(baseInput, Ownership.None);
        using DiscUtils.VirtualDiskLayer parentLayer = vhdx ? new DiscUtils.Vhdx.DiskImageFile(parentInput, Ownership.None) : new DiscUtils.Vhd.DiskImageFile(parentInput, Ownership.None);
        using DiscUtils.VirtualDiskLayer childLayer = vhdx ? new DiscUtils.Vhdx.DiskImageFile(childInput, Ownership.None) : new DiscUtils.Vhd.DiskImageFile(childInput, Ownership.None);
        using var baseContent = baseLayer.OpenContent(null!, Ownership.None);
        using var parentContent = parentLayer.OpenContent(baseContent, Ownership.None);
        using var content = childLayer.OpenContent(parentContent, Ownership.None);
        foreach (var (offset, value) in new[] { (0, 3), (512, 0), (1024, 4) })
        { content.Position = offset; Assert.Equal(value, content.ReadByte()); }
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void WrongMissingAndRepeatedAncestorsAreRejectedWithoutPublishing(bool vhdx)
    {
        using var basis = new MemoryStream(); Base(basis, vhdx);
        using var other = new MemoryStream(); Base(other, vhdx);
        using var parent = new MemoryStream(); Child(parent, [basis], vhdx);
        using var destination = new MemoryStream();
        Assert.Throws<InvalidDataException>(() => Child(destination, [parent, other], vhdx));
        Assert.Throws<NotSupportedException>(() => Child(destination, [parent], vhdx));
        Assert.Throws<ArgumentException>(() => Child(destination, [basis, other], vhdx));
        Assert.Throws<ArgumentException>(() => Child(destination, [parent, parent], vhdx));
        Assert.Throws<ArgumentException>(() => Child(destination, [], vhdx));
        Assert.Equal(0, destination.Length);
        Assert.True(basis.CanRead); Assert.True(parent.CanRead); Assert.True(other.CanRead);
    }
}
