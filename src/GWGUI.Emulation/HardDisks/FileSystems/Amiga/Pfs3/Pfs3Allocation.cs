namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;

    public static partial class Pfs3Allocation
    {
/*
 * the following routines (NewBitmapBlock & NewBitmapIndexBlock are
 * primarily (read only) used by Format
 */
        public static async Task<Pfs3CachedBlock> NewBitmapBlock(uint seqnr, Pfs3GlobalData g)
        {
            Pfs3CachedBlock blok;
            Pfs3CachedBlock indexblock;
            var volume = g.currentvolume;
            var andata = g.glob_anodedata;
            var alloc_data = g.glob_allocdata;
            uint indexblnr, blocknr, indexoffset;
            uint i;
            ushort oldlock;

            /* get indexblock */
            indexblnr = seqnr / andata.indexperblock;
            indexoffset = seqnr % andata.indexperblock;
            if ((indexblock = await GetBitmapIndex((ushort)indexblnr, g)) == null)
            {
                if ((indexblock = await NewBitmapIndexBlock((ushort)indexblnr, g)) == null)
                {
                    return null;
                }
            }

            oldlock = indexblock.used;
            Pfs3Cache.LOCK(indexblock, g);
            if ((blok = await Pfs3Lru.AllocLRU(g)) == null || (blocknr = AllocReservedBlock(g)) == 0)
            {
                return null;
            }

            indexblock.IndexBlock.index[indexoffset] = (int)blocknr;

            blok.volume = volume;
            blok.blocknr = blocknr;
            blok.used = 0;
            var blok_blk = new Pfs3BitmapBlock((int)g.glob_allocdata.longsperbmb)
            {
                id = Pfs3Constants.BMBLKID,
                seqnr = seqnr
            };
            blok.blk = blok_blk;
            blok.changeflag = true;

            /* fill bitmap */
            var bitmap = blok_blk.bitmap;
            for (i = 0; i < alloc_data.longsperbmb; i++)
            {
                // bitmap[i] = ~0;
                bitmap[i] = UInt32.MaxValue; //  hexadecimal 0xFFFFFFFF
            }

            // Macro.MinAddHead(volume.bmblks, blok);
            Pfs3Macro.AddToIndexes(volume.bmblks, volume.bmblksBySeqNr, blok);
            await Pfs3Update.MakeBlockDirty(indexblock, g);
            indexblock.used = oldlock; // unlock;

            return blok;
        }

        public static async Task<Pfs3CachedBlock> NewBitmapIndexBlock(ushort seqnr, Pfs3GlobalData g)
        {
            Pfs3CachedBlock blok;
            var volume = g.currentvolume;

            if (seqnr > (g.SuperMode ? Pfs3Constants.MAXBITMAPINDEX : Pfs3Constants.MAXSMALLBITMAPINDEX) ||
                (blok = await Pfs3Lru.AllocLRU(g)) == null)
            {
                return null;
            }

            if ((g.RootBlock.idx.large.bitmapindex[seqnr] = AllocReservedBlock(g)) == 0)
            {
                Pfs3Lru.FreeLRU(blok, g);
                return null;
            }

            volume.rootblockchangeflag = true;

            blok.volume = volume;
            blok.blocknr = g.RootBlock.idx.large.bitmapindex[seqnr];
            blok.used = 0;
            blok.blk = new Pfs3IndexBlock(g)
            {
                id = Pfs3Constants.BMIBLKID,
                seqnr = seqnr
            };
            blok.changeflag = true;
            // Macro.MinAddHead(volume.bmindexblks, blok);
            Pfs3Macro.AddToIndexes(volume.bmindexblks, volume.bmindexblksBySeqNr, blok);

            return blok;
        }

        /*
         * AllocReservedBlock
         */
        public static uint AllocReservedBlock(Pfs3GlobalData g)
        {
            var vol = g.currentvolume;
            var alloc_data = g.glob_allocdata;
            var bitmap = g.RootBlock.ReservedBitmapBlock.bitmap;
            //uint free = (uint)g.RootBlock.ReservedFree;
            uint blocknr;
            int i, j;

            // ENTER("AllocReservedBlock");
#if DEBUG
            Pfs3Logger.Instance.Debug($"Allocation: AllocReservedBlock Enter");
#endif

            /* Check if allocation possible
             * (really necessary?)
             */
            if (g.RootBlock.ReservedFree == 0)
            {
                return 0;
            }

            j = (int)(31 - alloc_data.res_roving % 32);
            for (i = (int)(alloc_data.res_roving / 32); i < (alloc_data.numreserved + 31) / 32; i++, j = 31)
            {
                if (bitmap[i] != 0)
                {
                    uint field = bitmap[i];
                    for (; j >= 0; j--)
                    {
                        if ((field & (1 << j)) != 0)
                        {
                            blocknr = (uint)(g.RootBlock.FirstReserved + (i * 32 + (31 - j)) * vol.rescluster);
                            if (blocknr <= g.RootBlock.LastReserved)
                            {
                                bitmap[i] &= (uint)~(1 << j);
                                g.currentvolume.rootblockchangeflag = true;
                                g.dirty = true;
                                g.RootBlock.ReservedFree--;
                                alloc_data.res_roving = (uint)(32 * i + (31 - j));
                                // DB(Trace(10,"AllocReservedBlock","allocated %ld\n", blocknr));
#if DEBUG
                                Pfs3Logger.Instance.Debug($"Allocation: AllocReservedBlock, allocated block nr = {blocknr}, bitmap = {i}, bit = {j}, alloc_data.res_roving = {alloc_data.res_roving}");
#endif
                                return blocknr;
                            }
                        }
                    }
                }
            }

            /* end of bitmap reached. Reset roving pointer and try again
            */
            if (alloc_data.res_roving != 0)
            {
#if DEBUG
                Pfs3Logger.Instance.Debug($"Allocation: end of bitmap reached,alloc_data.res_roving = {alloc_data.res_roving}");
#endif
                alloc_data.res_roving = 0;
                blocknr = AllocReservedBlock(g);
            }
            else
                blocknr = 0;

            // EXIT("AllocReservedBlock");
#if DEBUG
            Pfs3Logger.Instance.Debug($"Allocation: AllocReservedBlock Exit, block nr = {blocknr}");
#endif
            return blocknr;
        }

        public static void FreeReservedBlock(uint blocknr, Pfs3GlobalData g)
        {
            /*
             * frees reserved block, or does nothing if blocknr = 0
             */
            if (blocknr != 0 && blocknr <= g.RootBlock.LastReserved)
            {
                var bitmap = g.RootBlock.ReservedBitmapBlock.bitmap;
                var t = (blocknr - g.RootBlock.FirstReserved) / g.currentvolume.rescluster;

#if DEBUG
                Pfs3Logger.Instance.Debug($"Allocation: FreeReservedBlock, block nr = {blocknr}, bitmap = {t / 32}, bit = {t % 32}");
#endif

                bitmap[t / 32] |= 0x80000000U >> (int)(t % 32);
                g.RootBlock.ReservedFree++;
                g.currentvolume.rootblockchangeflag = true;
            }
        }

        public static async Task<Pfs3CachedBlock> GetBitmapIndex(ushort nr, Pfs3GlobalData g)
        {
            uint blocknr;
            Pfs3CachedBlock indexblk;
            var volume = g.currentvolume;

            /* check cache */
            // for (var node = Macro.HeadOf(volume.bmindexblks); node != null; node = node.Next)
            // {
            //     indexblk = node.Value;
            //     if (indexblk.blk is indexblock indexBlock && indexBlock.seqnr == nr)
            //     {
            //         Lru.MakeLRU(indexblk, g);
            //         return indexblk;
            //     }
            // }
            foreach (var node in volume.bmindexblks)
            {
                indexblk = node.Value;
                if (indexblk.blk is Pfs3IndexBlock indexBlock && indexBlock.seqnr == nr)
                {
                    Pfs3Lru.MakeLRU(indexblk, g);
                    return indexblk;
                }
            }

            /* not in cache, put it in */
            if (nr > (g.SuperMode ? Pfs3Constants.MAXBITMAPINDEX : Pfs3Constants.MAXSMALLBITMAPINDEX) ||
                (blocknr = g.RootBlock.idx.large.bitmapindex[nr]) == 0 ||
                (indexblk = await Pfs3Lru.AllocLRU(g)) == null)
            {
                return null;
            }

            // DB(Trace(10,"GetBitmapIndex", "seqnr = %ld blocknr = %lx\n", nr, blocknr));
#if DEBUG
            Pfs3Logger.Instance.Debug($"Allocation: GetBitmapIndex seqnr = {nr}, blocknr = {blocknr}");
#endif

            IPfs3Block blk;
            if ((blk = await Pfs3Disk.RawRead<Pfs3IndexBlock>(g.currentvolume.rescluster, blocknr, g)) == null)
            {
                Pfs3Lru.FreeLRU(indexblk, g);
                return null;
            }

            indexblk.blk = blk;

            if (indexblk.blk.id == Pfs3Constants.BMIBLKID)
            {
                indexblk.volume = volume;
                indexblk.blocknr = blocknr;
                indexblk.used = 0;
                indexblk.changeflag = false;
                // Macro.MinAddHead(volume.bmindexblks, indexblk);
                Pfs3Macro.AddToIndexes(volume.bmindexblks, volume.bmindexblksBySeqNr, indexblk);
            }
            else
            {
                // ULONG args[5];
                // args[0] = indexblk->blk.id;
                // args[1] = BMIBLKID;
                // args[2] = blocknr;
                // args[3] = nr;
                // args[4] = 0;
                Pfs3Lru.FreeLRU(indexblk, g);
                // ErrorMsg (AFS_ERROR_DNV_WRONG_INDID, args, g);
                return null;
            }

            Pfs3Cache.LOCK(indexblk, g);
            return indexblk;
        }

/*
 * Pfs3Update bitmap
 */
        public static async Task UpdateFreeList(Pfs3GlobalData g)
        {
            Pfs3CachedBlock bitmap = null;
            ushort i;
            uint longnr, blocknr, bmseqnr, newbmseqnr, bmoffset, bitnr;
            var alloc_data = g.glob_allocdata;

            /* sort the free list */
            // not done right now

            /* free all blocks in list */
            bmseqnr = UInt32.MaxValue;
            for (i = 0; i < alloc_data.tobefreed_index; i++)
            {
                for (blocknr = alloc_data.tobefreed[i][Pfs3Constants.TBF_BLOCKNR];
                     blocknr < alloc_data.tobefreed[i][Pfs3Constants.TBF_SIZE] +
                     alloc_data.tobefreed[i][Pfs3Constants.TBF_BLOCKNR];
                     blocknr++)
                {
                    /* now free block blocknr */
                    bitnr = blocknr - alloc_data.bitmapstart;
                    longnr = bitnr / 32;
                    newbmseqnr = longnr / alloc_data.longsperbmb;
                    bmoffset = longnr % alloc_data.longsperbmb;
                    if (newbmseqnr != bmseqnr)
                    {
                        bmseqnr = newbmseqnr;
                        bitmap = await GetBitmapBlock(bmseqnr, g);
                    }

                    bitmap.BitmapBlock.bitmap[bmoffset] |= (uint)(1 << (int)(31 - (bitnr % 32)));
                    await Pfs3Update.MakeBlockDirty(bitmap, g);
                }

                alloc_data.clean_blocksfree += alloc_data.tobefreed[i][Pfs3Constants.TBF_SIZE];
            }

            /* update global data */
            /* alloc_data.alloc_available should already be equal blocksfree - alwaysfree */
            alloc_data.tobefreed_index = 0;
            alloc_data.tbf_resneed = 0;
            g.RootBlock.BlocksFree = alloc_data.clean_blocksfree;
            g.currentvolume.rootblockchangeflag = true;
        }

/* this routine is analogous GetAnodeBlock()
 * GetBitmapIndex is analogous GetIndexBlock()
 */
        public static async Task<Pfs3CachedBlock> GetBitmapBlock(uint seqnr, Pfs3GlobalData g)
        {
            uint blocknr = 0, temp;
            Pfs3CachedBlock bmb = null;
            Pfs3CachedBlock indexblock;
            var volume = g.currentvolume;
            var andata = g.glob_anodedata;

            /* check cache */
            // for (var node = Macro.HeadOf(volume.bmblks); node != null; node = node.Next)
            // {
            //     bmb = node.Value;
            //     if (bmb.BitmapBlock.seqnr == seqnr)
            //     {
            //         Lru.MakeLRU(bmb, g);
            //         return bmb;
            //     }
            // }
            if (volume.bmblksBySeqNr.ContainsKey(seqnr))
            {
                bmb = volume.bmblksBySeqNr[seqnr];
                Pfs3Lru.MakeLRU(bmb, g);
                return bmb;
            }

            /* not in cache, put it in */
            /* get the indexblock */
            temp = Pfs3Init.divide(seqnr, andata.indexperblock);
            if ((indexblock = await GetBitmapIndex((ushort)temp /* & 0xffff */, g)) == null)
                return null;

            /* get blocknr */
            var indexBlockBlk = indexblock.IndexBlock;
            if ((blocknr = (uint)indexBlockBlk.index[temp >> 16]) == 0 || (bmb = await Pfs3Lru.AllocLRU(g)) == null)
            {
                return null;
            }

            // DB(Trace(10,"GetBitmapBlock", "seqnr = %ld blocknr = %lx\n", seqnr, blocknr));
#if DEBUG
            Pfs3Logger.Instance.Debug($"Allocation: GetBitmapBlock seqnr = {seqnr}, blocknr = {blocknr}");
#endif
            /* read it */
            bmb.blk = await Pfs3Disk.RawRead<Pfs3BitmapBlock>(g.currentvolume.rescluster, blocknr, g);
            if (bmb.blk == null)
            {
                Pfs3Lru.FreeLRU(bmb, g);
                return null;
            }

            /* check it */
            if (bmb.blk.id != Pfs3Constants.BMBLKID)
            {
                // ULONG args[2];
                // args[0] = bmb->blk.id;
                // args[1] = blocknr;
                Pfs3Lru.FreeLRU(bmb, g);
                // ErrorMsg (AFS_ERROR_DNV_WRONG_BMID, args, g);
                return null;
            }

            /* initialize it */
            bmb.volume = volume;
            bmb.blocknr = blocknr;
            bmb.used = 0;
            bmb.changeflag = false;
            // Macro.MinAddHead(volume.bmblks, bmb);
            Pfs3Macro.AddToIndexes(volume.bmblks, volume.bmblksBySeqNr, bmb);

            return bmb;
        }

/*
 * Free all blocks in an anodechain? Use FreeBlocksAC(achain,ULONG_MAX,freetype,g)
 */

/*
 * Frees blocks allocated with AllocateBlocks. Freed blocks are added
 * to tobefreed list, and are not actually freed until UpdateFreeList
 * is called. Frees from END of anodelist.
 *
 * 'freetype' specifies if the anodes involved should be freed (freeanodes) or
 * not (keepanodes). In keepanodes mode the whole file (size >= filesize) should
 * be deleted, since otherwise the anodes cannot consistently be kept (dual
 * definition). In freeanodes mode the leading anode is not freed.
 *
 * VERSION23: uses tobedone fields. Because freeing blocks is idem-potent, fully
 * repeating an interrupted operation after reboot is ok. The blocks should not
 * be added to the blocksfree counter twice, however (see DoPostoned())
 *
 * -> all references that indicate the freed blocks must have been
 *  done (atomically).
 */
    }
}
