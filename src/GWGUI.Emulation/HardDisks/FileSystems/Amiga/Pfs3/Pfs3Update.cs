namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;

    public static partial class Pfs3Update
    {
/* --> part of update
 * marks a directory or anodeblock dirty. Nothing happens if it already
 * was dirty. If it wasn't, the block will be reallocated and marked dirty.
 * If the reallocation fails, an error is displayed.
 *
 * result: TRUE = was clean; FALSE: was already dirty
 *
 * LOCKing the block until next packet proves to be too restrictive,
 * so unlock afterwards.
 */
        public static async Task<bool> MakeBlockDirty(Pfs3CachedBlock blk, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug(
                $"Pfs3Update: MakeBlockDirty, block nr = {blk.blocknr}, block type '{(blk.blk == null ? "null" : blk.blk.GetType().Name)}'");
#endif
            uint blocknr;
            ushort oldlock;

            if (!blk.changeflag)
            {
                g.dirty = true;
                oldlock = blk.used;
                Pfs3Cache.LOCK(blk, g);

                blocknr = Pfs3Allocation.AllocReservedBlock(g);
                if (blocknr != 0)
                {
                    blk.oldblocknr = blk.blocknr;
                    blk.blocknr = blocknr;
                    await UpdateBlocknr(blk, blocknr, g);
                }
                else
                {
// #ifdef BETAVERSION
//                     ErrorMsg(AFS_BETA_WARNING_2, NULL, g);
// #endif
                    blk.changeflag = true;
                }

                blk.used = oldlock; // unlock block
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task UpdateBlocknr(Pfs3CachedBlock blk, uint newblocknr, Pfs3GlobalData g)
        {
            switch (blk.blk.id)
            {
                case Pfs3Constants.DBLKID: /* dirblock */
                    await UpdateDBLK(blk, newblocknr, g);
                    break;

                case Pfs3Constants.ABLKID: /* anodeblock */
                    await UpdateABLK(blk, newblocknr, g);
                    break;

                case Pfs3Constants.IBLKID: /* indexblock */
                    await UpdateIBLK(blk, newblocknr, g);
                    break;

                case Pfs3Constants.BMBLKID: /* bitmapblock */
                    await UpdateBMBLK(blk, newblocknr, g);
                    break;

                case Pfs3Constants.BMIBLKID: /* bitmapindexblock */
                    UpdateBMIBLK(blk, newblocknr, g);
                    break;

                case Pfs3Constants.EXTENSIONID: /* rootblockextension */
                    await UpdateRBlkExtension(blk, newblocknr, g);
                    break;

                case Pfs3Constants.DELDIRID: /* deldir */
                    await UpdateDELDIR(blk, newblocknr, g);
                    break;

                case Pfs3Constants.SBLKID: /* superblock */
                    UpdateSBLK(blk, newblocknr, g);
                    break;
            }
        }

        public static async Task UpdateDBLK(Pfs3CachedBlock blk, uint newblocknr, Pfs3GlobalData g)
        {
            // update dir block dictionary from old to new block nr
            if (g.currentvolume.dirblks.ContainsKey(blk.oldblocknr))
            {
                g.currentvolume.dirblks.Remove(blk.oldblocknr);
                g.currentvolume.dirblks.Add(newblocknr, blk);
            }
#if DEBUG
            Pfs3Logger.Instance.Debug($"Pfs3Update: UpdateDBLK, oldblocknr = {blk.oldblocknr}, newblocknr = {newblocknr}");
#endif
            Pfs3CachedBlock dblk = blk;
            Pfs3Canode anode = new Pfs3Canode();
            uint oldblocknr = dblk.oldblocknr;

            Pfs3Cache.LOCK(blk, g);

            /* get old anode (all 1-block anodes) */
            await Pfs3Anodes.GetAnode(anode, dblk.dirblock.anodenr, g);
            while (anode.blocknr != oldblocknr && anode.next != 0)
            {
                //anode.next purely safety
                await Pfs3Anodes.GetAnode(anode, anode.next, g);
            }

            /* change it.. */
            if (anode.blocknr != oldblocknr)
            {
#if DEBUG
                Pfs3Logger.Instance.Debug(
                    $"Pfs3Update: UpdateDBLK, anode.blocknr = {anode.blocknr}, dblk.blocknr = {dblk.blocknr}");
#endif
                // DB(Trace(4, "UpdateDBLK", "anode.blocknr=%ld, dblk->blocknr=%ld\n",
                //     anode.blocknr, dblk->blocknr));
                // ErrorMsg (AFS_ERROR_CACHE_INCONSISTENCY, NULL, g);
                throw new IOException("AFS_ERROR_CACHE_INCONSISTENCY");
            }

            /* This must happen AFTER anode correction, because Pfs3Update() could be called,
             * causing trouble (invalid checkpoint: dirblock uptodate, anode not)
             */
            blk.changeflag = true;
            anode.blocknr = newblocknr;
            await Pfs3Anodes.SaveAnode(anode, anode.nr, g);

            //Macro.ReHash(blk, g.currentvolume.dirblks, Constants.HASHM_DIR, g);
        }

        public static async Task UpdateABLK(Pfs3CachedBlock blk, uint newblocknr, Pfs3GlobalData g)
        {
            // update anode block dictionary from old to new block nr
            if (g.currentvolume.anblks.ContainsKey(blk.oldblocknr))
            {
                g.currentvolume.anblks.Remove(blk.oldblocknr);
                g.currentvolume.anblks.Add(newblocknr, blk);
            }

            //struct cindexblock *index;
            uint indexblknr, indexoffset, temp;
            var andata = g.glob_anodedata;

            blk.changeflag = true;
            temp = blk.ANodeBlock.seqnr;
            indexblknr = temp / andata.indexperblock;
            indexoffset = temp % andata.indexperblock;

            /* this one should already be in the cache */
            var index = await Pfs3Anodes.GetIndexBlock((ushort)indexblknr, g);
            if (index == null)
            {
                throw new IOException("UpdateABLK, GetIndexBlock returned NULL!");
            }

            // DBERR(if (!index) ErrorTrace(5,"UpdateABLK", "GetIndexBlock returned NULL!"));

#if DEBUG
            Pfs3Logger.Instance.Debug($"Pfs3Update: UpdateABLK, oldblocknr = {blk.oldblocknr}, newblocknr = {newblocknr}");
#endif
            if (index.IndexBlock.index[indexoffset] != blk.oldblocknr)
            {
                throw new IOException(
                    $"Pfs3Update anode block at index offset {indexoffset} doesn't have expected old block nr {blk.oldblocknr} but instead block nr {index.IndexBlock.index[indexoffset]}");
            }

            index.IndexBlock.index[indexoffset] = (int)newblocknr;
            await MakeBlockDirty(index, g);
            //Macro.ReHash(blk, g.currentvolume.anblks, Constants.HASHM_ANODE, g);
        }

        public static async Task UpdateIBLK(Pfs3CachedBlock blk, uint newblocknr, Pfs3GlobalData g)
        {
            // update index block dictionary from old to new block nr
            if (g.currentvolume.indexblks.ContainsKey(blk.oldblocknr))
            {
                g.currentvolume.indexblks.Remove(blk.oldblocknr);
                g.currentvolume.indexblks.Add(newblocknr, blk);
            }

            // struct cindexblock *superblk;
            uint temp;
            var andata = g.glob_anodedata;

            blk.changeflag = true;
            if (g.SuperMode)
            {
                temp = Pfs3Init.divide(blk.IndexBlock.seqnr, andata.indexperblock);
                var superblk = await Pfs3Anodes.GetSuperBlock((ushort)temp /* & 0xffff */, g);

                // DBERR(if (!superblk) ErrorTrace(5,"UpdateIBLK", "GetSuperBlock returned NULL!"));
                if (superblk == null)
                {
                    throw new IOException("UpdateIBLK, GetSuperBlock returned NULL!");
                }

#if DEBUG
                Pfs3Logger.Instance.Debug(
                    $"Pfs3Update: UpdateIBLK, SuperMode, oldblocknr = {blk.oldblocknr}, newblocknr = {newblocknr}");
#endif
                if (superblk.IndexBlock.index[temp >> 16] != blk.oldblocknr)
                {
                    throw new IOException(
                        $"Pfs3Update index block in super block at index offset {temp >> 16} doesn't have expected old block nr {blk.oldblocknr} but instead block nr {superblk.IndexBlock.index[temp >> 16]}");
                }

                superblk.IndexBlock.index[temp >> 16] = (int)newblocknr;
                await MakeBlockDirty(superblk, g);
            }
            else
            {
#if DEBUG
                Pfs3Logger.Instance.Debug(
                    $"Pfs3Update: UpdateIBLK, small, oldblocknr = {blk.oldblocknr}, newblocknr = {newblocknr}");
#endif
                if (g.RootBlock.idx.small.indexblocks[blk.IndexBlock.seqnr] != blk.oldblocknr)
                {
                    throw new IOException(
                        $"Pfs3Update index block in small at index offset {blk.IndexBlock.seqnr} doesn't have expected old block nr {blk.oldblocknr} but instead block nr {g.RootBlock.idx.small.indexblocks[blk.IndexBlock.seqnr]}");
                }

                g.RootBlock.idx.small.indexblocks[blk.IndexBlock.seqnr] = newblocknr;
                g.currentvolume.rootblockchangeflag = true;
            }
        }

        public static void UpdateSBLK(Pfs3CachedBlock blk, uint newblocknr, Pfs3GlobalData g)
        {
            // update super block dictionary from old to new block nr
            if (g.currentvolume.superblks.ContainsKey(blk.oldblocknr))
            {
                g.currentvolume.superblks.Remove(blk.oldblocknr);
                g.currentvolume.superblks.Add(newblocknr, blk);
            }

            // blk->changeflag = TRUE;
            // blk->volume->rblkextension->changeflag = TRUE;
            // blk->volume->rblkextension->blk.superindex[((struct cindexblock *)blk)->blk.seqnr] = newblocknr;
            blk.changeflag = true;
            blk.volume.rblkextension.changeflag = true;
#if DEBUG
            Pfs3Logger.Instance.Debug($"Pfs3Update: UpdateSBLK, oldblocknr = {blk.oldblocknr}, newblocknr = {newblocknr}");
#endif
            if (blk.volume.rblkextension.rblkextension.superindex[blk.IndexBlock.seqnr] != blk.oldblocknr)
            {
                throw new IOException(
                    $"Pfs3Update super block at index offset {blk.IndexBlock.seqnr} doesn't have expected old block nr {blk.oldblocknr} but instead block nr {blk.volume.rblkextension.rblkextension.superindex[blk.IndexBlock.seqnr]}");
            }

            blk.volume.rblkextension.rblkextension.superindex[blk.IndexBlock.seqnr] = newblocknr;
        }

        public static async Task UpdateBMBLK(Pfs3CachedBlock blk, uint newblocknr, Pfs3GlobalData g)
        {
            // update bitmap block dictionary from old to new block nr
            if (g.currentvolume.bmblks.ContainsKey(blk.oldblocknr))
            {
                g.currentvolume.bmblks.Remove(blk.oldblocknr);
                g.currentvolume.bmblks.Add(newblocknr, blk);
            }

            // struct cindexblock *indexblock;
            var bmb = blk;
            uint temp;
            var andata = g.glob_anodedata;

            blk.changeflag = true;
            var bitmapBlk = bmb.BitmapBlock;
            temp = Pfs3Init.divide(bitmapBlk.seqnr, andata.indexperblock);
            var indexblock = await Pfs3Allocation.GetBitmapIndex((ushort)temp /* & 0xffff */, g);

            // DBERR(if (!indexblock) ErrorTrace(5,"UpdateBMBLK", "GetBitmapIndex returned NULL!"));
            if (indexblock == null)
            {
                throw new IOException("UpdateBMBLK, GetBitmapIndex returned NULL!");
            }

#if DEBUG
            Pfs3Logger.Instance.Debug($"Pfs3Update: UpdateBMBLK, oldblocknr = {blk.oldblocknr}, newblocknr = {newblocknr}");
#endif
            var indexBlockBlk = indexblock.IndexBlock;
            if (indexBlockBlk.index[temp >> 16] != blk.oldblocknr)
            {
                throw new IOException(
                    $"Pfs3Update bitmap block at index offset {temp >> 16} doesn't have expected old block nr {blk.oldblocknr} but instead block nr {indexBlockBlk.index[temp >> 16]}");
            }

            indexBlockBlk.index[temp >> 16] = (int)newblocknr;
            await MakeBlockDirty(indexblock, g); /* recursion !! */
        }

        public static void UpdateBMIBLK(Pfs3CachedBlock blk, uint newblocknr, Pfs3GlobalData g)
        {
            // update bitmap index block dictionary from old to new block nr
            if (g.currentvolume.bmindexblks.ContainsKey(blk.oldblocknr))
            {
                g.currentvolume.bmindexblks.Remove(blk.oldblocknr);
                g.currentvolume.bmindexblks.Add(newblocknr, blk);
            }

            // blk->changeflag = TRUE;
            // blk->volume->rootblk->idx.large.bitmapindex[((struct cindexblock *)blk)->blk.seqnr] = newblocknr;
            // blk->volume->rootblockchangeflag = TRUE;
            blk.changeflag = true;
#if DEBUG
            Pfs3Logger.Instance.Debug(
                $"Pfs3Update: UpdateBMIBLK, oldblocknr = {blk.oldblocknr}, newblocknr = {newblocknr}");
#endif
            if (g.RootBlock.idx.large.bitmapindex[blk.IndexBlock.seqnr] != blk.oldblocknr)
            {
                throw new IOException(
                    $"Pfs3Update bitmap index block at index offset {blk.IndexBlock.seqnr} doesn't have expected old block nr {blk.oldblocknr} but instead block nr {g.RootBlock.idx.large.bitmapindex[blk.IndexBlock.seqnr]}");
            }

            g.RootBlock.idx.large.bitmapindex[blk.IndexBlock.seqnr] = newblocknr;
            g.currentvolume.rootblockchangeflag = true;
        }

// #if VERSION23
        public static async Task UpdateRBlkExtension(Pfs3CachedBlock blk, uint newblocknr, Pfs3GlobalData g)
        {
            // blk->changeflag = TRUE;
            // blk->volume->rootblk->extension = newblocknr;
            // blk->volume->rootblockchangeflag = TRUE;
            blk.changeflag = true;
#if DEBUG
            Pfs3Logger.Instance.Debug(
                $"Pfs3Update: UpdateRBlkExtension, oldblocknr = {blk.oldblocknr}, newblocknr = {newblocknr}");
#endif
            if (g.RootBlock.Extension != blk.oldblocknr)
            {
                throw new IOException(
                    $"Pfs3Update root extension block doesn't have expected old block nr {blk.oldblocknr} but instead block nr {g.RootBlock.Extension}");
            }

            g.RootBlock.Extension = newblocknr;
            g.currentvolume.rootblockchangeflag = true;
            await MakeBlockDirty(blk, g);
        }
// #endif

        public static async Task UpdateDELDIR(Pfs3CachedBlock blk, uint newblocknr, Pfs3GlobalData g)
        {
            // update dir block dictionary from old to new block nr
            if (g.currentvolume.deldirblks.ContainsKey(blk.oldblocknr))
            {
                g.currentvolume.deldirblks.Remove(blk.oldblocknr);
                g.currentvolume.deldirblks.Add(newblocknr, blk);
            }

            // blk->changeflag = TRUE;
            // blk->volume->rblkextension->blk.deldir[((struct cdeldirblock *)blk)->blk.seqnr] = newblocknr;
            // MakeBlockDirty((struct Pfs3DoctorCachedBlock *)blk->volume->rblkextension, g);
            blk.changeflag = true;
#if DEBUG
            Pfs3Logger.Instance.Debug(
                $"Pfs3Update: UpdateDELDIR, oldblocknr = {blk.oldblocknr}, newblocknr = {newblocknr}");
#endif
            if (blk.volume.rblkextension.rblkextension.deldir[blk.deldirblock.seqnr] != blk.oldblocknr)
            {
                throw new IOException(
                    $"Pfs3Update deldir block at index offset {blk.deldirblock.seqnr} doesn't have expected old block nr {blk.oldblocknr} but instead block nr {blk.volume.rblkextension.rblkextension.deldir[blk.deldirblock.seqnr]}");
            }

            blk.volume.rblkextension.rblkextension.deldir[blk.deldirblock.seqnr] = newblocknr;
            await MakeBlockDirty(blk.volume.rblkextension, g);
        }

/* Pfs3Update datestamp (copy from rootblock
 * Call before writing block (lru.c)
 */
        public static void UpdateDatestamp(Pfs3CachedBlock blk, Pfs3GlobalData g)
        {
            // struct cdirblock *dblk = (struct cdirblock *)blk;
            // struct crootblockextension *rext = (struct crootblockextension *)blk;

            // switch (((UWORD *)blk->data)[0])
            switch (blk.blk.id)
            {
                case Pfs3Constants.DBLKID: /* dirblock */
                case Pfs3Constants.ABLKID: /* anodeblock */
                case Pfs3Constants.IBLKID: /* indexblock */
                case Pfs3Constants.BMBLKID: /* bitmapblock */
                case Pfs3Constants.BMIBLKID: /* bitmapindexblock */
                case Pfs3Constants.DELDIRID: /* deldir */
                case Pfs3Constants.SBLKID: /* superblock */
                // dblk->blk.datestamp = g->currentvolume->rootblk->datestamp;
                // break;

                case Pfs3Constants.EXTENSIONID: /* rootblockextension */
                    // rext->blk.datestamp = g->currentvolume->rootblk->datestamp;
                    blk.blk.datestamp = g.RootBlock.Datestamp;
                    break;
            }
        }

    }
}
