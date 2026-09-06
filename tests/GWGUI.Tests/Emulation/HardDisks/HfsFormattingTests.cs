using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class HfsFormattingTests
{
    [Theory]
    [InlineData(false, 8)] [InlineData(false, 256)]
    [InlineData(true, 8)] [InlineData(true, 256)]
    public void RootCatalogCanBeReadAndAllocationMatchesFreeSpace(bool sensitive, int mib)
    {
        using var volume = new SparseMemoryStream(); volume.SetLength((long)mib << 20);
        if (sensitive) HfsxVolumeFormatter.Format(volume, "DATA"); else HfsPlusVolumeFormatter.Format(volume, "DATA");
        var header = new byte[512]; volume.Position = 1024; volume.ReadExactly(header);
        var backup = new byte[512]; volume.Position = volume.Length - 1024; volume.ReadExactly(backup);
        Assert.Equal(header, backup);
        Assert.Equal(sensitive ? 0x4858 : 0x482b, BinaryPrimitives.ReadUInt16BigEndian(header));
        var bitmapStart = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(128));
        var bitmapBlocks = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(132));
        var bitmap = new byte[bitmapBlocks * 4096]; volume.Position = (long)bitmapStart * 4096; volume.ReadExactly(bitmap);
        var total = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(44));
        var free = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(48));
        Assert.Equal((long)free, bitmap.Sum(value => (long)(8 - System.Numerics.BitOperations.PopCount((uint)value))));
        Assert.NotEqual(0, bitmap[0] & 0x80); Assert.NotEqual(0, bitmap[(total - 1) / 8] & (0x80 >> (int)((total - 1) % 8)));
        var catalog = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(288));
        var node = new byte[4096]; volume.Position = (long)catalog * 4096; volume.ReadExactly(node);
        Assert.Equal(sensitive ? 0xbc : 0xcf, node[14 + 37]);
        volume.Position = 0;
        using var fs = new DiscUtils.HfsPlus.HfsPlusFileSystem(volume);
        Assert.Equal("DATA", fs.VolumeLabel);
        Assert.Empty(fs.Root.GetFiles()); Assert.Empty(fs.Root.GetDirectories());
    }

    [Fact]
    public void UnimplementedUnicodeLabelsAreRejectedBeforeWriting()
    {
        using var volume = new MemoryStream(); volume.SetLength(8L << 20); volume.WriteByte(42);
        Assert.Throws<ArgumentException>(() => HfsPlusVolumeFormatter.Format(volume, "DATA😀"));
        volume.Position = 0; Assert.Equal(42, volume.ReadByte());
    }

    [Theory]
    [InlineData(false, "Données")]
    [InlineData(true, "Données")]
    [InlineData(false, "Donne\u0301es")]
    [InlineData(true, "Ångström")]
    public void LatinLabelsUseCanonicalDecomposition(bool sensitive, string label)
    {
        using var volume = new SparseMemoryStream(); volume.SetLength(16L << 20);
        if (sensitive) HfsxVolumeFormatter.Format(volume, label); else HfsPlusVolumeFormatter.Format(volume, label);
        volume.Position = 0;
        using var fs = new DiscUtils.HfsPlus.HfsPlusFileSystem(volume);
        Assert.NotNull(fs.VolumeLabel);
        Assert.Equal(label.Normalize(), fs.VolumeLabel.Normalize());
        var header = new byte[512]; volume.Position = 1024; volume.ReadExactly(header);
        var catalog = BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(288));
        var leaf = new byte[4096]; volume.Position = catalog * 4096L + 4096; volume.ReadExactly(leaf);
        var length = BinaryPrimitives.ReadUInt16BigEndian(leaf.AsSpan(20));
        Assert.Equal(label.Normalize(System.Text.NormalizationForm.FormD), System.Text.Encoding.BigEndianUnicode.GetString(leaf, 22, length * 2));
    }

    [Fact]
    public void LabelLimitAppliesAfterDecomposition()
    {
        using var volume = new MemoryStream(); volume.SetLength(8L << 20); volume.WriteByte(42);
        Assert.Throws<ArgumentException>(() => HfsPlusVolumeFormatter.Format(volume, new string('é', 128)));
        Assert.Throws<ArgumentException>(() => HfsPlusVolumeFormatter.Format(volume, "\u212b"));
        volume.Position = 0; Assert.Equal(42, volume.ReadByte());
    }
}
