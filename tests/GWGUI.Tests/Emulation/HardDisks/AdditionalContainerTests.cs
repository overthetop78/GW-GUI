using System.Buffers.Binary;
using System.Text;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class AdditionalContainerTests
{
    [Theory]
    [InlineData(DiskContainerKind.Chd)]
    [InlineData(DiskContainerKind.Parallels)]
    [InlineData(DiskContainerKind.Qcow)]
    public void SparseContainersPreserveBoundariesAndUnallocatedSpace(DiskContainerKind kind)
    {
        const long capacity = (512L << 20) + 512;
        long[] positions = [0, 65535, 65536, (1 << 20) - 1, 1 << 20, capacity - 513, capacity - 512, capacity - 1];
        using var image = new MemoryStream();
        DiskContainerWriter.Write(image, capacity, kind, content =>
        {
            for (var i = 0; i < positions.Length; i++) { content.Position = positions[i]; content.WriteByte((byte)(i + 1)); }
        });
        var bytes = image.ToArray();
        Assert.True(bytes.Length < 8 << 20);
        for (var i = 0; i < positions.Length; i++) Assert.Equal(i + 1, Read(bytes, kind, positions[i]));
        Assert.Equal(0, Read(bytes, kind, 123456789));
        Assert.Equal(0, Read(bytes, kind, 512));
        if (kind == DiskContainerKind.Chd)
        {
            Assert.Equal("MComprHD", Encoding.ASCII.GetString(bytes, 0, 8));
            Assert.Equal(5u, Be32(bytes, 12)); Assert.Equal((ulong)capacity, Be64(bytes, 32));
            var meta = checked((int)Be64(bytes, 48));
            Assert.Equal("GDDD", Encoding.ASCII.GetString(bytes, meta, 4));
            var length = (int)(Be32(bytes, meta + 4) & 0xffffff);
            Assert.Equal($"CYLS:{capacity / 512},HEADS:1,SECS:1,BPS:512\0", Encoding.ASCII.GetString(bytes, meta + 16, length));
            Assert.Equal(0UL, Be64(bytes, meta + 8));
        }
        else if (kind == DiskContainerKind.Parallels)
        {
            Assert.Equal("WithouFreSpacExt", Encoding.ASCII.GetString(bytes, 0, 16));
            Assert.Equal((ulong)(capacity / 512), Le64(bytes, 36));
            Assert.Equal(0x312e3276u, Le32(bytes, 44));
        }
        else
        {
            Assert.Equal(0x514649fbu, Be32(bytes, 0)); Assert.Equal(1u, Be32(bytes, 4));
            Assert.Equal((ulong)capacity, Be64(bytes, 24)); Assert.Equal(0u, Be32(bytes, 36));
        }
    }

    [Theory]
    [InlineData("chd")]
    [InlineData("parallels")]
    [InlineData("qcow")]
    public void ContainersAcceptComposedFileSystems(string container)
    {
        using var image = new MemoryStream();
        DiskImageBuilder.Write(image, new(2L << 20, container, "none", [new(0, 2L << 20, "fat12", "DATA")]));
        var kind = Enum.Parse<DiskContainerKind>(container, true); var bytes = image.ToArray();
        Assert.Equal(0x55, Read(bytes, kind, 510)); Assert.Equal(0xaa, Read(bytes, kind, 511));
    }

    [Theory]
    [InlineData("chd")]
    [InlineData("parallels")]
    [InlineData("qcow")]
    public void UnsupportedAllocationDoesNotWrite(string container)
    {
        using var image = new MemoryStream();
        Assert.Throws<NotSupportedException>(() => DiskImageBuilder.Write(image, new(4096, container, "none", [], true)));
        Assert.Equal(0, image.Length);
    }

    [Fact]
    public void ChdChecksGeometryBeforeInitialization()
    {
        using var image = new MemoryStream(); var initialized = false;
        Assert.Throws<ArgumentOutOfRangeException>(() => ChdImageWriter.Write(image, 4096,
            _ => initialized = true, heads: 3, sectors: 5));
        Assert.False(initialized); Assert.Equal(0, image.Length);
    }

    // Read the serialized structures independently of the writers' allocation helpers.
    private static byte Read(byte[] image, DiskContainerKind kind, long logical)
    {
        ulong physical;
        if (kind == DiskContainerKind.Chd)
        {
            var size = Be32(image, 56); var map = Be64(image, 40);
            physical = Be32(image, checked((int)(map + (ulong)(logical / size) * 4))) * (ulong)size;
            if (physical == 0) return 0;
            physical += (ulong)(logical % size);
        }
        else if (kind == DiskContainerKind.Parallels)
        {
            var size = Le32(image, 28) * 512UL;
            physical = Le32(image, checked(64 + (int)((ulong)logical / size) * 4)) * size;
            if (physical == 0) return 0;
            Assert.True(physical >= Le32(image, 48) * 512UL);
            physical += (ulong)logical % size;
        }
        else
        {
            var size = 1UL << image[32]; var entries = 1UL << image[33]; var unit = (ulong)logical / size;
            var l2 = Be64(image, checked((int)(Be64(image, 40) + unit / entries * 8)));
            if (l2 == 0) return 0;
            physical = Be64(image, checked((int)(l2 + unit % entries * 8)));
            if (physical == 0) return 0;
            physical += (ulong)logical % size;
        }
        return image[checked((int)physical)];
    }
    private static uint Be32(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset));
    private static ulong Be64(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(offset));
    private static uint Le32(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset));
    private static ulong Le64(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(offset));
}
