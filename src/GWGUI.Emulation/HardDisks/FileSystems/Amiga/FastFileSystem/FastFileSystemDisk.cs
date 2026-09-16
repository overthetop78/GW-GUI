namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;

    public static class FastFileSystemDisk
    {
        private static async Task<byte[]> ReadBlockBytes(FastFileSystemVolumeState volume, uint sector)
        {
            var blockOffset = volume.PartitionStartOffset + sector * volume.FileSystemBlockSize;
            volume.Stream.Seek(blockOffset, SeekOrigin.Begin);
            return await Amiga.AmigaDisk.ReadBlock(volume.Stream, volume.FileSystemBlockSize);
        }

        private static async Task WriteBlockBytes(FastFileSystemVolumeState volume, uint sector, byte[] blockBytes)
        {
            var blockOffset = volume.PartitionStartOffset + sector * volume.FileSystemBlockSize;
            volume.Stream.Seek(blockOffset, SeekOrigin.Begin);
            await Amiga.AmigaDisk.WriteBlock(volume.Stream, blockBytes);
        }

        public static async Task<FastFileSystemRootBlock> ReadRootBlock(FastFileSystemVolumeState volume, uint sector)
        {
            var blockBytes = await ReadBlock(volume, sector);
            return FastFileSystemRootBlockParser.Parse(blockBytes);
        }

        public static async Task WriteRootBlock(FastFileSystemVolumeState volume, uint nSect, FastFileSystemRootBlock root)
        {
            var blockBytes = FastFileSystemRootBlockBuilder.Build(root, volume.FileSystemBlockSize);
            await WriteBlock(volume, nSect, blockBytes);
        }

        public static async Task<FastFileSystemBitmapBlock> ReadBitmapBlock(FastFileSystemVolumeState volume, uint sector)
        {
            var blockBytes = await ReadBlock(volume, sector);
            return FastFileSystemBitmapBlockParser.Parse(blockBytes);
        }

        public static async Task WriteBitmapBlock(FastFileSystemVolumeState vol, uint nSect, FastFileSystemBitmapBlock bitmapBlock)
        {
            var blockBytes = FastFileSystemBitmapBlockBuilder.Build(bitmapBlock, vol.FileSystemBlockSize);
            await WriteBlock(vol, nSect, blockBytes);
        }

        public static async Task<FastFileSystemBitmapExtensionBlock> ReadBitmapExtensionBlock(FastFileSystemVolumeState volume, uint sector)
        {
            var blockBytes = await ReadBlock(volume, sector);
            return FastFileSystemBitmapExtensionBlockParser.Parse(blockBytes);
        }

        public static async Task<FastFileSystemEntryBlock> ReadEntryBlock(FastFileSystemVolumeState volume, uint sector)
        {
            var blockBytes = await ReadBlock(volume, sector);
            var entryBlock = FastFileSystemEntryBlockParser.Parse(blockBytes, volume.UseLnfs);

            if (volume.UseLnfs && entryBlock.CommentBlock != 0)
            {
                var commentBlockBytes = await ReadBlock(volume, entryBlock.CommentBlock);
                var commentBlock = FastFileSystemLongNameFileSystemCommentBlockReader.Parse(commentBlockBytes);
                entryBlock.Comment = commentBlock.Comment;
            }

            return entryBlock;
        }

        public static async Task<FastFileSystemDataBlock> ReadDataBlock(FastFileSystemVolumeState vol, uint nSect)
        {
            var blockBytes = await ReadBlock(vol, nSect);

            if (vol.UseOfs)
            {
                var dBlock = FastFileSystemDataBlockParser.Parse(blockBytes);
                if (!IsSectorNumberValid(vol, dBlock.HeaderKey))
                    throw new IOException("headerKey out of range");
                if (!IsSectorNumberValid(vol, dBlock.NextData))
                    throw new IOException("nextData out of range");

                return dBlock;
            }

            return new FastFileSystemDataBlock
            {
                BlockBytes = blockBytes,
                Data = blockBytes
            };
        }

        public static async Task WriteDataBlock(FastFileSystemVolumeState volume, uint nSect, FastFileSystemDataBlock dataBlock)
        {
            var blockBytes = volume.UseOfs
                ? FastFileSystemDataBlockBuilder.Build(dataBlock, volume.FileSystemBlockSize)
                : dataBlock.Data;
            await WriteBlock(volume, nSect, blockBytes);
        }

        public static async Task<FastFileSystemFileExtBlock> ReadFileExtBlock(FastFileSystemVolumeState volume, uint nSect)
        {
            var blockBytes = await ReadBlock(volume, nSect);
            var fileExtBlock = FastFileSystemFileExtBlockParser.Parse(blockBytes);

            if (fileExtBlock.HeaderKey != nSect)
            {
                throw new IOException("Header key not equal to sector");
            }

            if (fileExtBlock.HighSeq > volume.IndexSize)
            {
                throw new IOException("High seq out of range");
            }

            if (!IsSectorNumberValid(volume, fileExtBlock.Parent))
            {
                throw new IOException("Parent out of range");
            }

            if (fileExtBlock.Extension != 0 && !IsSectorNumberValid(volume, fileExtBlock.Extension))
            {
                throw new IOException("Extension out of range");
            }

            return fileExtBlock;
        }

        public static async Task WriteEntryBlock(FastFileSystemVolumeState vol, uint nSect, FastFileSystemEntryBlock entryBlock)
        {
            var blockBytes = FastFileSystemEntryBlockBuilder.Build(entryBlock, vol.FileSystemBlockSize, vol.UseLnfs);
            await WriteBlock(vol, nSect, blockBytes);
        }

        public static async Task WriteFileExtBlock(FastFileSystemVolumeState vol, uint nSect, FastFileSystemFileExtBlock fileExtBlock)
        {
            var blockBytes = FastFileSystemFileExtBlockBuilder.Build(fileExtBlock, vol.FileSystemBlockSize);
            await WriteBlock(vol, nSect, blockBytes);
        }

        public static async Task<FastFileSystemDirCacheBlock> ReadDirCacheBlock(FastFileSystemVolumeState vol, uint nSect)
        {
            var blockBytes = await ReadBlock(vol, nSect);

            var dirCacheBlock = FastFileSystemDirCacheBlockParser.Parse(blockBytes);
            if (dirCacheBlock.HeaderKey != nSect)
            {
                throw new IOException($"Invalid dir cache block header key '{dirCacheBlock.HeaderKey}' is not equal to sector {nSect}");
            }

            return dirCacheBlock;
        }

        public static async Task WriteDirCacheBlock(FastFileSystemVolumeState vol, uint nSect, FastFileSystemDirCacheBlock dirCacheBlock)
        {
            dirCacheBlock.HeaderKey = nSect;

            var blockBytes = FastFileSystemDirCacheBlockBuilder.Build(dirCacheBlock, vol.FileSystemBlockSize);
            await WriteBlock(vol, nSect, blockBytes);
        }

        /// <summary>
        /// Is sector number valid
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="logicalSector"></param>
        /// <returns>True, if logical sector number is within volume first and last block. Otherwise false.</returns>
        public static bool IsSectorNumberValid(FastFileSystemVolumeState volume, uint logicalSector)
        {
            return logicalSector <= volume.LastBlock - volume.FirstBlock;
        }

        public static void ThrowExceptionIfSectorNumberInvalid(FastFileSystemVolumeState volume, uint logicalSector)
        {
            if (IsSectorNumberValid(volume, logicalSector))
            {
                return;
            }

            throw new IOException($"Logical sector '{logicalSector}' is out of range");
        }

        /// <summary>
        /// Read block
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="logicalSector">Logical block number</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public static async Task<byte[]> ReadBlock(FastFileSystemVolumeState volume, uint logicalSector)
        {
            if (!volume.Mounted)
            {
                throw new IOException("FastFileSystemVolumeState is not mounted");
            }

            // translate logical sector to physical sector
            var physicalSector = logicalSector + volume.FirstBlock;
            if (physicalSector < volume.FirstBlock || physicalSector > volume.LastBlock)
            {
                throw new IOException($"Logical sector '{logicalSector}' is out of range");
            }

            return await ReadBlockBytes(volume, physicalSector);
        }

        public static async Task WriteBlock(FastFileSystemVolumeState volume, uint logicalSector, byte[] blockBytes)
        {
            if (!volume.Mounted)
            {
                throw new IOException("FastFileSystemVolumeState is not mounted");
            }

            if (volume.ReadOnly)
            {
                throw new IOException("FastFileSystemVolumeState is mounted read only");
            }

            // translate logical sector to physical sector
            var physicalSector = logicalSector + volume.FirstBlock;

            if (physicalSector < volume.FirstBlock || physicalSector > volume.LastBlock)
            {
                throw new IOException($"Logical sector '{logicalSector}' is out of range");
            }

            await WriteBlockBytes(volume, physicalSector, blockBytes);
        }
    }
}
