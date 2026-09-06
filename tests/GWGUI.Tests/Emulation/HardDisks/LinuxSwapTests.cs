using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class LinuxSwapTests
{
    [Theory]
    [InlineData(4096, false)] [InlineData(8192, false)] [InlineData(16384, false)] [InlineData(32768, false)] [InlineData(65536, false)]
    [InlineData(4096, true)] [InlineData(8192, true)] [InlineData(16384, true)] [InlineData(32768, true)] [InlineData(65536, true)]
    public void HeaderFieldsAndBadPageMapFollowTheirDeclaredByteOrder(int pageBytes, bool bigEndian)
    {
        var uuid = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");
        using var volume = new SparseMemoryStream(); volume.SetLength(pageBytes * 123L);
        LinuxSwapVolumeFormatter.Format(volume, "SWAP", pageBytes, bigEndian, uuid, [3, 90, 122]);
        var header = new byte[pageBytes]; volume.Position = 0; volume.ReadExactly(header);
        uint U32(int offset) => bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(header.AsSpan(offset)) : BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(offset));
        Assert.Equal(1U, U32(1024)); Assert.Equal(122U, U32(1028)); Assert.Equal(3U, U32(1032));
        Assert.Equal(new uint[] { 3, 90, 122 }, Enumerable.Range(0, 3).Select(i => U32(1536 + i * 4)));
        Assert.Equal(uuid, new Guid(header.AsSpan(1036, 16), bigEndian: true));
        Assert.True(header.AsSpan(1052, 5).SequenceEqual("SWAP\0"u8));
        Assert.All(header[..1024], value => Assert.Equal(0, value));
        Assert.True(header.AsSpan(pageBytes - 10).SequenceEqual("SWAPSPACE2"u8));
        var id = $"linux-swap-{pageBytes / 1024}k" + (bigEndian ? "-be" : "");
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(8L << 20, "raw", "mbr", [new(1L << 20, 4L << 20, id, "SWAP")]));
        disk.Position = 450; Assert.Equal(0x82, disk.ReadByte());
        disk.Position = (1L << 20) + pageBytes - 10;
        var signature = new byte[10]; disk.ReadExactly(signature); Assert.True(signature.AsSpan().SequenceEqual("SWAPSPACE2"u8));
    }

    [Fact]
    public void MaximumLastPageAndGptTypeArePreserved()
    {
        using var volume = new SparseMemoryStream(); volume.SetLength((long)uint.MaxValue * 65536);
        LinuxSwapVolumeFormatter.Format(volume, "", 65536, uuid: Guid.Empty);
        volume.Position = 1028; var value = new byte[4]; volume.ReadExactly(value);
        Assert.Equal(uint.MaxValue - 1, BinaryPrimitives.ReadUInt32LittleEndian(value));
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(8L << 20, "raw", "gpt", [new(1L << 20, 4L << 20, "linux-swap-4k", "SWAP")]));
        disk.Position = 1024; var type = new byte[16]; disk.ReadExactly(type);
        Assert.Equal(Guid.Parse("0657FD6D-A4AB-43C4-84E5-0933C84B4F4F"), new Guid(type));
    }

    [Fact]
    public void InvalidLabelsBadPagesAndCapacitiesAreRejectedBeforeWriting()
    {
        using var volume = new MemoryStream(new byte[4096 * 10]);
        Assert.Throws<ArgumentException>(() => LinuxSwapVolumeFormatter.Format(volume, "1234567890123456"));
        Assert.Throws<ArgumentException>(() => LinuxSwapVolumeFormatter.Format(volume, "é"));
        Assert.Throws<ArgumentException>(() => LinuxSwapVolumeFormatter.Format(volume, "", badPages: [1, 1]));
        Assert.Throws<ArgumentException>(() => LinuxSwapVolumeFormatter.Format(volume, "", badPages: [0]));
        Assert.Throws<ArgumentException>(() => LinuxSwapVolumeFormatter.Format(volume, "", badPages: [10]));
        Assert.Throws<ArgumentException>(() => LinuxSwapVolumeFormatter.Format(volume, "", badPages: [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => LinuxSwapVolumeFormatter.Validate(4096 * 9, ""));
        Assert.Throws<ArgumentOutOfRangeException>(() => LinuxSwapVolumeFormatter.Validate(4096 * 10 + 512, ""));
        Assert.Throws<ArgumentOutOfRangeException>(() => LinuxSwapVolumeFormatter.Validate(((long)uint.MaxValue + 1) * 4096, ""));
        Assert.All(volume.ToArray(), value => Assert.Equal(0, value));
    }
}
