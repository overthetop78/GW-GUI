using System.Buffers.Binary;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class V7FormattingTests
{
    [Theory]
    [InlineData("v7-le", 0)] [InlineData("v7-be", 1)] [InlineData("v7-pdp", 2)]
    public void FreeBlockChainExcludesMetadataAndRoot(string id, int order)
    {
        using var disk = new SparseMemoryStream();
        DiskImageBuilder.Write(disk, new(512L << 10, "raw", "none", [new(0, 512L << 10, id, "")]));
        byte[] Block(uint block)
        { var bytes = new byte[512]; disk.Position = block * 512L; disk.ReadExactly(bytes); return bytes; }
        ushort U16(byte[] bytes, int at) => order == 1 ? BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(at)) : BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(at));
        uint U32(byte[] bytes, int at)
        {
            var value = order == 1 ? BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(at)) : BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(at));
            return order == 2 ? value >> 16 | value << 16 : value;
        }
        var super = Block(1); var first = U16(super, 0); var total = U32(super, 2);
        Assert.Equal(1024u, total);
        var cache = new List<uint>();
        for (var i = 0; i < U16(super, 6); i++) cache.Add(U32(super, 8 + i * 4));
        var free = new HashSet<uint>();
        while (cache.Count > 0)
        {
            var block = cache[^1]; cache.RemoveAt(cache.Count - 1);
            if (block == 0) { Assert.Empty(cache); break; }
            Assert.InRange(block, (uint)first + 1, total - 1); Assert.True(free.Add(block));
            if (cache.Count != 0) continue;
            var link = Block(block); var count = U16(link, 0); Assert.InRange((int)count, 1, 50);
            for (var i = 0; i < count; i++) cache.Add(U32(link, 2 + i * 4));
        }
        Assert.Equal(total - first - 1, (uint)free.Count); Assert.Equal((uint)free.Count, U32(super, 418));
        Assert.Equal(254, U16(super, 422));
        var inode = Block(2); Assert.Equal(0x41ed, U16(inode, 64)); Assert.Equal(2, U16(inode, 66)); Assert.Equal(32u, U32(inode, 72));
        var pointer = order switch
        { 1 => inode[76] * 65536 + inode[77] * 256 + inode[78], 2 => inode[76] * 65536 + inode[78] * 256 + inode[77], _ => inode[78] * 65536 + inode[77] * 256 + inode[76] };
        Assert.Equal(first, pointer); var root = Block((uint)pointer);
        Assert.Equal(2, U16(root, 0)); Assert.Equal(2, U16(root, 16)); Assert.Equal((byte)'.', root[2]); Assert.Equal(".."u8.ToArray(), root[18..20]);
    }

    [Fact]
    public void FullVolumeHasAnEmptyFreeListAndInvalidGeometryIsRejected()
    {
        using var disk = new MemoryStream(); disk.SetLength(5 * 512);
        V7VolumeFormatter.Format(disk, inodes: 16);
        var bytes = disk.ToArray(); Assert.Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(512 + 418)));
        Assert.Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(512 + 8)));
        Assert.Throws<ArgumentOutOfRangeException>(() => V7VolumeFormatter.Validate(0x1000000L * 512));
        Assert.Throws<ArgumentException>(() => V7VolumeFormatter.Validate(32L << 20, inodes: 17));
    }
}
