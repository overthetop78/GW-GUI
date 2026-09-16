namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using RigidDiskBlocks;

    public static class Pfs3Helper
    {
        public static async Task<Pfs3GlobalData> Mount(Stream stream, PartitionBlock partitionBlock)
        {
            return await Mount(stream, partitionBlock.Sectors, partitionBlock.BlocksPerTrack,
                partitionBlock.Surfaces, partitionBlock.LowCyl, partitionBlock.HighCyl, partitionBlock.NumBuffer,
                partitionBlock.BlockSize, partitionBlock.Mask);
        }

        public static async Task<Pfs3GlobalData> Mount(Stream stream, uint sectors, uint blocksPerTrack, uint surfaces, uint lowCyl,
            uint highCyl, uint numBuffer, uint blockSize, uint mask)
        {
            var g = Pfs3Init.CreateGlobalData(sectors, blocksPerTrack, surfaces, lowCyl, highCyl, numBuffer, mask);
            g.stream = stream;

            Pfs3Init.Initialize(g);

            var rootBlock = await Pfs3VolumeOperations.GetCurrentRoot(g);

            await Pfs3VolumeOperations.DiskInsertSequence(rootBlock, g);

            return g;
        }

        public static async Task Flush(Pfs3GlobalData g)
        {
            if (g.stream.CanWrite)
            {
                await Pfs3Update.UpdateDisk(g);
            }

            Pfs3VolumeOperations.FreeVolumeResources(g.currentvolume, g);
            await g.stream.FlushAsync();
        }

        public static int CalculateBitmapBlocksCount(int bitmapsCount, Pfs3GlobalData g)
        {
            var bitmapsPerBlock = g.blocksize / Amiga.SizeOf.ULong;
            var bitmapsPerFirstBlock = (g.blocksize - (Amiga.SizeOf.UWord * 2) - (Amiga.SizeOf.ULong * 2)) / Amiga.SizeOf.ULong;

            return bitmapsCount > bitmapsPerFirstBlock
                ? Convert.ToInt32(Math.Ceiling((double)(bitmapsCount - bitmapsPerFirstBlock) / bitmapsPerBlock) + 1)
                : 1;
        }
    }
}
