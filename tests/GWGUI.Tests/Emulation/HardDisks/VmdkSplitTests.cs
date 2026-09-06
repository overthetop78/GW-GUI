using System.Text;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class VmdkSplitTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void SplitExtentsReassembleAcrossBoundariesIncludingThePartialLastExtent(bool sparse)
    {
        const long capacity = (2L << 20) + 512;
        var files = new Dictionary<string, byte[]>();
        var emitted = new List<string>();
        long[] offsets = [0, (1L << 20) - 1, 1L << 20, (2L << 20) - 1, 2L << 20, capacity - 1];
        VmdkSplitImageWriter.Write(capacity, "disk", sparse, (name, source) =>
        {
            emitted.Add(name); using var copy = new MemoryStream(); source.CopyTo(copy); files.Add(name, copy.ToArray());
        }, content =>
        {
            for (var i = 0; i < offsets.Length; i++) { content.Position = offsets[i]; content.WriteByte((byte)(i + 20)); }
        }, extentBytes: 1L << 20);
        Assert.Equal(4, emitted.Count);
        Assert.Equal("disk.vmdk", emitted[^1]);
        var descriptor = Encoding.UTF8.GetString(files["disk.vmdk"]);
        Assert.Contains(sparse ? "twoGbMaxExtentSparse" : "twoGbMaxExtentFlat", descriptor);
        Assert.Contains("RW 1 ", descriptor);
        foreach (var name in emitted.Take(3)) Assert.Contains(name, descriptor);
        using var layer = (DiscUtils.Vmdk.DiskImageFile)Activator.CreateInstance(typeof(DiscUtils.Vmdk.DiskImageFile),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null,
            [new VmdkImageSetTests.MemoryLocator(files), "disk.vmdk", FileAccess.Read], null)!;
        Assert.Equal(capacity, layer.Capacity);
        using var logical = layer.OpenContent(null!, Ownership.None);
        for (var i = 0; i < offsets.Length; i++)
        { logical.Position = offsets[i]; Assert.Equal(i + 20, logical.ReadByte()); }
        logical.Position = (1L << 20) - 1;
        Assert.Equal(new byte[] { 21, 22, 0, 0 }, logical.ReadExactly(4));
        logical.Position = 65536; Assert.Equal(0, logical.ReadByte());
        if (sparse)
            foreach (var name in emitted.Take(3)) Assert.All(files[name][28..44], value => Assert.Equal(0, value));
    }

    [Fact]
    public void InvalidLayoutAndInitializationEmitNoMember()
    {
        var count = 0;
        Assert.Throws<ArgumentOutOfRangeException>(() => VmdkSplitImageWriter.Write(1L << 40, "disk", true, (_, _) => count++, extentBytes: 65536));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmdkSplitImageWriter.Write(65536, "disk", true, (_, _) => count++, extentBytes: 1));
        Assert.Throws<ArgumentException>(() => VmdkSplitImageWriter.Write(65536, "disk\"", false, (_, _) => count++));
        Assert.Throws<InvalidOperationException>(() => VmdkSplitImageWriter.Write(65536, "disk", true, (_, _) => count++, stream => stream.SetLength(0)));
        Assert.Equal(0, count);
    }

    [Fact]
    public void MonolithicFlatHasOneLinearExtentAndASeparateDescriptor()
    {
        var files = new Dictionary<string, byte[]>();
        VmdkImageSetWriter.Write(65536, "disk", VmdkImageSetKind.MonolithicFlat, (name, source) =>
        {
            using var copy = new MemoryStream(); source.CopyTo(copy); files.Add(name, copy.ToArray());
        }, content => { content.Position = 65535; content.WriteByte(42); });
        Assert.Equal(["disk-flat.vmdk", "disk.vmdk"], files.Keys);
        Assert.Equal(65536, files["disk-flat.vmdk"].Length);
        Assert.Contains("monolithicFlat", Encoding.UTF8.GetString(files["disk.vmdk"]));
        using var layer = (DiscUtils.Vmdk.DiskImageFile)Activator.CreateInstance(typeof(DiscUtils.Vmdk.DiskImageFile),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null,
            [new VmdkImageSetTests.MemoryLocator(files), "disk.vmdk", FileAccess.Read], null)!;
        using var logical = layer.OpenContent(null!, Ownership.None);
        Assert.Equal(65536, logical.Length);
        logical.Position = 65535; Assert.Equal(42, logical.ReadByte());
    }
}
