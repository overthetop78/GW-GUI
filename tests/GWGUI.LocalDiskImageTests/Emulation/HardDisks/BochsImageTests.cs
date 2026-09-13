using System.Buffers.Binary;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class BochsImageTests
{
    [Theory]
    [InlineData(1, 4096)] [InlineData(2, 4096)]
    [InlineData(1, 1048576)] [InlineData(2, 1048576)]
    [InlineData(2, 8388608)]
    public void CatalogAndBitmapMapBoundaryWrites(int version, int extent)
    {
        var capacity = 4L * extent + 512;
        using var image = new MemoryStream();
        BochsImageWriter.Write(image, capacity, content =>
        {
            content.Position = extent - 1; content.WriteByte(41); content.WriteByte(42);
            content.Position = capacity - 1; content.WriteByte(43);
        }, version, extent);
        var bytes = image.ToArray();
        Assert.Equal("Bochs Virtual HD Image"u8.ToArray(), bytes[..22]);
        Assert.Equal((uint)version << 16, U32(bytes, 64));
        Assert.Equal((ulong)capacity, BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(version == 1 ? 84 : 88)));
        Assert.Equal(41, Read(bytes, extent - 1)); Assert.Equal(42, Read(bytes, extent));
        Assert.Equal(43, Read(bytes, capacity - 1)); Assert.Equal(0, Read(bytes, 3L * extent));
        var bitmapBytes = ((U32(bytes, 76) + 511) / 512) * 512;
        var endOrdinal = U32(bytes, 512 + 4 * 4);
        var finalBitmap = checked((int)(512 + U32(bytes, 72) * 4 + endOrdinal * (extent + bitmapBytes)));
        Assert.Equal(1, bytes[finalBitmap]); Assert.All(bytes.AsSpan(finalBitmap + 1, (int)bitmapBytes - 1).ToArray(), b => Assert.Equal(0, b));
    }

    [Theory]
    [InlineData("bochs-v1")] [InlineData("bochs-v2")]
    public void EmptyImageNeedsOnlyHeaderAndCatalog(string id)
    {
        using var image = new MemoryStream();
        DiskImageBuilder.Write(image, new(32L << 20, id, "none", []));
        Assert.Equal(1024, image.Length); Assert.Equal(0, Read(image.ToArray(), (32L << 20) - 1));
    }

    [Fact]
    public void InvalidGeometryAndInitializerCannotWriteMetadata()
    {
        using var image = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => BochsImageWriter.Write(image, 512, extentBytes: 8193));
        Assert.Throws<ArgumentOutOfRangeException>(() => BochsImageWriter.Write(image, (1L << 40) + 512));
        Assert.Throws<InvalidOperationException>(() => BochsImageWriter.Write(image, 512, s => s.SetLength(1024)));
        Assert.Equal(0, image.Length);
    }

    private static int Read(byte[] bytes, long offset)
    {
        var extent = U32(bytes, 80); var index = checked((int)(offset / extent));
        var ordinal = U32(bytes, 512 + index * 4); if (ordinal == uint.MaxValue) return 0;
        var bitmap = ((U32(bytes, 76) + 511) / 512) * 512;
        var start = 512L + U32(bytes, 72) * 4 + ordinal * (long)(bitmap + extent);
        var sector = offset % extent / 512;
        if ((bytes[(int)(start + sector / 8)] & (1 << (int)(sector % 8))) == 0) return 0;
        return bytes[(int)(start + bitmap + offset % extent)];
    }
    private static uint U32(byte[] bytes, int at) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(at));
}
