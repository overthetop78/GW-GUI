namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;
    using Core.Extensions;

    public static partial class Pfs3Disk
    {
        // disk.c

        public static void BoundsCheck(bool write, uint blocknr, uint blocks, Pfs3GlobalData g)
        {
            if (!(Pfs3Macro.InPartition(blocknr, g) && Pfs3Macro.InPartition(blocknr + blocks - 1, g)))
            {
                // ULONG args[5];
                // args[0] = g->tdmode;
                // args[1] = blocknr;
                // args[2] = blocks;
                // args[3] = g->firstblock;
                // args[4] = g->lastblock;
                // ErrorMsg(write ? AFS_ERROR_WRITE_OUTSIDE : AFS_ERROR_READ_OUTSIDE, args, g);
                throw new IOException(write ? "AFS_ERROR_WRITE_OUTSIDE" : "AFS_ERROR_READ_OUTSIDE");
            }
        }

        public static async Task<byte[]> RawRead(uint blocks, uint blocknr, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: Raw read bytes from block nr {blocknr} with size of {blocks} blocks");
#endif

            if (blocknr == UInt32.MaxValue) // blocknr of uninitialised anode
            {
                return default;
            }

            blocknr += g.firstblock;

            if (g.softprotect)
            {
                throw new IOException("ERROR_DISK_WRITE_PROTECTED");
            }

            BoundsCheck(false, blocknr, blocks, g);

            // seek to block in stream
            // while (blocks > 0)
            // {
            //     var transfer = min(blocks,maxtransfer);
            //
            //     buffer += transfer << BLOCKSHIFT;
            //     blocks -= transfer;
            //     blocknr += transfer;
            // }

            var offset = (long)g.blocksize * blocknr;
            g.stream.Seek(offset, SeekOrigin.Begin);

            // read block bytes
            return await g.stream.ReadBytes((int)(g.blocksize * blocks));
        }

        public static async Task<IPfs3Block> RawRead<T>(uint blocks, uint blocknr, Pfs3GlobalData g) where T : IPfs3Block
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: Raw read block type '{typeof(T).Name}' from block nr {blocknr} with size of {blocks} blocks");
#endif

            var buffer = await RawRead(blocks, blocknr, g);

            var type = typeof(T);
            if (type == typeof(Pfs3AnodeBlock))
            {
                return Pfs3AnodeBlockReader.Parse(buffer, g);
            }

            if (type == typeof(Pfs3DirBlock))
            {
                return Pfs3DirBlockReader.Parse(buffer, g);
            }

            if (type == typeof(Pfs3IndexBlock))
            {
                return Pfs3IndexBlockReader.Parse(buffer, g);
            }

            if (type == typeof(Pfs3BitmapBlock))
            {
                return Pfs3BitmapBlockReader.Parse(buffer, (int)g.glob_allocdata.longsperbmb);
            }

            if (type == typeof(Pfs3DelDirBlock))
            {
                return Pfs3DelDirBlockReader.Parse(buffer, g);
            }

            if (type == typeof(Pfs3RootBlockExtension))
            {
                return Pfs3RootBlockExtensionReader.Parse(buffer);
            }

            return default;
        }

        public static async Task<bool> RawWrite(Stream stream, byte[] buffer, uint blocks, uint blocknr, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: Raw write bytes to block nr {blocknr} with buffer size {buffer.Length} and size of {blocks} blocks");
#endif
            // RawReadWrite_DS(TRUE, buffer, blocks, blocknr, g);
            if (g.softprotect)
            {
                throw new IOException("ERROR_DISK_WRITE_PROTECTED");
            }

            if (blocknr == UInt32.MaxValue) // blocknr of uninitialised anode
                return false;

            blocknr += g.firstblock;

            BoundsCheck(true, blocknr, blocks, g);

            var blockOffset = (long)g.blocksize * blocknr;

            g.stream.Seek(blockOffset, SeekOrigin.Begin);
            await stream.WriteAsync(buffer, 0, Convert.ToInt32(Math.Min(buffer.Length, g.blocksize * blocks)));

            return true;
        }

        public static async Task<bool> RawWrite(Stream stream, IPfs3Block block, uint blocks, uint blocknr, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: Raw write block type '{block.GetType().Name}' to block nr {blocknr} with size of {blocks} blocks");
#endif

            byte[] buffer;
            switch (block)
            {
                case Pfs3AnodeBlock anodeBlock:
                    buffer = Pfs3AnodeBlockWriter.BuildBlock(anodeBlock, g);
                    break;
                case Pfs3DirBlock dirBlock:
                    buffer = Pfs3DirBlockWriter.BuildBlock(dirBlock, g);
                    break;
                case Pfs3IndexBlock indexBlock:
                    buffer = Pfs3IndexBlockWriter.BuildBlock(indexBlock, g);
                    break;
                case Pfs3BitmapBlock bitmapBlock:
                    buffer = Pfs3BitmapBlockWriter.BuildBlock(bitmapBlock, g);
                    break;
                case Pfs3DelDirBlock delDirBlock:
                    buffer = Pfs3DelDirBlockWriter.BuildBlock(delDirBlock, g);
                    break;
                case Pfs3RootBlockExtension rootBlockExtension:
                    buffer = Pfs3RootBlockExtensionWriter.BuildBlock(rootBlockExtension, g);
                    break;
                default:
                    return false;
            }

            return await RawWrite(stream, buffer, blocks, blocknr, g);
        }

/* write all dirty blocks to disk
 */
        public static async Task UpdateDataCache(Pfs3GlobalData g)
        {
            int i;

            for (i = 0; i < g.dc.size; i++)
            {
                if (g.dc.ref_[i].dirty && g.dc.ref_[i].blocknr != 0)
                    await UpdateSlot(i, g);
            }
        }

/* update a data cache slot, and any adjacent blocks
 */
        public static async Task UpdateSlot(int slotnr, Pfs3GlobalData g)
        {
            uint blocknr;
            int i;

            blocknr = g.dc.ref_[slotnr].blocknr;

            /* find out how many adjacent blocks can be written */
            for (i = slotnr; i < g.dc.size; i++)
            {
                if (g.dc.ref_[i].blocknr != blocknr++)
                    break;
                g.dc.ref_[i].dirty = false;
            }

            /* write them */
            //await RawWrite(g.dc.data[slotnr << g.blockshift], i-slotnr, g.dc.ref_[slotnr].blocknr, g);
            var slotData = new byte[(int)(g.blocksize * (i - slotnr))];

            var di = slotnr << g.blockshift;
            for (var si = 0; si < slotData.Length; si++)
            {
                slotData[si] = g.dc.data[di++];
            }
            await RawWrite(g.stream, slotData, (uint)(i - slotnr), g.dc.ref_[slotnr].blocknr, g);
        }

        /* SeekInFile
**
** Specification:
**
** - set fileposition
** - if wrong position, resultposition unknown and error
** - result = old position to start of file, -1 = error
**
** - the end of the file is 0 from end
*/
    }
}
