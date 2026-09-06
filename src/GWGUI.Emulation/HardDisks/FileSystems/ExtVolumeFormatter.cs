using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>Shared block-group and inode construction for explicit ext format profiles.</summary>
internal static class ExtVolumeFormatter
{
    private const int Block = 4096, BlocksPerGroup = 32768, InodesPerGroup = 1024, InodeTableBlocks = 32;
    public static void Validate(long capacity, string label)
    {
        if (capacity < 8L << 20 || capacity > 128L << 30 || capacity % Block != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (label.Any(c => c < 32 || c > 126) || label.Length > 16) throw new ArgumentException("Invalid ext2 label.");
        var blocks = capacity / Block; var groups = (blocks + BlocksPerGroup - 1) / BlocksPerGroup;
        var descriptors = (groups * 32 + Block - 1) / Block;
        if (blocks - (groups - 1) * BlocksPerGroup <= 3 + descriptors + InodeTableBlocks)
            throw new ArgumentException("The last ext2 group cannot hold its metadata.");
    }
    public static void Format(Stream volume, string label, bool journal, bool extents)
    {
        Validate(volume.Length, label);
        var blocks = checked((uint)(volume.Length / Block));
        var groups = (int)((blocks + BlocksPerGroup - 1) / BlocksPerGroup);
        var descriptorBlocks = (groups * 32 + Block - 1) / Block;
        var metadataBlocks = 3 + descriptorBlocks + InodeTableBlocks;
        var journalBlocks = journal ? ExtJournalWriter.BlockCount + (extents ? 0 : 1) : 0;
        var descriptors = new byte[descriptorBlocks * Block];
        var freeBlocks = blocks - (uint)(groups * metadataBlocks + journalBlocks) - 1;
        var super = new byte[1024];
        U32(super, 0, (uint)(groups * InodesPerGroup)); U32(super, 4, blocks); U32(super, 12, freeBlocks);
        U32(super, 16, (uint)(groups * InodesPerGroup - 10)); U32(super, 24, 2); U32(super, 28, 2);
        U32(super, 32, BlocksPerGroup); U32(super, 36, BlocksPerGroup); U32(super, 40, InodesPerGroup);
        U16(super, 54, 20); U16(super, 56, 0xef53); U16(super, 58, 1); U16(super, 60, 1);
        U32(super, 76, 1); U32(super, 84, 11); U16(super, 88, 128); U32(super, 96, 2);
        Guid.NewGuid().ToByteArray().CopyTo(super, 104); Encoding.ASCII.GetBytes(label).CopyTo(super, 120);
        if (journal) { U32(super, 92, 4); U32(super, 224, 8); }
        if (extents) U32(super, 96, 0x42);
        for (var group = 0; group < groups; group++)
        {
            var first = (uint)(group * BlocksPerGroup); var count = Math.Min(BlocksPerGroup, blocks - first);
            var bitmap = first + 1 + (uint)descriptorBlocks; var inodeBitmap = bitmap + 1; var inodeTable = bitmap + 2;
            var used = metadataBlocks + (group == 0 ? 1 + journalBlocks : 0);
            var offset = group * 32;
            U32(descriptors, offset, bitmap); U32(descriptors, offset + 4, inodeBitmap); U32(descriptors, offset + 8, inodeTable);
            U16(descriptors, offset + 12, (int)count - used); U16(descriptors, offset + 14, InodesPerGroup - (group == 0 ? 10 : 0));
            U16(descriptors, offset + 16, group == 0 ? 1 : 0);
            var allocation = new byte[Block];
            for (var bit = 0; bit < BlocksPerGroup; bit++)
                if (bit < used || bit >= count) allocation[bit / 8] |= (byte)(1 << (bit % 8));
            Put(volume, (long)bitmap * Block, allocation);
            var inodes = new byte[Block];
            for (var bit = 0; bit < Block * 8; bit++)
                if (bit >= InodesPerGroup || (group == 0 && bit < 10)) inodes[bit / 8] |= (byte)(1 << (bit % 8));
            Put(volume, (long)inodeBitmap * Block, inodes);
            var inodeData = new byte[InodeTableBlocks * Block];
            if (group == 0)
            {
                U16(inodeData, 128, 0x41ed); U32(inodeData, 132, Block); U16(inodeData, 154, 2);
                U32(inodeData, 156, Block / 512); U32(inodeData, 168, (uint)metadataBlocks);
                if (extents) ExtJournalWriter.SetExtent(inodeData.AsSpan(128, 128), (uint)metadataBlocks, 1);
                if (journal)
                {
                    ExtJournalWriter.Write(volume, (uint)metadataBlocks + 1, inodeData.AsSpan(7 * 128, 128), super.AsSpan(104, 16), extents);
                    super[253] = 1;
                    inodeData.AsSpan(7 * 128 + 40, 60).CopyTo(super.AsSpan(268));
                    U32(super, 268 + 16 * 4, ExtJournalWriter.BlockCount * Block);
                }
                var root = new byte[Block]; U32(root, 0, 2); U16(root, 4, 12); root[6] = 1; root[7] = 2; root[8] = (byte)'.';
                U32(root, 12, 2); U16(root, 16, Block - 12); root[18] = 2; root[19] = 2; root[20] = root[21] = (byte)'.';
                Put(volume, (long)metadataBlocks * Block, root);
            }
            Put(volume, (long)inodeTable * Block, inodeData);
        }
        for (var group = 0; group < groups; group++)
        {
            var first = (long)group * BlocksPerGroup * Block;
            Put(volume, first, new byte[Block]); U16(super, 90, group);
            Put(volume, first + (group == 0 ? 1024 : 0), super);
            Put(volume, first + Block, descriptors);
        }
        volume.Flush();
    }
    private static void U16(byte[] bytes, int offset, int value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), checked((ushort)value));
    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
    private static void Put(Stream stream, long offset, byte[] bytes) { stream.Position = offset; stream.Write(bytes); }
}
