using System.Buffers.Binary;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using DiscUtils.Fat;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class UdifImageTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void BlockMapPreservesFilesAndChunkBoundaries(bool compressed)
    {
        using var image = new MemoryStream();
        const long capacity = 32L << 20;
        UdifImageWriter.Write(image, capacity, content =>
        {
            GWGUI.Emulation.HardDisks.FileSystems.ExplicitFatVolumeFormatter.Format(content, "DATA",
                GWGUI.Emulation.HardDisks.FileSystems.FatVariant.Fat16);
            using (var fs = new FatFileSystem(content))
            using (var file = fs.OpenFile("CHECK.BIN", FileMode.Create, FileAccess.ReadWrite))
                file.Write("UDIF file contents"u8);
            content.Position = (8 << 20) - 1; content.WriteByte(41); content.WriteByte(42);
            content.Position = capacity - 1; content.WriteByte(43);
        }, compressed);
        using var decoded = Decode(image.ToArray());
        Assert.Equal(capacity, decoded.Length);
        decoded.Position = (8 << 20) - 1; Assert.Equal(41, decoded.ReadByte()); Assert.Equal(42, decoded.ReadByte());
        decoded.Position = capacity - 1; Assert.Equal(43, decoded.ReadByte());
        decoded.Position = 0;
        using var reopened = new FatFileSystem(decoded);
        using var check = reopened.OpenFile("CHECK.BIN", FileMode.Open, FileAccess.Read);
        var actual = new byte[18]; check.ReadExactly(actual); Assert.Equal("UDIF file contents"u8.ToArray(), actual);
        if (compressed) Assert.True(image.Length < 1 << 20);
    }

    [Theory]
    [InlineData("udif")] [InlineData("udif-zlib")]
    public void EmptyLargeDiskIsAnExplicitZeroRun(string container)
    {
        using var image = new MemoryStream();
        DiskImageBuilder.Write(image, new(1L << 40, container, "none", []));
        Assert.True(image.Length < 4096);
        using var decoded = Decode(image.ToArray());
        Assert.Equal(1L << 40, decoded.Length); decoded.Position = decoded.Length - 1; Assert.Equal(0, decoded.ReadByte());
    }

    [Fact]
    public void InvalidCapacityAndInitializerLeaveDestinationUntouched()
    {
        using var image = new MemoryStream();
        Assert.Throws<ArgumentOutOfRangeException>(() => UdifImageWriter.Write(image, 513));
        Assert.Throws<ArgumentOutOfRangeException>(() => UdifImageWriter.Write(image, (1L << 40) + 512));
        Assert.Throws<InvalidOperationException>(() => UdifImageWriter.Write(image, 512, content => content.SetLength(1024)));
        Assert.Equal(0, image.Length);
    }

    private static SparseMemoryStream Decode(byte[] image)
    {
        var footer = image.AsSpan(image.Length - 512).ToArray();
        Assert.Equal("koly"u8.ToArray(), footer[..4]); Assert.Equal(4u, U32(footer, 4)); Assert.Equal(512u, U32(footer, 8));
        Assert.Equal(1u, U32(footer, 56)); Assert.Equal(1u, U32(footer, 60));
        var xmlOffset = checked((int)U64(footer, 216)); var xmlLength = checked((int)U64(footer, 224));
        Assert.Equal(image.Length - 512, xmlOffset + xmlLength); Assert.Equal((ulong)xmlOffset, U64(footer, 32));
        using var xmlStream = new MemoryStream(image, xmlOffset, xmlLength);
        using var reader = XmlReader.Create(xmlStream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null });
        var xml = XDocument.Load(reader); var map = Convert.FromBase64String(xml.Descendants("data").Single().Value);
        Assert.Equal("mish"u8.ToArray(), map[..4]); Assert.Equal(1u, U32(map, 4));
        var result = new SparseMemoryStream(); result.SetLength(checked((long)U64(footer, 492) * 512));
        Assert.Equal(U64(footer, 492), U64(map, 16));
        var count = U32(map, 200); Assert.Equal(204 + count * 40L, map.Length);
        long covered = 0;
        for (var i = 0; i < count; i++)
        {
            var at = 204 + i * 40; var type = U32(map, at);
            var sector = checked((long)U64(map, at + 8)); var sectors = checked((long)U64(map, at + 16));
            Assert.Equal(covered, sector);
            if (type == uint.MaxValue) { Assert.Equal(count - 1, (uint)i); Assert.Equal(0, sectors); break; }
            Assert.True(sectors > 0); covered += sectors;
            if (type == 0) { Assert.Equal(0UL, U64(map, at + 32)); continue; }
            var start = checked((int)U64(map, at + 24)); var length = checked((int)U64(map, at + 32));
            Assert.InRange(start + length, 0, xmlOffset);
            using var chunk = new MemoryStream(image, start, length);
            result.Position = sector * 512;
            if (type == 1) { Assert.Equal(sectors * 512, length); chunk.CopyTo(result); }
            else
            {
                Assert.Equal(0x80000005u, type);
                using var zlib = new ZLibStream(chunk, CompressionMode.Decompress); zlib.CopyTo(result);
                Assert.Equal((sector + sectors) * 512, result.Position);
            }
        }
        Assert.Equal(result.Length / 512, covered); result.Position = 0; return result;
    }
    private static uint U32(byte[] bytes, int at) => BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(at));
    private static ulong U64(byte[] bytes, int at) => BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(at));
}
