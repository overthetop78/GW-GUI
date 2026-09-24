namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.Collections.Generic;
    using System.IO;
    using Blocks;
    using Core.Converters;

    public static class Pfs3AnodeBlockReader
    {
        public static Pfs3AnodeBlock Parse(byte[] blockBytes, Pfs3GlobalData g)
        {
            var id = BigEndianConverter.ConvertBytesToUInt16(blockBytes);
            if (id != Pfs3Constants.ABLKID)
            {
                throw new IOException($"Invalid anode block id '{id}'");
            }

            var datestamp = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x4);
            var seqNr = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x8);

            var nodes = new List<Pfs3Anode>();
            var nodesCount = (g.RootBlock.ReservedBlksize - Amiga.SizeOf.UWord * 2 - Amiga.SizeOf.ULong * 3) /
                             (Amiga.SizeOf.ULong * 3);

            var offset = 0x10;
            for (var i = 0; i < nodesCount; i++)
            {
                var clusterSize = BigEndianConverter.ConvertBytesToUInt32(blockBytes, offset);
                var blockNr = BigEndianConverter.ConvertBytesToUInt32(blockBytes, offset + 0x4);
                var next = BigEndianConverter.ConvertBytesToUInt32(blockBytes, offset + 0x8);

                nodes.Add(new Pfs3Anode
                {
                    clustersize = clusterSize,
                    blocknr = blockNr,
                    next = next
                });

                offset += 3 * Amiga.SizeOf.ULong;
            }

            return new Pfs3AnodeBlock(g)
            {
                id = id,
                datestamp = datestamp,
                seqnr = seqNr,
                nodes = nodes.ToArray()
            };
        }
    }
}
