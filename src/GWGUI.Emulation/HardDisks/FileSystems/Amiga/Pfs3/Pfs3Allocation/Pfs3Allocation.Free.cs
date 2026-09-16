namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;
    using Exceptions;

    public static partial class Pfs3Allocation
    {
        public static async Task FreeBlocksAC(Pfs3AnodeChain achain, uint size, Pfs3FreeBlockType freetype, Pfs3GlobalData g)
        {
            Pfs3AnodeChainNode chnode, tail;
            uint freeing;
            uint i, t = 0;
            bool empty = false;
            uint blocksdone = 0;
            var alloc_data = g.glob_allocdata;


            i = alloc_data.tobefreed_index;
            tail = null;

            /* store operation tobedone */
            var rext = g.currentvolume.rblkextension;
            var rext_blk = rext.rblkextension;
            if (rext != null)
            {
                if (freetype == Pfs3FreeBlockType.keepanodes)
                    rext_blk.tobedone.operation_id = Pfs3Constants.PP_FREEBLOCKS_KEEP;
                else
                    rext_blk.tobedone.operation_id = Pfs3Constants.PP_FREEBLOCKS_FREE;

                rext_blk.tobedone.argument1 = achain.head.an.nr;
                rext_blk.tobedone.argument2 = size;
                rext_blk.tobedone.argument3 = 0; /* blocks done (FREEBLOCKS_KEEP) */
                await Pfs3Update.MakeBlockDirty(rext, g);
            }

            /* check if tobefreedcache is sufficiently large,
             * otherwise updatedisk
             */
            for (chnode = achain.head; chnode.next != null; t++)
                chnode = chnode.next;

            if ((i > 0) && (t + i >= Pfs3Constants.TBF_CACHE_SIZE - 1))
            {
                await Pfs3Update.UpdateDisk(g);
                i = alloc_data.tobefreed_index;
            }

            var l1 = size != 0;

            Pfs3AnodeChainNode lastChNode = null;

            /* reverse order freeloop */
            while (size != 0 && !empty)
            {
                if (!l1)
                {
                    /* Get chainnode to free from */
                    chnode = achain.head;
                    while (chnode.next != tail)
                        chnode = chnode.next;
                }

                l1 = false;
                if (lastChNode != null && lastChNode == chnode)
                {
                    throw new FileSystemDiagnosticException(FileSystemErrorCode.FreeBlockLoop,
                        Pfs3ErrorMessages.FreeBlockLoop(chnode.an.nr), chnode.an.nr);
                }

                /* get blocks to free */
                if (chnode.an.clustersize <= size)
                {
                    freeing = chnode.an.clustersize;
                    chnode.an.clustersize = 0;

                    tail = chnode;
                    empty = tail == achain.head;
                }
                else
                {
                    /* anode is partially freed;
                     * should only be possible in freeanodes mode
                     */
                    freeing = size;
                    chnode.an.clustersize -= size;
                    chnode.an.next = 0;
                    if (freetype == Pfs3FreeBlockType.freeanodes)
                        await Pfs3Anodes.SaveAnode(chnode.an, chnode.an.nr, g);
                }

                /* and put them in the tobefreed list */
                if (freeing != 0)
                {
                    alloc_data.tobefreed[i][Pfs3Constants.TBF_BLOCKNR] = chnode.an.blocknr + chnode.an.clustersize;
                    alloc_data.tobefreed[i++][Pfs3Constants.TBF_SIZE] = freeing;
                    alloc_data.tbf_resneed += 3 + freeing / (32 * alloc_data.longsperbmb);
                    alloc_data.alloc_available += freeing;
                    size -= freeing;
                    blocksdone += freeing;

                    /* free anode if it is empty, we're supposed to and it is not the head */
                    if (!empty && freetype == Pfs3FreeBlockType.freeanodes && chnode.an.clustersize == 0)
                    {
                        await Pfs3Anodes.FreeAnode(chnode.an.nr, g);
                    }
                }

                /* check if intermediate update is needed
                 * (tobefreed cache full, low on reserved blocks etc)
                 */
                if (i >= Pfs3Constants.TBF_CACHE_SIZE || Pfs3Macro.IsUpdateNeeded(Pfs3Constants.RTBF_POSTPONED_TH, g))
                {
                    alloc_data.tobefreed_index = i;
                    g.dirty = true;
                    if (rext != null && freetype == Pfs3FreeBlockType.freeanodes)
                    {
                        /* make anodechain consistent */
                        await RestoreAnodeChain(achain, empty, tail, g);
                        tail = null;

                        /* postponed op: finish operation later */
                        rext_blk.tobedone.argument2 = size;
                    }
                    else
                        /* postponed op: repeat operation later, but don't increase blocks free twice */
                        rext_blk.tobedone.argument3 = blocksdone;

                    await Pfs3Update.MakeBlockDirty(rext, g);
                    await Pfs3Update.UpdateDisk(g);
                    i = alloc_data.tobefreed_index;
                }

                lastChNode = chnode;
            }

            /* restore anode chain (both cached and on disk) */
            if (freetype == Pfs3FreeBlockType.freeanodes)
                await RestoreAnodeChain(achain, empty, tail, g);

            /* cancel posponed operation */
            if (rext != null)
            {
                rext_blk.tobedone.operation_id = 0;
                rext_blk.tobedone.argument1 = 0;
                rext_blk.tobedone.argument2 = 0;
                rext_blk.tobedone.argument3 = 0;
                await Pfs3Update.MakeBlockDirty(rext, g);
            }

            /* update tobefreed index */
            g.dirty = true;
            alloc_data.tobefreed_index = i;

        }

        /* local function of FreeBlocksAC
 * restore anodechain (freeanode mode only)
 */
        public static async Task RestoreAnodeChain(Pfs3AnodeChain achain, bool empty, Pfs3AnodeChainNode tail, Pfs3GlobalData g)
        {
            Pfs3AnodeChainNode chnode;

            if (empty)
            {
                achain.head.next = null;
                achain.head.an.clustersize = 0;
                achain.head.an.blocknr = UInt32.MaxValue;
                achain.head.an.next = 0;
                await Pfs3Anodes.SaveAnode(achain.head.an, achain.head.an.nr, g);
            }
            else
            {
                chnode = achain.head;
                while (chnode.next != tail)
                    chnode = chnode.next;

                chnode.next = null;
                chnode.an.next = 0;
                await Pfs3Anodes.SaveAnode(chnode.an, chnode.an.nr, g);
            }
        }

/*
 * AllocateBlocksAC
 * Allocate blocks to end of (cached) anodechain.
 * If ref != NULL then online directory update enabled.
 * Make sure the state is valid before you call this function!
 * Returns success
 * if FAIL, then a part of the needed blocks could already have been allocated
 */
    }
}
