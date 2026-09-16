namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;

    public static partial class Pfs3Update
    {
        public static async Task<bool> UpdateDisk(Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug("Pfs3Update: UpdateDisk");
#endif
            // struct DateStamp time;
            var volume = g.currentvolume;
            var alloc_data = g.glob_allocdata;
            var andata = g.glob_anodedata;
            bool success;

            /*
             * Do update
             */
            if (volume != null && g.dirty && !g.softprotect)
            {
                /*
                 * For performance reasons avoid concurrent access to same physical
                 * device. Note that the lock can be broken safely, it's only used
                 * to avoid excessive seeking due to competing updates.
                 */
                // only needed for amiga to send request to scsi io command
                //lock_device_unit(g);

                g.uip = true;
                g.updateok = true;
                await Pfs3Disk.UpdateDataCache(g); /* flush DiskRead DiskWrite cache */

// #if VERSION23
                /* make sure rootblockextension is reallocated */
                if (volume.rblkextension != null)
                {
                    await MakeBlockDirty(volume.rblkextension, g);
                }
// #endif

                /* commit user space free list */
            await Pfs3Allocation.UpdateFreeList(g);

                /* remove empty dir, anode, index and superblocks */
                await RemoveEmptyDBlocks(volume, g);
                await RemoveEmptyABlocks(volume, g);
                await RemoveEmptyIBlocks(volume, g);
                RemoveEmptySBlocks(volume, g);

                /* update anode, dir, index and superblocks (not changed by UpdateFreeList) */
                // for (i = 0; i <= Constants.HASHM_DIR; i++)
                // {
                //     if (!await UpdateList(Macro.HeadOf(volume.dirblks[i]), g))
                //     {
                //         g.updateok = false;
                //     }
                // }
                if (!await UpdateList(volume.dirblks.Values, g))
                {
                    g.updateok = false;
                }

                // for (i = 0; i <= Constants.HASHM_ANODE; i++)
                // {
                //     if (!await UpdateList(Macro.HeadOf(volume.anblks[i]), g))
                //     {
                //         g.updateok = false;
                //     }
                // }
                if (!await UpdateList(volume.anblks.Values, g))
                {
                    g.updateok = false;
                }

                // if (!await UpdateList(Macro.HeadOf(volume.indexblks), g))
                // {
                //     g.updateok = false;
                // }
                if (!await UpdateList(volume.indexblks.Values, g))
                {
                    g.updateok = false;
                }

                // if (!await UpdateList(Macro.HeadOf(volume.superblks), g))
                // {
                //     g.updateok = false;
                // }
                if (!await UpdateList(volume.superblks.Values, g))
                {
                    g.updateok = false;
                }

// #if DELDIR
                // if (!await UpdateList(Macro.HeadOf(volume.deldirblks), g))
                // {
                //     g.updateok = false;
                // }
                if (!await UpdateList(volume.deldirblks.Values, g))
                {
                    g.updateok = false;
                }
// #endif

// #if VERSION23
                if (volume.rblkextension != null)
                {
                    var rext = volume.rblkextension;

                    /* reserved roving and anode roving */
                    var rext_blk = rext.rblkextension;
                    rext_blk.reserved_roving = alloc_data.res_roving;
                    rext_blk.rovingbit = (ushort)alloc_data.rovingbit;
                    rext_blk.curranseqnr = andata.curranseqnr;

                    /* volume datestamp */
                    //DateStamp(&time);
                    rext_blk.VolumeDate = DateTime.UtcNow;
                    // rext->blk.volume_date[0] = (UWORD)time.ds_Days;
                    // rext->blk.volume_date[1] = (UWORD)time.ds_Minute;
                    // rext->blk.volume_date[2] = (UWORD)time.ds_Tick;
                    rext_blk.datestamp = g.RootBlock.Datestamp;

                    if (!await UpdateDirtyBlock(rext, g))
                    {
                        g.updateok = false;
                    }
                }
// #endif

                /* commit reserved to be freed list */
                CommitReservedToBeFreed(g);

                /* update bitmap and bitmap index blocks */
                // if (!await UpdateList(Macro.HeadOf(volume.bmblks), g))
                // {
                //     g.updateok = false;
                // }
                if (!await UpdateList(volume.bmblks.Values, g))
                {
                    g.updateok = false;
                }

                // if (!await UpdateList(Macro.HeadOf(volume.bmindexblks), g))
                // {
                //     g.updateok = false;
                // }
                if (!await UpdateList(volume.bmindexblks.Values, g))
                {
                    g.updateok = false;
                }

                /* update root (MUST be done last) */
                if (g.updateok)
                {
                    var rootBlockBytes = Pfs3RootBlockWriter.BuildBlock(g.RootBlock, g);
                    g.RootBlock.BlockBytes = rootBlockBytes;
                    await Pfs3Disk.RawWrite(g.stream, rootBlockBytes, 1, Pfs3Constants.ROOTBLOCK, g);

                    var reservedBitmapBlockBytes = Pfs3BitmapBlockWriter.BuildBlock(g.RootBlock.ReservedBitmapBlock, g);
                    g.RootBlock.ReservedBitmapBlock.BlockBytes = reservedBitmapBlockBytes;
                    var blocks = (uint)(reservedBitmapBlockBytes.Length / g.blocksize);
                    await Pfs3Disk.RawWrite(g.stream, reservedBitmapBlockBytes, blocks, Pfs3Constants.ROOTBLOCK + 1, g);

                    g.RootBlock.Datestamp++;
                    volume.rootblockchangeflag = false;

                    /* make sure update is really done */
                    // only needed for amiga to send request to scsi io command
                    // UpdateAndMotorOff(g);
                    success = true;
                }
                else
                {
                    // ErrorMsg(AFS_ERROR_UPDATE_FAIL, NULL, g);
                    throw new IOException("AFS_ERROR_UPDATE_FAIL");
                }

                g.uip = false;

                // only needed for amiga to send request to scsi io command
                // unlock_device_unit(g);
            }
            else
            {
                if (volume != null && g.dirty && g.softprotect)
                {
                    // ErrorMsg (AFS_ERROR_UPDATE_FAIL, NULL, g);
                    throw new IOException("AFS_ERROR_UPDATE_FAIL");
                }

                success = true;
            }

            g.dirty = false;

            // EXIT("UpdateDisk");
            return success;
        }

/*
 * Empty Dirblocks
 */
    }
}
