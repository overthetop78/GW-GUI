using System.Buffers.Binary;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class Qcow2VariantTests
{
    [Theory]
    [InlineData(2, 9)] [InlineData(3, 9)]
    [InlineData(2, 16)] [InlineData(3, 16)]
    [InlineData(2, 21)] [InlineData(3, 21)]
    public void MappingAndRefcountsRemainConsistent(int version, int bits)
    {
        var cluster = 1 << bits;
        var boundary = (long)cluster * (cluster / 8);
        using var image = new MemoryStream();
        Qcow2ImageWriter.Write(image, boundary + 512, content =>
        {
            content.Position = boundary - 1; content.WriteByte(42); content.WriteByte(43);
            content.Position = boundary + 511; content.WriteByte(44);
        }, version, bits);
        var bytes = image.ToArray();
        Assert.Equal((uint)version, U32(bytes, 4)); Assert.Equal((uint)bits, U32(bytes, 20));
        Assert.Equal(42, Read(bytes, boundary - 1)); Assert.Equal(43, Read(bytes, boundary));
        Assert.Equal(44, Read(bytes, boundary + 511)); Assert.Equal(0, Read(bytes, 0));
        if (version == 2) Assert.All(bytes[72..104], b => Assert.Equal(0, b));
        else { Assert.Equal(4u, U32(bytes, 96)); Assert.Equal(104u, U32(bytes, 100)); }
        CheckRefs(bytes);
    }

    [Fact]
    public void RefcountTableCanSpanSeveralClusters()
    {
        using var image = new MemoryStream();
        Qcow2ImageWriter.Write(image, 12L << 20, content => content.Write(new byte[10 << 20]), clusterBits: 9);
        var bytes = image.ToArray();
        Assert.True(U32(bytes, 56) > 1);
        CheckRefs(bytes);
    }

    [Fact]
    public void V2IsAvailableInComposition()
    {
        using var image = new MemoryStream();
        DiskImageBuilder.Write(image, new(4096, "qcow2-v2", "none", []));
        Assert.Equal(2u, U32(image.ToArray(), 4));
    }

    [Fact]
    public void InvalidOptionsAndResizingDoNotTouchDestination()
    {
        using var image = new MemoryStream(); image.WriteByte(42);
        Assert.Throws<ArgumentOutOfRangeException>(() => Qcow2ImageWriter.Write(image, 512, version: 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Qcow2ImageWriter.Write(image, 512, clusterBits: 8));
        Assert.Throws<ArgumentOutOfRangeException>(() => Qcow2ImageWriter.Write(image, 1L << 40, clusterBits: 9));
        Assert.Equal(new byte[] { 42 }, image.ToArray());
        using var empty = new MemoryStream();
        Assert.Throws<InvalidOperationException>(() => Qcow2ImageWriter.Write(empty, 512, content => content.SetLength(1024)));
        Assert.Equal(0, empty.Length);
    }

    private static void CheckRefs(byte[] bytes)
    {
        var cluster = 1 << (int)U32(bytes, 20);
        var table = (int)U64(bytes, 48);
        for (var index = 0; index < bytes.Length / cluster; index++)
        {
            var block = (int)U64(bytes, table + index / (cluster / 2) * 8);
            Assert.InRange(block, cluster, bytes.Length - cluster);
            Assert.Equal(1, BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(block + index % (cluster / 2) * 2)));
        }
    }
    private static byte Read(byte[] bytes, long offset)
    {
        var cluster = 1 << (int)U32(bytes, 20);
        var l1 = (int)U64(bytes, 40);
        var l2 = (long)(U64(bytes, l1 + (int)(offset / cluster / (cluster / 8)) * 8) & 0x00fffffffffffe00UL);
        if (l2 == 0) return 0;
        var data = (long)(U64(bytes, (int)l2 + (int)(offset / cluster % (cluster / 8)) * 8) & 0x00fffffffffffe00UL);
        return data == 0 ? (byte)0 : bytes[(int)(data + offset % cluster)];
    }
    private static uint U32(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset));
    private static ulong U64(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(offset));
}
