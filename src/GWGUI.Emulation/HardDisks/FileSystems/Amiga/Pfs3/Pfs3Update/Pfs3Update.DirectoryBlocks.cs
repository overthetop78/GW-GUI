namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;

    public static partial class Pfs3Update
    {
        public static async Task RemoveEmptyDBlocks(Pfs3VolumeData volume, Pfs3GlobalData g)
        {
            //struct cdirblock *blk, *next;
            //CachedBlock blk;
            Pfs3Canode anode = new Pfs3Canode();
            uint previous; //, i;

            // for (i = 0; i <= Constants.HASHM_DIR; i++)
            // {
            //     for (var node = Macro.HeadOf(volume.dirblks[i]); node != null; node = node.Next)
            //     {
            //         blk = node.Value;
            //         if (blk.dirblock != null && Macro.IsEmptyDBlk(blk, g) && !await IsFirstDBlk(blk, g) &&
            //             !Cache.ISLOCKED(blk, g))
            //         {
            //             previous = await GetAnodeOfDBlk(blk, anode, g);
            //             await Pfs3Anodes.RemoveFromAnodeChain(anode, previous, blk.dirblock.anodenr, g);
            //             Macro.MinRemove(blk, g);
            //             Pfs3Allocation.FreeReservedBlock(blk.blocknr, g);
            //             Lru.ResToBeFreed(blk.oldblocknr, g);
            //             Lru.FreeLRU(blk, g);
            //         }
            //     }
            // }
            foreach (var blk in volume.dirblks)
            {
                if (blk.Value.dirblock != null && Pfs3Macro.IsEmptyDBlk(blk.Value, g) && !await IsFirstDBlk(blk.Value, g) &&
                    !Pfs3Cache.ISLOCKED(blk.Value, g))
                {
                    previous = await GetAnodeOfDBlk(blk.Value, anode, g);
                    await Pfs3Anodes.RemoveFromAnodeChain(anode, previous, blk.Value.dirblock.anodenr, g);
                    Pfs3Macro.MinRemove(blk.Value, g);
                Pfs3Allocation.FreeReservedBlock(blk.Value.blocknr, g);
                    Pfs3Lru.ResToBeFreed(blk.Value.oldblocknr, g);
                    Pfs3Lru.FreeLRU(blk.Value, g);
                }
            }
        }

        public static async Task<uint> GetAnodeOfDBlk(Pfs3CachedBlock blk, Pfs3Canode anode, Pfs3GlobalData g)
        {
            uint prev = 0;
            await Pfs3Anodes.GetAnode(anode, blk.dirblock.anodenr, g);
            while (anode.blocknr != blk.blocknr && anode.next != 0) //anode.next purely safety
            {
                prev = anode.nr;
                await Pfs3Anodes.GetAnode(anode, anode.next, g);
            }

            return prev;
        }

        public static async Task<bool> IsFirstDBlk(Pfs3CachedBlock blk, Pfs3GlobalData g)
        {
            bool first;
            Pfs3Canode anode = new Pfs3Canode();

            await Pfs3Anodes.GetAnode(anode, blk.dirblock.anodenr, g);
            first = (anode.blocknr == blk.blocknr);

            return first;
        }

        public static bool IsFirstABlk(Pfs3CachedBlock blk)
        {
            // #define IsFirstABlk(blk) (blk->blk.seqnr == 0)

            return blk.ANodeBlock.seqnr == 0;
        }

        public static bool IsFirstIBlk(Pfs3CachedBlock blk)
        {
            //#define IsFirstIBlk(blk) (blk->blk.seqnr == 0)
            return blk.IndexBlock.seqnr == 0;
        }

        public static async Task<bool> UpdateList(LinkedListNode<Pfs3CachedBlock> blk, Pfs3GlobalData g)
        {
            Pfs3CachedBlock blk2;

            if (!g.updateok)
                return false;

            while (blk != null)
            {
                if (blk.Value.changeflag)
                {
                Pfs3Allocation.FreeReservedBlock(blk.Value.oldblocknr, g);
                    blk2 = blk.Value;
                    blk2.blk.datestamp = g.RootBlock.Datestamp;
                    blk.Value.oldblocknr = 0;
                    if (!(await Pfs3Disk.RawWrite(g.stream, blk.Value.blk, g.currentvolume.rescluster, blk.Value.blocknr,
                            g)))
                    {
                        // goto update_error;
                        // ErrorMsg (AFS_ERROR_UPDATE_FAIL, NULL, g);
                        throw new IOException("AFS_ERROR_UPDATE_FAIL");
                    }

                    blk.Value.changeflag = false;
                }

                blk = blk.Next;
            }

            return true;
        }

        public static async Task<bool> UpdateList(IEnumerable<Pfs3CachedBlock> list, Pfs3GlobalData g)
        {
            if (!g.updateok)
                return false;

            foreach (var blk in list)
            {
                if (blk.changeflag)
                {
            Pfs3Allocation.FreeReservedBlock(blk.oldblocknr, g);
                    blk.blk.datestamp = g.RootBlock.Datestamp;
                    blk.oldblocknr = 0;
                    if (!(await Pfs3Disk.RawWrite(g.stream, blk.blk, g.currentvolume.rescluster, blk.blocknr,
                            g)))
                    {
                        // goto update_error;
                        // ErrorMsg (AFS_ERROR_UPDATE_FAIL, NULL, g);
                        throw new IOException("AFS_ERROR_UPDATE_FAIL");
                    }

                    blk.changeflag = false;
                }
            }

            return true;
        }

    }
}
