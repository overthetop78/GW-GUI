namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.Collections.Generic;
    using System.IO;
    using Blocks;
    using Core.Converters;

    public static class Pfs3DirBlockReader
    {
        public static Pfs3DirBlock Parse(byte[] blockBytes, Pfs3GlobalData g)
        {
            var id = BigEndianConverter.ConvertBytesToUInt16(blockBytes);
            if (id != Pfs3Constants.DBLKID)
            {
                throw new IOException($"Invalid dir block id '{id}'");
            }

            var notUsed = BigEndianConverter.ConvertBytesToUInt16(blockBytes, 0x2); // 0x2, not used
            var datestamp = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x4); // 0x4

            // not_used_2, offset 0x8 + 0xa

            var anodenr = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0xc); // 12
            var parent = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x10); // 16

            var maxDirEntries = (Pfs3SizeOf.Pfs3DirBlockSize.Entries(g) / Pfs3SizeOf.Pfs3DirEntrySize.Struct) + 5;
            var offset = 0x14;
            var dirEntriesNo = 0;
            var dirEntries = new List<Pfs3DirEntry>(maxDirEntries);

            do
            {
                var entry = Pfs3DirEntryReader.Read(blockBytes, offset, g);
                if (entry.Next == 0)
                {
                    break;
                }

                dirEntries.Add(entry);
                dirEntriesNo++;
                if (dirEntriesNo >= maxDirEntries)
                {
                    throw new IOException($"Read entries from dir block exceeded max entries, possibly corrupt dir block");
                }
                offset += entry.Next;
            } while (offset < blockBytes.Length);

            return new Pfs3DirBlock(g)
            {
                id = id,
                not_used_1 = notUsed,
                datestamp = datestamp,
                anodenr = anodenr,
                parent = parent,
                DirEntries = dirEntries
            };
        }
    }
}
