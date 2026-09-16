namespace Hst.Amiga.FileSystems.FastFileSystem.Blocks
{
    using System.IO;
    using Amiga.Extensions;
    using Core.Converters;

    /// <summary>
    /// Reader for long name file system comment block (LNFS) present in DOS\6 and DOS\7
    /// </summary>
    public static class FastFileSystemLongNameFileSystemCommentBlockReader
    {
        public static FastFileSystemLongNameFileSystemCommentBlock Parse(byte[] blockBytes)
        {
            var type = BigEndianConverter.ConvertBytesToInt32(blockBytes);

            if (type != FastFileSystemConstants.TYPE_COMMENT)
            {
                throw new IOException("Invalid long name file system comment block type");
            }

            /* Set to comment block's own block number */
            var ownKey = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x4);

            /* The number of the directory entry block which the comment is associated with */
            var headerKey = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x8);

            // long * 2: spare 1 / not used, must be set to zero

            var checksum = BigEndianConverter.ConvertBytesToInt32(blockBytes, 0x14);

            var calculatedChecksum = ChecksumHelper.CalculateChecksum(blockBytes, 0x14);
            if (checksum != calculatedChecksum)
            {
                throw new IOException("Invalid long name file system comment block checksum");
            }

            var comment = blockBytes.ReadStringWithLength(0x18, FastFileSystemConstants.MAXCMMTLEN);

            return new FastFileSystemLongNameFileSystemCommentBlock
            {
                BlockBytes = blockBytes,
                Type = type,
                OwnKey = ownKey,
                HeaderKey = headerKey,
                Checksum = checksum,
                Comment = comment
            };
        }
    }
}
