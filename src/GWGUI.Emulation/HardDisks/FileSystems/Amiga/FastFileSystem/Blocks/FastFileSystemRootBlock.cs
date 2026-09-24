namespace Hst.Amiga.FileSystems.FastFileSystem.Blocks
{
    using System;
    using System.Collections.Generic;

    public class FastFileSystemRootBlock : FastFileSystemEntryBlock
    {
        public uint BitmapFlags { get; set; }
        public uint[] BitmapBlockOffsets { get; set; } // bmPages
        public uint BitmapBlocksOffset { get; set; }

        /// <summary>
        /// first bitmap extension block (when there's more than 25 bitmap blocks)
        /// </summary>
        public uint BitmapExtensionBlocksOffset { get; set; }

        public string DiskName => Name;
        public DateTime RootAlterationDate => Date;
        public DateTime DiskAlterationDate { get; set; }
        public DateTime FileSystemCreationDate { get; set; }

        public IEnumerable<FastFileSystemBitmapBlock> BitmapBlocks { get; set; }
        public IEnumerable<FastFileSystemBitmapExtensionBlock> BitmapExtensionBlocks { get; set; }

        public FastFileSystemRootBlock(int fileSystemBlockSize) : base(fileSystemBlockSize)
        {
            var hashtableSize = FastFileSystemHelper.CalculateHashtableSize((uint)fileSystemBlockSize);

            HeaderKey = 0;
            HighSeq = 0;
            HashTableSize = hashtableSize;
            FirstData = 0;
            Checksum = 0;
            HashTable = new uint[hashtableSize];

            BitmapFlags = FastFileSystemConstants.BM_VALID;// -1

            var now = DateTime.UtcNow;
            Date = now;
            DiskAlterationDate = now;
            FileSystemCreationDate = now;

            Extension = 0;

            BitmapBlockOffsets = Array.Empty<uint>();
            BitmapBlocks = new List<FastFileSystemBitmapBlock>();
            BitmapExtensionBlocks = new List<FastFileSystemBitmapExtensionBlock>();

            SecType = FastFileSystemConstants.ST_ROOT;
        }
    }
}
