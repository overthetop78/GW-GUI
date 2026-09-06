using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

internal static class HfsVolumeWriter
{
    private const int Block = 4096;
    internal static void Validate(long capacity, string label)
    {
        if (capacity < 8L << 20 || capacity > 1L << 40 || capacity % Block != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        _ = HfsVolumeLabel.Normalize(label);
    }
    internal static void Format(Stream volume, string label, bool caseSensitive)
    {
        Validate(volume.Length, label);
        label = HfsVolumeLabel.Normalize(label);
        var total = checked((uint)(volume.Length / Block));
        var bitmapBlocks = (total + Block * 8 - 1) / (Block * 8);
        var extentsBlock = 1 + bitmapBlocks; var catalogBlock = extentsBlock + 1;
        var attributesBlock = catalogBlock + 2; var firstFree = attributesBlock + 1;
        var bitmap = new byte[checked((int)bitmapBlocks * Block)];
        for (uint block = 0; block < firstFree; block++) Allocate(bitmap, block);
        for (var block = total - 1; block < (long)bitmap.Length * 8; block++) Allocate(bitmap, block);
        Put(volume, Block, bitmap);
        Put(volume, (long)extentsBlock * Block, HfsBTreeWriter.EmptyExtents());
        Put(volume, (long)catalogBlock * Block, HfsBTreeWriter.EmptyCatalog(label, caseSensitive));
        Put(volume, (long)attributesBlock * Block, HfsBTreeWriter.EmptyAttributes());
        var header = new byte[512]; U16(header, 0, caseSensitive ? 0x4858 : 0x482b); U16(header, 2, caseSensitive ? 5 : 4);
        U32(header, 4, 0x100); U32(header, 8, 0x47574755);
        U32(header, 40, Block); U32(header, 44, total); U32(header, 48, total - firstFree - 1); U32(header, 52, firstFree);
        U32(header, 56, Block); U32(header, 60, Block); U32(header, 64, 16); U32(header, 68, 1);
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(72), 1);
        Guid.NewGuid().ToByteArray().AsSpan(0, 8).CopyTo(header.AsSpan(104));
        Fork(header, 112, 1, bitmapBlocks); Fork(header, 192, extentsBlock, 1); Fork(header, 272, catalogBlock, 2);
        Fork(header, 352, attributesBlock, 1);
        Put(volume, 0, new byte[Block]); Put(volume, volume.Length - Block, new byte[Block]);
        Put(volume, 1024, header); Put(volume, volume.Length - 1024, header); volume.Flush();
    }
    private static void Allocate(byte[] bitmap, uint block) => bitmap[block / 8] |= (byte)(0x80 >> (int)(block % 8));
    private static void Fork(byte[] header, int offset, uint start, uint count)
    {
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(offset), (ulong)count * Block);
        U32(header, offset + 8, Block); U32(header, offset + 12, count); U32(header, offset + 16, start); U32(header, offset + 20, count);
    }
    private static void U16(byte[] bytes, int offset, int value) => BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(offset), checked((ushort)value));
    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
    private static void Put(Stream stream, long offset, byte[] bytes) { stream.Position = offset; stream.Write(bytes); }
}
