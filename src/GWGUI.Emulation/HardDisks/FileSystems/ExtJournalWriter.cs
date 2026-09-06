using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.FileSystems;

internal static class ExtJournalWriter
{
    internal const int BlockCount = 1024;
    private const int Block = 4096;
    internal static void Write(Stream volume, uint firstBlock, Span<byte> inode, ReadOnlySpan<byte> uuid, bool extents)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(inode, 0x8180);
        Le32(inode, 4, BlockCount * Block); BinaryPrimitives.WriteUInt16LittleEndian(inode[26..], 1);
        Le32(inode, 28, (uint)(BlockCount + (extents ? 0 : 1)) * (Block / 512));
        if (extents) SetExtent(inode, firstBlock, BlockCount);
        else
        {
            for (var i = 0; i < 12; i++) Le32(inode, 40 + i * 4, firstBlock + (uint)i);
            Le32(inode, 88, firstBlock + BlockCount);
            var indirect = new byte[Block];
            for (var i = 12; i < BlockCount; i++) Le32(indirect, (i - 12) * 4, firstBlock + (uint)i);
            volume.Position = ((long)firstBlock + BlockCount) * Block; volume.Write(indirect);
        }
        var zero = new byte[Block]; volume.Position = (long)firstBlock * Block;
        for (var i = 0; i < BlockCount; i++) volume.Write(zero);
        var header = new byte[1024];
        Be32(header, 0, 0xc03b3998); Be32(header, 4, 4); Be32(header, 12, Block);
        Be32(header, 16, BlockCount); Be32(header, 20, 1); Be32(header, 24, 1);
        Be32(header, 40, 1);
        uuid.CopyTo(header.AsSpan(48)); Be32(header, 64, 1); uuid.CopyTo(header.AsSpan(256));
        volume.Position = (long)firstBlock * Block; volume.Write(header);
    }
    internal static void SetExtent(Span<byte> inode, uint firstBlock, ushort count)
    {
        inode.Slice(40, 60).Clear(); Le32(inode, 32, 0x80000);
        BinaryPrimitives.WriteUInt16LittleEndian(inode[40..], 0xf30a);
        BinaryPrimitives.WriteUInt16LittleEndian(inode[42..], 1);
        BinaryPrimitives.WriteUInt16LittleEndian(inode[44..], 4);
        BinaryPrimitives.WriteUInt16LittleEndian(inode[56..], count); Le32(inode, 60, firstBlock);
    }
    private static void Le32(Span<byte> bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes[offset..], value);
    private static void Be32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
}
