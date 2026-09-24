using System.Collections.Generic;

namespace Hst.Amiga.FileSystems.FastFileSystem.Blocks
{
    using System.IO;
    using Amiga.Extensions;
    using Core.Converters;

    public static class FastFileSystemEntryBlockParser
    {
        public static FastFileSystemEntryBlock Parse(byte[] blockBytes, bool useLnfs = false)
        {
            var type = BigEndianConverter.ConvertBytesToInt32(blockBytes);
            var secType = BigEndianConverter.ConvertBytesToInt32(blockBytes, blockBytes.Length - 0x4);

            if (type != FastFileSystemConstants.T_HEADER)
            {
                throw new IOException($"Invalid entry block type '{type}'");
            }

            switch (secType)
            {
                case FastFileSystemConstants.ST_ROOT:
                    return FastFileSystemRootBlockParser.Parse(blockBytes);
                case FastFileSystemConstants.ST_DIR:
                case FastFileSystemConstants.ST_FILE:
                case FastFileSystemConstants.ST_LDIR:
                case FastFileSystemConstants.ST_LFILE:
                    return useLnfs
                        ? FastFileSystemLongNameEntryBlockReader.Read(blockBytes)
                        : ParseEntryBlock(blockBytes);
                default:
                    throw new IOException($"Invalid entry block sec type '{secType}'");
            }
        }

        public static FastFileSystemEntryBlock ParseEntryBlock(byte[] blockBytes)
        {
            var type = BigEndianConverter.ConvertBytesToInt32(blockBytes);

            if (type != FastFileSystemConstants.T_HEADER)
            {
                throw new IOException("Invalid entry block type");
            }

            var headerKey = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x4);
            var highSeq = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x8);
            var firstData = BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x10);
            var checksum = BigEndianConverter.ConvertBytesToInt32(blockBytes, 0x14);

            var calculatedChecksum = ChecksumHelper.CalculateChecksum(blockBytes, 0x14);
            if (checksum != calculatedChecksum)
            {
                throw new IOException("Invalid entry block checksum");
            }

            var indexSize = FastFileSystemHelper.CalculateHashtableSize((uint)blockBytes.Length);
            var index = new List<uint>();
            for (var i = 0; i < indexSize; i++)
            {
                index.Add(BigEndianConverter.ConvertBytesToUInt32(blockBytes, 0x18 + (i * SizeOf.Long)));
            }

            var access = BigEndianConverter.ConvertBytesToUInt32(blockBytes, blockBytes.Length - 0xc0); // block size - 0xc0: access / protection
            var byteSize = BigEndianConverter.ConvertBytesToUInt32(blockBytes, blockBytes.Length - 0xbc); // block size - 0xbc: byte size
            var comment = blockBytes.ReadStringWithLength(blockBytes.Length - 0xb8, FastFileSystemConstants.MAXCMMTLEN); // block size - 0xb8: comment length (first byte) + comment max length 79 chars
            var date = DateHelper.ReadDate(blockBytes, blockBytes.Length - 0x5c); // block size - 0x5c: last access date
            var name = blockBytes.ReadStringWithLength(blockBytes.Length - 0x50, FastFileSystemConstants.MAXNAMELEN); // block size - 0x4f: name length (first byte) + name max length 30 chars
            var realEntry = BigEndianConverter.ConvertBytesToUInt32(blockBytes, blockBytes.Length - 0x2c); // block size - 0x2c: real_entry, FFS : pointer to "real" file or directory
            var nextLink = BigEndianConverter.ConvertBytesToUInt32(blockBytes, blockBytes.Length - 0x28); // block size - 0x28: next_link, FFS : hardlinks chained list (first=newest)
            var nextSameHash = BigEndianConverter.ConvertBytesToUInt32(blockBytes, blockBytes.Length - 0x10); // block size - 0x10: hash_chain, next entry ptr with same hash
            var parent = BigEndianConverter.ConvertBytesToUInt32(blockBytes, blockBytes.Length - 0x0c); // block size - 0x0c: parent directory
            var extension = BigEndianConverter.ConvertBytesToUInt32(blockBytes, blockBytes.Length - 0x08); // block size - 0x08: FFS : first directory cache block
            var secType = BigEndianConverter.ConvertBytesToInt32(blockBytes, blockBytes.Length - 0x04); // block size - 0x04: secondary type, eg. ST_USERDIR (== 2) for directory

            if (secType != FastFileSystemConstants.ST_FILE &&
                secType != FastFileSystemConstants.ST_DIR &&
                secType != FastFileSystemConstants.ST_LFILE &&
                secType != FastFileSystemConstants.ST_LDIR)
            {
                throw new IOException($"Invalid entry block sec type '{type}'");
            }

            return new FastFileSystemEntryBlock(blockBytes.Length)
            {
                BlockBytes = blockBytes,
                HeaderKey = headerKey,
                HighSeq = highSeq,
                FirstData = firstData,
                Checksum = checksum,
                IndexSize = indexSize,
                Index = index.ToArray(),
                Access = access,
                ByteSize = byteSize,
                Comment = comment,
                Date = date,
                Name = name,
                RealEntry = realEntry,
                NextLink = nextLink,
                NextSameHash = nextSameHash,
                Parent = parent,
                Extension = extension,
                SecType = secType
            };
        }
    }
}
