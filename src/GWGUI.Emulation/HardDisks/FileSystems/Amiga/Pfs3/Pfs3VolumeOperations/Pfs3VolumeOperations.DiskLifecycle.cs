namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Blocks;

    public static partial class Pfs3VolumeOperations
    {
        public static async Task<Pfs3RootBlock> GetCurrentRoot(Pfs3GlobalData g)
        {
            // read boot block
            var blockBytes = await Pfs3Disk.RawRead(1, Pfs3Constants.BOOTBLOCK1, g);
            var rootBlock = Pfs3RootBlockReader.Parse(blockBytes);

            if (!(rootBlock.DiskType == Pfs3Constants.ID_PFS_DISK || rootBlock.DiskType == Pfs3Constants.ID_PFS2_DISK))
            {
                throw new IOException("ID_NOT_REALLY_DOS");
            }

            g.disktype = Pfs3Constants.ID_PFS_DISK;

            // read root block
            blockBytes = await Pfs3Disk.RawRead(1, Pfs3Constants.ROOTBLOCK, g);
            rootBlock = Pfs3RootBlockReader.Parse(blockBytes);

            // read reserved bitmap blocks
            var numReserved = Pfs3Formatter.CalcNumReserved(g, rootBlock.ReservedBlksize);
            var reservedBitmapsCount = (int)(numReserved / 32 + 1);
            var reservedBitmapBlocksCount = Pfs3Helper.CalculateBitmapBlocksCount(reservedBitmapsCount, g);
            blockBytes = await Pfs3Disk.RawRead((uint)reservedBitmapBlocksCount, Pfs3Constants.ROOTBLOCK + 1, g);
            rootBlock.ReservedBitmapBlock = Pfs3BitmapBlockReader.Parse(blockBytes, reservedBitmapsCount);

            /* check size and read all rootblock blocks */
            // 17.10: with 1024 byte blocks rblsize can be 1!
            var rblsize = rootBlock.RblkCluster;
            if (rblsize < 1 || rblsize > 521)
            {
                throw new IOException("ID_NOT_REALLY_DOS");
            }

            // original PFS_DISK with PFS2_DISK features -> don't mount
            if (rootBlock.DiskType == Pfs3Constants.ID_PFS_DISK && (rootBlock.Options.HasFlag(Pfs3RootBlock.Pfs3DiskOptions.MODE_LARGEFILE) ||
                                                                rootBlock.ReservedBlksize > 1024))
                throw new IOException("ID_NOT_REALLY_DOS");

            Pfs3Lru.InitLRU(g, rootBlock.ReservedBlksize);

            /* size check */
            // if ((rootBlock.Options.HasFlag(RootBlock.DiskOptionsEnum.MODE_SIZEFIELD) &&
            //     (g->geom->dg_TotalSectors != (*rootblock)->disksize))
            // {
            //     throw new IOException("ID_NOT_REALLY_DOS");
            // }

            return rootBlock;
        }

        public static async Task DiskInsertSequence(Pfs3RootBlock rootBlock, Pfs3GlobalData g)
        {
            var fe = new Pfs3FileEntry
            {
                le = new Pfs3ListEntry
                {
                    info = new Pfs3ObjectInfo
                    {

                    }
                }
            };

            // fe = LOCKTOFILEENTRY(locklist);
            // if(fe->le.type.flags.type == ETF_VOLUME)
            //     g->currentvolume = fe->le.info.volume.volume;
            // else
            //     g->currentvolume = fe->le.volume;

            g.currentvolume = await MakeVolumeData(rootBlock, g);

            /* update rootblock */
            g.RootBlock = rootBlock;

            /* Reconfigure modules to new volume */
            await Pfs3Init.InitModules (g.currentvolume, false, g);

            /* create rootblockextension if its not there yet */
            if (g.currentvolume.rblkextension == null &&
                g.diskstate != Pfs3Constants.ID_WRITE_PROTECTED)
            {
                Pfs3Formatter.MakeRBlkExtension (g);
            }

            /* upgrade deldir */
            if (rootBlock.DelDir > 0)
            {
                /* kill current deldir */
                var ddblk = await Pfs3Lru.AllocLRU(g);
                if (ddblk != null)
                {
                    if ((ddblk.blk = await Pfs3Disk.RawRead<Pfs3DelDirBlock>(Pfs3Constants.RESCLUSTER(g), rootBlock.DelDir, g) ) != null)
                    {
                        var blk = ddblk.deldirblock;
                        if (blk.id == Pfs3Constants.DELDIRID)
                        {
                            for (var i=0; i<31; i++)
                            {
                                var nr = blk.entries[i].anodenr;
                                if (nr > 0)
                                    await Pfs3Directory.FreeAnodesInChain(nr, g);
                            }
                        }
                    }
                    Pfs3Lru.FreeLRU(ddblk, g);
                }

                /* create new deldir */
                await Pfs3Directory.SetDeldir(1, g);
                Pfs3Lru.ResToBeFreed(rootBlock.DelDir, g);
                rootBlock.DelDir = 0;
                rootBlock.Options |= Pfs3RootBlock.Pfs3DiskOptions.MODE_SUPERDELDIR;
            }

            /* update datestamp and enable */
            rootBlock.Options |= Pfs3RootBlock.Pfs3DiskOptions.MODE_DATESTAMP;
            rootBlock.Datestamp++;
            g.dirty = true;
        }

/* checks if disk is changed. If so calls NewVolume()
** NB: new volume might be NOVOLUME or NOTAFDSDISK
*/
        public static async Task UpdateCurrentDisk(Pfs3GlobalData g)
        {
            await NewVolume(false, g);
        }

        public static async Task NewVolume (bool force, Pfs3GlobalData g)
        {
            bool oldstate, newstate;//, changed;

            /* check if something changed */
            // changed = UpdateChangeCount (g);
            // if (!FORCE && !changed)
            //     return;
	           //
            // if (!AttemptLockDosList(LDF_VOLUMES | LDF_WRITE))
            //     return;

            // ENTER("NewVolume");
#if DEBUG
            Pfs3Logger.Instance.Debug("Pfs3VolumeOperations: NewVolume Enter");
#endif
            Pfs3Disk.FlushDataCache(g);

            /* newstate <=> there is a PFS disk present */
            oldstate = g.currentvolume != null;
            var rootBlock = await GetCurrentRoot(g);
            newstate = rootBlock != null;

            /* undo error enforced softprotect */
            if (g.softprotect && g.protectkey == ~0)
            {
                g.protectkey = 0;
                g.softprotect = false;
            }

            if (oldstate && !newstate)
            {
                await DiskRemoveSequence (g);
            }

            if (newstate)
            {
                // if (oldstate && SameDisk (rootBlock, g.currentvolume.rootblk))
                // {
                //     // FreeBufmem (rootblock, g);  /* @XLVII */
                // }
                // else
                // {
                //     if (oldstate)
                //     {
                //         await DiskRemoveSequence (g);
                //     }
                //     await DiskInsertSequence(rootBlock, g);
                // }
            }
            else
            {
                g.currentvolume = null;    /* @XL */
            }

            // UnLockDosList(LDF_VOLUMES | LDF_WRITE);

            // UpdateAndMotorOff(g);
            //EXIT("NewVolume");
#if DEBUG
            Pfs3Logger.Instance.Debug("Pfs3VolumeOperations: NewVolume Exit");
#endif
        }

/* pre:
**  globaldata->currentvolume not necessarily present
** post:
**  the old currentvolume is updated en als 'removed' currentvolume == 0
** return waarde = currentdisk back in drive?
** used by NewVolume and ACTION_INHIBIT
*/
        public static async Task DiskRemoveSequence(Pfs3GlobalData g)
        {
            Pfs3VolumeData oldvolume = g.currentvolume;

            // ENTER("DiskRemoveSequence");

            /* -I- update disk
            ** will ask for old volume if there are unsaved changes
            ** causes recursive NewVolume call. That's why 'currentvolume'
            ** has to be cleared first; UpdateDisk won't be called for the
            ** same disk again
            */
            if(oldvolume != null && g.dirty)
            {
                // RequestCurrentVolumeBack(g);
                await Pfs3Update.UpdateDisk(g);
                return;
            }

            /* disk removed */
            g.currentvolume = null;
            Pfs3Disk.FlushDataCache(g);

            /* -II- link locks in doslist
            ** lockentries: link to doslist...
            ** fileentries: link them too...
            */
            // if(!Macro.IsMinListEmpty(&oldvolume->fileentries))
            // {
            //     DB(Trace(1, "DiskRemoveSequence", "there are locks\n"));
            //     oldvolume->devlist->dl_LockList = MKBADDR(&(((listentry_t *)(HeadOf(&oldvolume->fileentries)))->lock));
            //     oldvolume->devlist->dl_Task = NULL;
            //     FreeUnusedResources(oldvolume, g);
            // }
            // else
            // {
            //     DB(Trace(1, "DiskRemoveSequence", "removing doslist\n"));
            //     RemDosEntry((struct DosList*)oldvolume->devlist);
            //     FreeDosEntry((struct DosList*)oldvolume->devlist);
            //     MinRemove(oldvolume);
            //     FreeVolumeResources(oldvolume, g);
            // }

// #ifdef TRACKDISK
//             if(g->trackdisk)
//             {
//                 g->request->iotd_Req.io_Command = CMD_CLEAR;
//                 DoIO((struct IORequest*)g->request);
//             }
// #endif

            // CreateInputEvent(FALSE, g);

// #if ACCESS_DETECT
// 	g->tdmode = ACCESS_UNDETECTED;
// #endif

            // EXIT("DiskRemoveSequence");
            // return;
        }
    }
}
