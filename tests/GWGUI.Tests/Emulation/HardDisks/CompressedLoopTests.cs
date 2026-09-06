using System.Buffers.Binary;
using System.IO.Compression;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class CompressedLoopTests
{
    [Theory]
    [InlineData(512)] [InlineData(65536)] [InlineData(262144)]
    public void EachCompressedBlockHasTheDeclaredLengthAndContents(int blockBytes)
    {
        using var image = new MemoryStream();
        CompressedLoopImageWriter.Write(image, blockBytes * 5L, content =>
        { content.Position = blockBytes - 1; content.WriteByte(41); content.WriteByte(42); content.Position = content.Length - 1; content.WriteByte(43); }, blockBytes);
        Assert.Equal("cloop", DiskContainerSignatures.Identify(image));
        using var decoded = Decode(image.ToArray());
        Assert.Equal(blockBytes * 5L, decoded.Length);
        decoded.Position = blockBytes - 1; Assert.Equal(41, decoded.ReadByte()); Assert.Equal(42, decoded.ReadByte());
        decoded.Position = blockBytes * 3L; Assert.Equal(0, decoded.ReadByte());
        decoded.Position = decoded.Length - 1; Assert.Equal(43, decoded.ReadByte());
    }

    [Fact]
    public void ComposedFilesystemReopensAfterDecompression()
    {
        using var image = new MemoryStream();
        DiskImageBuilder.Write(image, new(32L << 20, "cloop-v2", "none", [new(0, 32L << 20, "fat16", "DATA")]));
        using var decoded = Decode(image.ToArray());
        using var fs = new DiscUtils.Fat.FatFileSystem(decoded); Assert.Equal("DATA", fs.VolumeLabel.Trim());
    }

    [Fact]
    public void PartialBlocksAndInitializerFailuresWriteNothing()
    {
        using var image = new MemoryStream();
        Assert.Throws<ArgumentException>(() => CompressedLoopImageWriter.Write(image, 66048));
        Assert.Throws<ArgumentException>(() => CompressedLoopImageWriter.Write(image, 65536, blockBytes: 1536));
        Assert.Throws<InvalidOperationException>(() => CompressedLoopImageWriter.Write(image, 65536, stream => stream.SetLength(0)));
        Assert.Equal(0, image.Length);
    }

    private static SparseMemoryStream Decode(byte[] bytes)
    {
        var size = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(128));
        var count = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(132));
        var result = new SparseMemoryStream(); result.SetLength(count * (long)size);
        ulong previousEnd = 136 + (count + 1UL) * 8;
        for (var block = 0; block < count; block++)
        {
            var start = BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(136 + block * 8));
            var end = BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(144 + block * 8));
            Assert.Equal(previousEnd, start); Assert.True(end > start); Assert.True(end <= (ulong)bytes.Length);
            using var encoded = new MemoryStream(bytes, (int)start, (int)(end - start));
            using var zlib = new ZLibStream(encoded, CompressionMode.Decompress); zlib.CopyTo(result);
            Assert.Equal((block + 1L) * size, result.Position); previousEnd = end;
        }
        Assert.Equal((ulong)bytes.Length, previousEnd); result.Position = 0; return result;
    }
}
