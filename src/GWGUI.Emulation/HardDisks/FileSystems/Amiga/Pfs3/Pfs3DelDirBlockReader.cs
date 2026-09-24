namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.Collections.Generic;
    using System.IO;
    using Blocks;
    using Core.Converters;

    public static class Pfs3DelDirBlockReader
    {
        public static Pfs3DelDirBlock Parse(byte[] blockBytes, Pfs3GlobalData g)
        {
            var id = BigEndianConverter.ConvertBytesToUInt16(blockBytes);
            if (id != Pfs3Constants.DELDIRID)
            {
                throw new IOException($"Invalid del dir block id '{id}'");
            }

            var datestamp = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x4);
            var seqnr = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x8);

            var uid = BigEndianConverter.ConvertBytesToUInt16(blockBytes, 0x12);
            var gid = BigEndianConverter.ConvertBytesToUInt16(blockBytes, 0x14);
            var protection = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x16);
            var creationDate = Pfs3DateHelper.ReadDate(blockBytes, 0x1a);

            var entries = new List<Pfs3DelDirEntry>();
            var offset = 0x20; // first del dir entry offset
            for (var i = 0; i < Pfs3SizeOf.Pfs3DelDirBlockSize.Entries(g); i++)
            {
                var delDirEntry = Pfs3DelDirEntryReader.Read(blockBytes, offset);
                entries.Add(delDirEntry);
                offset += Pfs3SizeOf.Pfs3DelDirEntrySize.Struct;
            }

            return new Pfs3DelDirBlock(g)
            {
                id = id,
                datestamp = datestamp,
                seqnr = seqnr,
                uid = uid,
                gid = gid,
                protection = protection,
                CreationDate = creationDate,
                entries = entries.ToArray()
            };
        }
    }
}
