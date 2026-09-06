using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>V7 filesystem: 512-byte blocks, 64-byte inodes, 24-bit addresses and chained free-block caches.</summary>
public static class V7VolumeFormatter
{
    public static void Validate(long capacity, StorageByteOrder order = StorageByteOrder.LittleEndian, int inodes = 256)
    {
        if (!Enum.IsDefined(order) || inodes < 16 || inodes > 65528 || inodes % 8 != 0)
            throw new ArgumentException("Invalid V7 inode count or byte order.");
        if (capacity % 512 != 0 || capacity / 512 > 0xffffff || capacity / 512 < 3 + inodes / 8)
            throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    public static void Format(Stream volume, StorageByteOrder order = StorageByteOrder.LittleEndian, int inodes = 256)
    {
        Validate(volume.Length, order, inodes);
        var blocks = (uint)(volume.Length / 512); var firstData = 2 + inodes / 8;
        void U16(byte[] bytes, int at, int value)
        {
            if (order == StorageByteOrder.BigEndian) BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(at), checked((ushort)value));
            else BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(at), checked((ushort)value));
        }
        void U32(byte[] bytes, int at, uint value)
        {
            if (order == StorageByteOrder.BigEndian) BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(at), value);
            else
            {
                if (order == StorageByteOrder.PdpEndian) value = value >> 16 | value << 16;
                BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(at), value);
            }
        }
        var cache = new List<uint>(50) { 0 };
        for (var block = blocks - 1; block > firstData; block--)
        {
            if (cache.Count == 50)
            {
                var link = new byte[512]; U16(link, 0, cache.Count);
                for (var i = 0; i < cache.Count; i++) U32(link, 2 + i * 4, cache[i]);
                volume.Position = block * 512L; volume.Write(link); cache.Clear();
            }
            cache.Add(block);
        }
        var super = new byte[512]; U16(super, 0, firstData); U32(super, 2, blocks); U16(super, 6, cache.Count);
        for (var i = 0; i < cache.Count; i++) U32(super, 8 + i * 4, cache[i]);
        var freeInodes = Math.Min(100, inodes - 2); U16(super, 208, freeInodes);
        for (var i = 0; i < freeInodes; i++) U16(super, 210 + i * 2, i + 3);
        U32(super, 418, blocks - (uint)firstData - 1); U16(super, 422, inodes - 2);
        var inodeTable = new byte[inodes * 64];
        U16(inodeTable, 0, 0x8000); U16(inodeTable, 2, 1); // inode 1: empty bad-block file
        U16(inodeTable, 64, 0x41ed); U16(inodeTable, 66, 2); U32(inodeTable, 72, 32);
        var pointer = inodeTable.AsSpan(76, 3);
        if (order == StorageByteOrder.BigEndian)
        { pointer[0] = (byte)(firstData >> 16); pointer[1] = (byte)(firstData >> 8); pointer[2] = (byte)firstData; }
        else if (order == StorageByteOrder.PdpEndian)
        { pointer[0] = (byte)(firstData >> 16); pointer[1] = (byte)firstData; pointer[2] = (byte)(firstData >> 8); }
        else
        { pointer[0] = (byte)firstData; pointer[1] = (byte)(firstData >> 8); pointer[2] = (byte)(firstData >> 16); }
        var root = new byte[512]; U16(root, 0, 2); root[2] = (byte)'.';
        U16(root, 16, 2); root[18] = root[19] = (byte)'.';
        volume.Position = 0; volume.Write(new byte[512]); volume.Write(super); volume.Write(inodeTable); volume.Write(root); volume.Flush();
    }
}
