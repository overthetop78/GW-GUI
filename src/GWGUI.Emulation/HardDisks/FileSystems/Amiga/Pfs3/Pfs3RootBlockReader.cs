namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Blocks;
    using Core.Converters;

    public static class Pfs3RootBlockReader
    {
        public static Pfs3RootBlock Parse(byte[] blockBytes)
        {
            var diskType = BigEndianConverter.ConvertBytesToInt32(blockBytes);
            if (diskType != Pfs3Constants.ID_PFS_DISK && diskType != Pfs3Constants.ID_PFS2_DISK)
            {
                throw new IOException("Invalid root block");
            }

            var options = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 4); /* bit 0 is harddisk mode */
            var datestamp = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 8); /* current datestamp */
            var creationDate = Pfs3DateHelper.ReadDate(blockBytes, 0xc);
            var protection = BigEndianConverter.ConvertBytesToUInt16(blockBytes, 0x12); /* protection bits (ala ADOS)       */
            var diskNameLength = blockBytes[0x14]; /* disk label (pascal string)       */
            var diskName = AmigaTextHelper.GetString(blockBytes, 0x15, Math.Min((int)diskNameLength, 31));
            var lastReserved = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x34); /* reserved area. sector number of last reserved block */
            var firstReserved = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x38); /* sector number of first reserved block */
            var reservedFree = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x3c); /* number of reserved blocks (blksize blocks) free  */
            var reservedBlkSize = BigEndianConverter.ConvertBytesToUInt16(blockBytes, 0x40); /* size of reserved blocks in bytes */
            var rblkCluster = BigEndianConverter.ConvertBytesToUInt16(blockBytes, 0x42); /* number of sectors in rootblock, including bitmap  */
            var blocksFree = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x44); /* blocks free                      */
            var alwaysFree = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x48); /* minimum number of blocks free    */
            var rovingPtr = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x4c); /* current LONG bitmapfield nr for allocation       */
            var delDir = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x50); /* deldir location (<= 17.8)        */
            var diskSize = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x54); /* disksize in sectors              */
            var extension = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x58); /* rootblock extension (16.4)       offset=88 $58 */
            // 0x5c, not used

            var offset = 0x60;
            var idxUnion = new List<uint>();
            for (var i = 0; i < Pfs3SizeOf.Pfs3RootBlockSize.IdxUnion; i++)
            {
                idxUnion.Add(BigEndianConverter.ConvertBytesToUInt32(blockBytes, offset));
                offset += Amiga.SizeOf.ULong;
            }

            return new Pfs3RootBlock
            {
                DiskType = diskType,
                Options = (Pfs3RootBlock.Pfs3DiskOptions)options,
                Datestamp = datestamp,
                CreationDate = creationDate,
                Protection = protection,
                DiskName = diskName,
                LastReserved = lastReserved,
                FirstReserved = firstReserved,
                ReservedFree = reservedFree,
                ReservedBlksize = reservedBlkSize,
                RblkCluster = rblkCluster,
                BlocksFree = blocksFree,
                AlwaysFree = alwaysFree,
                RovingPtr = rovingPtr,
                DelDir = delDir,
                DiskSize = diskSize,
                Extension = extension,
                idx = new Pfs3RootBlockIndex(idxUnion.ToArray())
            };
        }
    }
}
