namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.IO;
    using Blocks;
    using Core.Converters;

    public static class Pfs3IndexBlockReader
    {
        public static Pfs3IndexBlock Parse(byte[] blockBytes, Pfs3GlobalData g)
        {
            var id = BigEndianConverter.ConvertBytesToUInt16(blockBytes);
            if (id != Pfs3Constants.IBLKID && id != Pfs3Constants.SBLKID && id != Pfs3Constants.BMIBLKID)
            {
                throw new IOException($"Invalid index block id '{id}'");
            }

            var datestamp = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x4);
            var seqNr = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x8);

            var offset = 0xc;
            var indexCount = (blockBytes.Length - Amiga.SizeOf.UWord * 2 - Amiga.SizeOf.ULong * 2) / Amiga.SizeOf.Long;
            var indexes = new int[indexCount];
            for (var i = 0; i < indexCount; i++)
            {
                indexes[i] = BigEndianConverter.ConvertBytesToInt32(blockBytes, offset);
                offset += Amiga.SizeOf.Long;
            }

            return new Pfs3IndexBlock(g)
            {
                id = id,
                datestamp = datestamp,
                seqnr = seqNr,
                index = indexes
            };
        }
    }
}
