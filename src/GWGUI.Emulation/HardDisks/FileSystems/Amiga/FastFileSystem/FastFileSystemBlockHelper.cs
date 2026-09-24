namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Amiga.Extensions;
    using Blocks;

    public static class FastFileSystemBlockHelper
    {
        public static int CalculateOffsetsPerBitmapBlockCount(uint fileSystemBlockSize)
        {
            // calculate bitmaps per bitmap blocks count
            return Convert.ToInt32((fileSystemBlockSize - SizeOf.Long) / SizeOf.Long);
        }

        public static int CalculateBitmapsPerBitmapBlockCount(uint fileSystemBlockSize)
        {
            // calculate bitmaps per bitmap blocks count
            return Convert.ToInt32(CalculateOffsetsPerBitmapBlockCount(fileSystemBlockSize) *
                                   FastFileSystemConstants.BitmapsPerULong);
        }

        public static int CalculateBitmapBlockOffsetsPerBitmapExtensionBlock(uint fileSystemBlockSize)
        {
            return Convert.ToInt32((fileSystemBlockSize - SizeOf.Long) / SizeOf.Long);
        }

        public static IEnumerable<FastFileSystemBitmapBlock> CreateBitmapBlocks(uint lowCyl, uint highCyl, uint heads,
            uint blocksPerTrack, uint blockSize, uint fileSystemBlockSize)
        {
            // calculate blocks count
            var cylinders = highCyl - lowCyl + 1;
            var blocksCount = cylinders * heads * blocksPerTrack / (fileSystemBlockSize / blockSize);

            var bitmapsPerBitmapBlockCount = CalculateBitmapsPerBitmapBlockCount(fileSystemBlockSize);

            for (var b = 0; b < blocksCount; b += bitmapsPerBitmapBlockCount)
            {
                var map = new List<uint>();

                var bitmapsInBitmapBlock = Math.Min(blocksCount - b, bitmapsPerBitmapBlockCount);
                for (var m = 0; m < bitmapsInBitmapBlock; m += FastFileSystemConstants.BitmapsPerULong)
                {
                    if (m + FastFileSystemConstants.BitmapsPerULong > bitmapsInBitmapBlock)
                    {
                        var bitmaps = 0U;
                        var lastBlocks = bitmapsInBitmapBlock - m;
                        for (var i = 0; i < lastBlocks; i++)
                        {
                            bitmaps |= 1U << i;
                        }
                        map.Add(bitmaps);
                        continue;
                    }

                    map.Add(uint.MaxValue);
                }

                yield return new FastFileSystemBitmapBlock((int)fileSystemBlockSize)
                {
                    Map = map.ToArray()
                };
            }
        }

        public static IEnumerable<FastFileSystemBitmapExtensionBlock> CreateBitmapExtensionBlocks(
            IEnumerable<FastFileSystemBitmapBlock> bitmapBlocks, uint fileSystemBlockSize)
        {
            // calculate pointers per bitmap extension block based on block size - next pointer
            var pointersPerBitmapExtensionBlock =
                Convert.ToInt32((fileSystemBlockSize - SizeOf.Long) / SizeOf.Long);

            // chunk bitmap blocks
            var bitmapBlockChunks = new List<FastFileSystemBitmapExtensionBlock>();
            bitmapBlocks.ChunkBy(pointersPerBitmapExtensionBlock, blocks => bitmapBlockChunks.Add(
                new FastFileSystemBitmapExtensionBlock
                {
                    BitmapBlocks = blocks.ToList(),
                }));

            return bitmapBlockChunks;
        }

        public static IEnumerable<FastFileSystemBitmapExtensionBlock> CreateBitmapExtensionBlocks(
            IEnumerable<FastFileSystemBitmapBlock> bitmapBlocks, uint fileSystemBlockSize, uint bitmapExtensionBlockOffset)
        {
            // calculate number of offsets stored in bitmap extension block
            var offsetsPerBitmapExtensionBlock = Convert.ToInt32((fileSystemBlockSize - 4) / 4);
            var currentBitmapExtensionBlockOffset = bitmapExtensionBlockOffset;

            var bitmapBlockChunks = new List<IEnumerable<FastFileSystemBitmapBlock>>();
            bitmapBlocks.ChunkBy(offsetsPerBitmapExtensionBlock, blocks => bitmapBlockChunks.Add(blocks.ToList()));
            for (var i = 0; i < bitmapBlockChunks.Count; i++)
            {
                var bitmapBlockChunk = bitmapBlockChunks[i].ToList();

                var nextBitmapExtensionBlockOffset =
                    FastFileSystemOffsetHelper.SetBitmapBlockOffsets(bitmapBlockChunk, currentBitmapExtensionBlockOffset + 1) + 1;

                yield return new FastFileSystemBitmapExtensionBlock
                {
                    Offset = currentBitmapExtensionBlockOffset,
                    BitmapBlocks = bitmapBlockChunk,
                    NextBitmapExtensionBlockPointer =
                        i < bitmapBlockChunks.Count - 1 ? nextBitmapExtensionBlockOffset : 0
                };

                currentBitmapExtensionBlockOffset = nextBitmapExtensionBlockOffset;
            }
        }

        public static void UpdateBitmaps(IEnumerable<FastFileSystemBitmapBlock> bitmapBlocks,
            IDictionary<uint, bool> blocksFreeMap, uint reserved, uint fileSystemBlockSize)
        {
            var bitmapBlocksList = bitmapBlocks.ToList();
            var bitmapsPerBitmapBlockCount = CalculateBitmapsPerBitmapBlockCount(fileSystemBlockSize);

            foreach (var entry in blocksFreeMap)
            {
                var bitmapBlockIndex = Convert.ToInt32((entry.Key - reserved) / bitmapsPerBitmapBlockCount);
                var blockIndex = (int)((entry.Key - reserved) % bitmapsPerBitmapBlockCount);

                FastFileSystemMapBlockHelper.SetBlock(bitmapBlocksList[bitmapBlockIndex], blockIndex,
                    entry.Value ? FastFileSystemBitmapBlock.FastFileSystemBlockState.Free : FastFileSystemBitmapBlock.FastFileSystemBlockState.Used);
            }
        }

        public static async Task<IEnumerable<FastFileSystemBitmapBlock>> ReadBitmapBlocks(FastFileSystemVolumeState volume,
            IEnumerable<uint> bitmapBlockOffsets)
        {
            var bitmapBlocks = new List<FastFileSystemBitmapBlock>();
            foreach (var bitmapBlockOffset in bitmapBlockOffsets)
            {
                var bitmapBlock = await FastFileSystemDisk.ReadBitmapBlock(volume, bitmapBlockOffset);
                bitmapBlock.Offset = bitmapBlockOffset;

                bitmapBlocks.Add(bitmapBlock);
            }

            return bitmapBlocks;
        }

        public static async Task<IEnumerable<FastFileSystemBitmapExtensionBlock>> ReadBitmapExtensionBlocks(FastFileSystemVolumeState volume,
            uint bitmapExtensionBlocksOffset)
        {
            var bitmapExtensionBlocks = new List<FastFileSystemBitmapExtensionBlock>();

            while (bitmapExtensionBlocksOffset != 0)
            {
                var bitmapExtensionBlock = await FastFileSystemDisk.ReadBitmapExtensionBlock(volume, bitmapExtensionBlocksOffset);
                bitmapExtensionBlock.BitmapBlocks =
                    await ReadBitmapBlocks(volume, bitmapExtensionBlock.BitmapBlockOffsets);

                bitmapExtensionBlocks.Add(bitmapExtensionBlock);

                bitmapExtensionBlocksOffset = bitmapExtensionBlock.NextBitmapExtensionBlockPointer;
            }

            return bitmapExtensionBlocks;
        }
    }
}
