namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Blocks;

    public static partial class Pfs3VolumeOperations
    {
/* make and fill in volume structure
 * uses g->geom!
 * returns 0 is fails
 */
        public static async Task<Pfs3VolumeData> MakeVolumeData(Pfs3RootBlock rootblock, Pfs3GlobalData g)
        {
            //  struct Pfs3VolumeData *volume;
            //  struct Pfs3DoctorMinList *list;
            //
            // ENTER("MakeVolumeData");

            // volume = AllocMemPR (sizeof(struct Pfs3VolumeData), g);
            var volume = new Pfs3VolumeData
            {
                rootblockchangeflag = false
            };

            /* lijsten initieren */
            // for (list = &volume.fileentries; list <= &volume->notifylist; list++)
            //     NewList((struct List *)list);
            // for (var node = volume.fileentries.First; list <= volume->notifylist; list++)
            // {
            //     list = node.Value;
            //     NewList((struct List *)list);
            // }


            /* andere gegevens invullen */
            volume.numsofterrors = 0;
            volume.diskstate = Pfs3Constants.ID_VALIDATED;

            /* these could be put in rootblock @@ see also HD version */
            volume.numblocks = g.TotalSectors;
            volume.bytesperblock = (ushort)g.blocksize;
            volume.rescluster = (ushort)(rootblock.ReservedBlksize / volume.bytesperblock);

            /* Calculate minimum fake block size that keeps total block count less than 16M.
             * Workaround for programs (including WB) that calculate free space using
             * "in use * 100 / total" formula that overflows if in use is block count is larger
             * than 16M blocks with 512 block size. Used only in ACTION_INFO.
             */
            g.infoblockshift = 0;
            // if (DOSBase->dl_lib.lib_Version < 50)
            // {
            //     ushort blockshift = 0;
            //     var bpb = volume.bytesperblock;
            //     while (bpb > 512)
            //     {
            //         blockshift++;
            //         bpb >>= 1;
            //     }
            //
            //     // Calculate smallest safe fake block size, up to max 32k. (512=0,1024=1,..32768=6)
            //     while ((volume.numblocks >> blockshift) >= 0x02000000 && g.infoblockshift < 6)
            //     {
            //         g.infoblockshift++;
            //         blockshift++;
            //     }
            // }

            /* load rootblock extension (if it is present) */
            if (rootblock.Extension > 0 && rootblock.Options.HasFlag(Pfs3RootBlock.Pfs3DiskOptions.MODE_EXTENSION))
            {
                var rext = new Pfs3CachedBlock();

                // rext = AllocBufmemR(sizeof(struct Pfs3DoctorCachedBlock) +rootblock->reserved_blksize, g);
                // memset(rext, 0, sizeof(struct Pfs3DoctorCachedBlock) +rootblock->reserved_blksize);
                IPfs3Block blk;
                if ((blk = await Pfs3Disk.RawRead<Pfs3RootBlockExtension>(volume.rescluster, rootblock.Extension, g)) == null)
                {
                    throw new IOException("AFS_ERROR_READ_EXTENSION");
                }
                else
                {
                    rext.blk = blk;
                    if (rext.blk.id == Pfs3Constants.EXTENSIONID)
                    {
                        volume.rblkextension = rext;
                        rext.volume = volume;
                        rext.blocknr = rootblock.Extension;
                    }
                    else
                    {
                        throw new IOException("AFS_ERROR_EXTENSION_INVALID");
                    }
                }
            }
            else
            {
                volume.rblkextension = null;
            }

            return volume;
        }

/* free all resources (memory) taken by volume accept doslist
** it is assumed all this data can be discarded (not checked here!)
** it is also assumed this volume is no part of any volumelist
*/
        public static void FreeVolumeResources(Pfs3VolumeData volume, Pfs3GlobalData g)
        {
            // ENTER("Free volume resources");

            if (volume != null)
            {
                FreeUnusedResources(volume, g);
// #if VERSION23
                // if (volume.rblkextension != null)
                // 	FreeBufmem (volume.rblkextension, g);
// #endif
// #if DELDIR
// 	//	if (g->deldirenabled)
// 	//		FreeBufmem (volume->deldir, g);
// #endif
                // FreeBufmem (volume->rootblk, g);
                // FreeMemP (volume, g);
            }

            // EXIT("FreeVolumeResources");
        }

        public static void FreeUnusedResources(Pfs3VolumeData volume, Pfs3GlobalData g)
        {
            // struct Pfs3DoctorMinList *list;
            // struct MinNode *node, *next;

            // ENTER("FreeUnusedResources");

            /* check if volume passed */
            if (volume == null)
                return;

            // for (list = volume->anblks; list<=&volume->bmindexblks; list++)

            /* start with anblks!, fileentries are to be kept! */
            // for (list = volume->anblks; list<=&volume->bmindexblks; list++)
            // {
            //     node = (struct MinNode *)HeadOf(list);
            //     while ((next = node->mln_Succ))
            //     {
            //         FlushBlock((struct Pfs3DoctorCachedBlock *)node, g);
            //         FreeLRU((struct Pfs3DoctorCachedBlock *)node);
            //         node = next;
            //     }
            // }
            // foreach (var list in volume.anblks)
            // {
            //     FreeMinList(list, g);
            // }
            FreeMinList(volume.anblks, g);

            // foreach (var list in volume.dirblks)
            // {
            //     FreeMinList(list, g);
            // }
            FreeMinList(volume.dirblks, g);

            // FreeMinList(volume.indexblks, g);
            FreeMinList(volume.indexblks, g);

            // FreeMinList(volume.bmblks, g);
            FreeMinList(volume.bmblks, g);

            // FreeMinList(volume.superblks, g);
            FreeMinList(volume.superblks, g);

            // FreeMinList(volume.deldirblks, g);
            FreeMinList(volume.deldirblks, g);

            // FreeMinList(volume.bmindexblks, g);
            FreeMinList(volume.bmindexblks, g);

            volume.anodechainlist.Clear();

            foreach (var node in g.glob_lrudata.LRUpool.Where(x => x.cblk?.blk == null).ToList())
            {
                g.glob_lrudata.LRUpool.Remove(node);
            }

            foreach (var node in g.glob_lrudata.LRUqueue.Where(x => x.cblk?.blk == null).ToList())
            {
                g.glob_lrudata.LRUqueue.Remove(node);
            }
        }

        private static void FreeMinList(LinkedList<Pfs3CachedBlock> list, Pfs3GlobalData g)
        {
            for (var node = list.First; node != null; node = node.Next)
            {
                Pfs3Lru.FlushBlock(node.Value, g);
                Pfs3Lru.FreeLRU(node.Value, g);
            }
        }

        private static void FreeMinList(IDictionary<uint, Pfs3CachedBlock> list, Pfs3GlobalData g)
        {
            var removeKeys = new List<uint>();

            foreach (var node in list)
            {
                if (node.Value != null)
                {
                    Pfs3Lru.FlushBlock(node.Value, g);
                    Pfs3Lru.FreeLRU(node.Value, g);
                }

                removeKeys.Add(node.Key);
            }

            foreach (var key in removeKeys)
            {
                list.Remove(key);
            }
        }

        /* CheckVolume checks if a volume (ve lock) is (still) present.
** If volume==NULL (no disk present) then FALSE is returned (@XLII).
** result: requested volume present/not present TRUE/FALSE
*/
        public static void CheckVolume(Pfs3VolumeData volume, bool write, Pfs3GlobalData g)
        {
            if (volume == null || g.currentvolume == null)
            {
                switch (g.disktype)
                {
                    case Pfs3Constants.ID_UNREADABLE_DISK:
                    case Pfs3Constants.ID_NOT_REALLY_DOS:
                        throw new IOException("ERROR_NOT_A_DOS_DISK");

                    case Pfs3Constants.ID_NO_DISK_PRESENT:
                        if (volume == null && g.currentvolume == null)
                        {
                            throw new IOException("ERROR_NO_DISK");
                        }

                        break;
                    default:
                        throw new IOException("ERROR_DEVICE_NOT_MOUNTED");
                }
            }
            else if (g.currentvolume == volume)
            {
                switch (g.diskstate)
                {
                    case Pfs3Constants.ID_WRITE_PROTECTED:
                        if (write)
                        {
                            throw new IOException("ERROR_DISK_WRITE_PROTECTED");
                        }

                        break;

                    case Pfs3Constants.ID_VALIDATING:
                        if (write)
                        {
                            throw new IOException("ERROR_DISK_NOT_VALIDATED");
                        }

                        break;

                    case Pfs3Constants.ID_VALIDATED:
                        if (write && g.softprotect)
                        {
                            throw new IOException("ERROR_DISK_WRITE_PROTECTED");
                        }

                        break;
                }
            }
            else
            {
                throw new IOException("ERROR_DEVICE_NOT_MOUNTED");
            }
        }

    }
}
