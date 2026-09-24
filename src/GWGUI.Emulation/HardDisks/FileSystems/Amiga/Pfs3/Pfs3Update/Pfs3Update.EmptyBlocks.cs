namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;

    public static partial class Pfs3Update
    {
        public static void CommitReservedToBeFreed(Pfs3GlobalData g)
        {
            var alloc_data = g.glob_allocdata;

            int i;
            for (i = 0; i < alloc_data.rtbf_index; i++)
            {
                if (alloc_data.reservedtobefreed[i] != 0)
                {
                    Pfs3Allocation.FreeReservedBlock(alloc_data.reservedtobefreed[i], g);
                    alloc_data.reservedtobefreed[i] = 0;
                }
            }

            alloc_data.rtbf_index = 0;
        }

        public static async Task<bool> UpdateDirtyBlock(Pfs3CachedBlock blk, Pfs3GlobalData g)
        {
            // ULONG error;

            if (!g.updateok)
                return false;

            if (blk.changeflag)
            {
                    Pfs3Allocation.FreeReservedBlock(blk.oldblocknr, g);
                blk.oldblocknr = 0;
                if (!await Pfs3Disk.RawWrite(g.stream, blk.blk, g.currentvolume.rescluster, blk.blocknr, g))
                {
                    // ErrorMsg (AFS_ERROR_UPDATE_FAIL, NULL, g);
                    throw new IOException("AFS_ERROR_UPDATE_FAIL");
                }
            }

            blk.changeflag = false;
            return true;
        }

        public static async Task RemoveEmptyABlocks(Pfs3VolumeData volume, Pfs3GlobalData g)
        {
            // canodeblock
            Pfs3CachedBlock blk;
            uint indexblknr, indexoffset, i;
            // struct cindexblock *index;
            var andata = g.glob_anodedata;

            foreach (var node in volume.anblks)
            {
                blk = node.Value;
                if (blk.changeflag && blk.ANodeBlock != null && !IsFirstABlk(blk) && IsEmptyABlk(blk, g) &&
                    !Pfs3Cache.ISLOCKED(blk, g))
                {
                    var anodeblock = blk.ANodeBlock;
                    indexblknr = anodeblock.seqnr / andata.indexperblock;
                    indexoffset = anodeblock.seqnr % andata.indexperblock;

                    /* kill the block */
                    Pfs3Macro.MinRemove(blk, g);
                    Pfs3Allocation.FreeReservedBlock(blk.blocknr, g);
                    Pfs3Lru.ResToBeFreed(blk.oldblocknr, g);
                    Pfs3Lru.FreeLRU(blk, g);

                    /* and remove the reference (this one should already be in the cache) */
                    var index = await Pfs3Anodes.GetIndexBlock((ushort)indexblknr, g);
                    if (index == null)
                    {
                        // DBERR(if (!index) ErrorTrace(5, "RemoveEmptyABlocks", "GetIndexBlock returned NULL!"))
                        throw new IOException("RemoveEmptyABlocks, GetIndexBlock returned NULL!");
                    }

                    index.IndexBlock.index[indexoffset] = 0;
                }
            }

            // for (i = 0; i <= Constants.HASHM_ANODE; i++)
            // {
            //     for (var node = Macro.HeadOf(volume.anblks[i]); node != null; node = node.Next)
            //     {
            //         blk = node.Value;
            //         if (blk.changeflag && blk.ANodeBlock != null && !IsFirstABlk(blk) && IsEmptyABlk(blk, g) &&
            //             !Cache.ISLOCKED(blk, g))
            //         {
            //             var anodeblock = blk.ANodeBlock;
            //             indexblknr = anodeblock.seqnr / andata.indexperblock;
            //             indexoffset = anodeblock.seqnr % andata.indexperblock;
            //
            //             /* kill the block */
            //             Macro.MinRemove(blk, g);
            //             Pfs3Allocation.FreeReservedBlock(blk.blocknr, g);
            //             Lru.ResToBeFreed(blk.oldblocknr, g);
            //             Lru.FreeLRU(blk, g);
            //
            //             /* and remove the reference (this one should already be in the cache) */
            //             var index = await Pfs3Anodes.GetIndexBlock((ushort)indexblknr, g);
            //             if (index == null)
            //             {
            //                 // DBERR(if (!index) ErrorTrace(5, "RemoveEmptyABlocks", "GetIndexBlock returned NULL!"))
            //                 throw new IOException("RemoveEmptyABlocks, GetIndexBlock returned NULL!");
            //             }
            //
            //             index.IndexBlock.index[indexoffset] = 0;
            //             index.changeflag = true;
            //         }
            //     }
            // }
        }

        public static bool IsEmptyABlk(Pfs3CachedBlock ablk, Pfs3GlobalData g)
        {
            // canodeblock
            Pfs3Anode[] anodes;
            uint j;
            bool found = false;
            var andata = g.glob_anodedata;

            /* zoek bezette anode */
            anodes = ablk.ANodeBlock.nodes;
            for (j = 0; j < andata.anodesperblock && !found; j++)
                found |= (anodes[j].blocknr != 0);

            found = !found;
            return found; /* not found -> empty */
        }

/*
 * Empty block check
 */
        public static async Task RemoveEmptyIBlocks(Pfs3VolumeData volume, Pfs3GlobalData g)
        {
            //struct cindexblock *blk, *next;
            Pfs3CachedBlock blk;

            // for (var node = Macro.HeadOf(volume.indexblks); node != null; node = node.Next)
            // {
            //     blk = node.Value;
            //     if (blk.changeflag && !IsFirstIBlk(blk) && IsEmptyIBlk(blk, g) && !Cache.ISLOCKED(blk, g))
            //     {
            //         await UpdateIBLK(blk, 0, g);
            //         Macro.MinRemove(blk, g);
            //         Pfs3Allocation.FreeReservedBlock(blk.blocknr, g);
            //         Lru.ResToBeFreed(blk.oldblocknr, g);
            //         Lru.FreeLRU(blk, g);
            //     }
            // }
            foreach (var node in volume.indexblks)
            {
                blk = node.Value;
                if (blk.changeflag && !IsFirstIBlk(blk) && IsEmptyIBlk(blk, g) && !Pfs3Cache.ISLOCKED(blk, g))
                {
                    await UpdateIBLK(blk, 0, g);
                    Pfs3Macro.MinRemove(blk, g);
                Pfs3Allocation.FreeReservedBlock(blk.blocknr, g);
                    Pfs3Lru.ResToBeFreed(blk.oldblocknr, g);
                    Pfs3Lru.FreeLRU(blk, g);
                }
            }
        }

        public static void RemoveEmptySBlocks(Pfs3VolumeData volume, Pfs3GlobalData g)
        {
            //struct cindexblock *blk, *next;
            Pfs3CachedBlock blk;

            // for (var node = Macro.HeadOf(volume.superblks); node != null; node = node.Next)
            // {
            //     blk = node.Value;
            //     if (blk.changeflag && !IsFirstIBlk(blk) && IsEmptyIBlk(blk, g) && !Cache.ISLOCKED(blk, g))
            //     {
            //         UpdateSBLK(blk, 0, g);
            //         Macro.MinRemove(blk, g);
            //         Pfs3Allocation.FreeReservedBlock(blk.blocknr, g);
            //         Lru.ResToBeFreed(blk.oldblocknr, g);
            //         Lru.FreeLRU(blk, g);
            //     }
            // }
            foreach (var node in volume.superblks)
            {
                blk = node.Value;
                if (blk.changeflag && !IsFirstIBlk(blk) && IsEmptyIBlk(blk, g) && !Pfs3Cache.ISLOCKED(blk, g))
                {
                    UpdateSBLK(blk, 0, g);
                    Pfs3Macro.MinRemove(blk, g);
                Pfs3Allocation.FreeReservedBlock(blk.blocknr, g);
                    Pfs3Lru.ResToBeFreed(blk.oldblocknr, g);
                    Pfs3Lru.FreeLRU(blk, g);
                }
            }
        }

        public static bool IsEmptyIBlk(Pfs3CachedBlock blk, Pfs3GlobalData g)
        {
            // ULONG *index, i;
            bool found = false;
            var andata = g.glob_anodedata;

            var index = blk.IndexBlock.index;
            for (var i = 0; i < andata.indexperblock; i++)
            {
                found = index[i] != 0;
                if (found)
                    break;
            }

            found = !found;
            return found;
        }
    }
}
