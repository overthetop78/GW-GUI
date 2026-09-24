namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;
    using Exceptions;

    public static partial class Pfs3Allocation
    {
        public static async Task<bool> AllocateBlocksAC(Pfs3AnodeChain achain, uint size, Pfs3FileInfo ref_, Pfs3GlobalData g)
        {
            uint nr, field, i = 0, j = 0, blocknr, blocksdone = 0;
            uint extra;
            uint oldfilesize = 0;
            uint bmseqnr = 0;
            ushort bmoffset = 0, oldlocknr = 0;
            Pfs3CachedBlock bitmap; // cbitmapblock_t
            Pfs3AnodeChainNode chnode;
            var vol = g.currentvolume;
            var alloc_data = g.glob_allocdata;
            bool extend = false, updateroving = true;

#if DEBUG
            Pfs3Logger.Instance.Debug($"Allocation: AllocateBlocksAC Enter");
#endif

            /* Check if allocation possible */
            if (alloc_data.alloc_available < size)
                return false;

            /* check for sufficient clean freespace (freespace that doesn't overlap
             * with the current state on disk)
             */
            if (alloc_data.clean_blocksfree < size)
            {
                await Pfs3Update.UpdateDisk(g);
            }

        	/* remember filesize in order to be able to cancel */
            if (ref_ != null)
            {
                oldfilesize = Pfs3Directory.GetDEFileSize(ref_.direntry, g);
            }

            /* count number of fragments and decide on fileextend preallocation
             * get anode to expand
             */
            chnode = achain.head;
            for (i = 0; chnode.next != null; i++)
                chnode = chnode.next;

            extra = Math.Min(256, i * 8);
            if (chnode.an.blocknr != 0 && chnode.an.blocknr != UInt32.MaxValue) // != -1
            {
                i = chnode.an.blocknr + chnode.an.clustersize - alloc_data.bitmapstart;
                nr = i / 32;
                i %= 32;
                j = (uint)(1L << (31 - (int)i));
                bmseqnr = nr / alloc_data.longsperbmb;
                bmoffset = (ushort)(nr % alloc_data.longsperbmb);
                bitmap = await GetBitmapBlock(bmseqnr, g);
                var bitmapBlk = bitmap.BitmapBlock;
                if (bitmapBlk == null)
                {
                    throw new FileSystemDiagnosticException(FileSystemErrorCode.CachedChainBitmapMissing,
                        Pfs3ErrorMessages.CachedChainBitmap(bitmap.blocknr, bitmap.blk), bitmap.blocknr,
                        bitmap.blk?.GetType().Name ?? Pfs3ErrorMessages.NullBlockType);
                }
                field = bitmapBlk.bitmap[bmoffset];

                /* block directly behind file free ? */
                if ((field & j) != 0)
                {
                    extend = true;

                    /* if the position we want to allocate does not corresponds to the
                     * rovingpointer, the rovingpointer should not be updated
                     */
                    if (nr != g.RootBlock.RovingPtr)
                        updateroving = false;
                }
            }


            /* Get bitmap to allocate from */
            if (!extend)
            {
                nr = g.RootBlock.RovingPtr;
                bmseqnr = nr / alloc_data.longsperbmb;
                bmoffset = (ushort)(nr % alloc_data.longsperbmb);
                i = alloc_data.rovingbit;
                j = (uint)(1L << (31 - (int)i));
            }

            /* Allocate */
            while (size != 0)
            {
                /* scan all bitmapblocks */
                bitmap = await GetBitmapBlock(bmseqnr, g);
                oldlocknr = bitmap.used;

                /* find all empty fields */
                while (bmoffset < alloc_data.longsperbmb)
                {
                    var bitmapBlk = bitmap.BitmapBlock;
                    if (bitmapBlk == null)
                    {
                        throw new FileSystemDiagnosticException(FileSystemErrorCode.CachedBitmapMissing,
                            Pfs3ErrorMessages.CachedBitmap(bitmap.blocknr, bitmap.blk), bitmap.blocknr,
                            bitmap.blk?.GetType().Name ?? Pfs3ErrorMessages.NullBlockType);
                    }
                    field = bitmapBlk.bitmap[bmoffset];
                    if (field != 0)
                    {
                        /* take all empty bits */
                        for (; i < 32; j >>= 1, i++)
                        {
                            if ((field & j) != 0)
                            {
                                /* block is available, calc blocknr */
                                blocknr = (bmseqnr * alloc_data.longsperbmb + bmoffset) * 32 + i +
                                          alloc_data.bitmapstart;

                                /* check in range */
                                if (blocknr >= vol.numblocks)
                                {
                                    bmoffset = (ushort)alloc_data.longsperbmb;
                                    continue;
                                }
                                /* take block */
                                else
                                {
                                    Pfs3Macro.Lock(bitmap, g);

                                    /* uninitialized anode */
                                    if (chnode.an.blocknr == UInt32.MaxValue) // = -1
                                    {
                                        chnode.an.blocknr = blocknr;
                                        chnode.an.clustersize = 0;
                                        chnode.an.next = 0;
                                    }
                                    /* check blockconnect */
                                    else if (chnode.an.blocknr + chnode.an.clustersize != blocknr)
                                    {
                                        uint anodenr;

                                        chnode.next = new Pfs3AnodeChainNode();

                                        anodenr = await Pfs3Anodes.AllocAnode(chnode.an.nr, g); /* should not go wrong! */
                                        chnode.an.next = anodenr;
                                        await Pfs3Anodes.SaveAnode(chnode.an, chnode.an.nr, g);
                                        chnode = chnode.next;
                                        chnode.an.nr = anodenr;
                                        chnode.an.blocknr = blocknr;
                                        chnode.an.clustersize = 0;
                                        chnode.an.next = 0;
                                    }

                                    bitmapBlk.bitmap[bmoffset] &= ~j; /* remove block from freelist */
                                    chnode.an.clustersize++; /* to file  	  	  */
                                    await Pfs3Update.MakeBlockDirty(bitmap, g);

                                    /* update counters */
                                    alloc_data.clean_blocksfree--;
                                    alloc_data.alloc_available--;
                                    blocksdone++;

                                    /* update reference */
                                    if (ref_ != null)
                                    {
                                        Pfs3Directory.SetDEFileSize(ref_.dirblock.dirblock, ref_.direntry, Pfs3Directory.GetDEFileSize(ref_.direntry, g) + Pfs3Macro.BLOCKSIZE(g), g);
                                        if (Pfs3Macro.IsUpdateNeeded(Pfs3Constants.RTBF_POSTPONED_TH, g))
                                        {
                                            /* make state valid and update disk */
                                            await Pfs3Update.MakeBlockDirty(ref_.dirblock, g);
                                            await Pfs3Anodes.SaveAnode(chnode.an, chnode.an.nr, g);
                                            if (bitmap.BitmapBlock == null)
                                            {
                                                throw new FileSystemDiagnosticException(
                                                    FileSystemErrorCode.BitmapBlockMissing,
                                                    Pfs3ErrorMessages.BitmapBlockMissing);
                                            }
                                            await Pfs3Update.UpdateDisk(g);

                                            /* abort if running out of reserved blocks */
                                            if (g.RootBlock.ReservedFree <= Pfs3Constants.RESFREE_THRESHOLD)
                                            {
                                                ref_.direntry = Pfs3Directory.SetDEFileSize(ref_.dirblock.dirblock, ref_.direntry, oldfilesize, g);
                                                await Pfs3Update.MakeBlockDirty (ref_.dirblock, g);
                                                await FreeBlocksAC(achain, blocksdone, Pfs3FreeBlockType.freeanodes, g);
                                                return false;
                                            }
                                        }
                                    }


                                    if (--size == 0)
                                        goto alloc_end;
                                }
                            }
                        }

                        i = 0;
                        j = (uint)(1L << 31);
                    }

                    bmoffset++;
                }

                bitmap.used = oldlocknr;

                /* get ready for next block */
                bmseqnr = (bmseqnr + 1) % (alloc_data.no_bmb);
                bmoffset = 0;
            }

            alloc_end:

            /* finish by saving anode and updating roving ptr */
            await Pfs3Anodes.SaveAnode(chnode.an, chnode.an.nr, g);

            /* add fileextension preallocation */
            if (updateroving)
            {
                if (extend)
                {
                    i += extra;
                    alloc_data.rovingbit = i % 32;
                    bmoffset += (ushort)(i / 32);
                    if (bmoffset >= alloc_data.longsperbmb)
                    {
                        bmoffset -= (ushort)alloc_data.longsperbmb;
                        bmseqnr = (bmseqnr + 1) % (alloc_data.no_bmb);
                    }
                }
                else
                {
                    alloc_data.rovingbit = i;
                }

                g.RootBlock.RovingPtr = bmseqnr * alloc_data.longsperbmb + bmoffset;
            }

#if DEBUG
            Pfs3Logger.Instance.Debug($"Allocation: AllocateBlocksAC Exit");
#endif
            return true;
        }
    }
}
