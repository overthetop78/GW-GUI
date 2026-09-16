namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Blocks;
    using Exceptions;

    public static partial class Pfs3Directory
    {
        public static async Task<uint> ReadFromObject(Pfs3FileEntry file, byte[] buffer, uint size, Pfs3GlobalData g)
        {
            Pfs3CheckAccess.CheckReadAccess(file, g);

            /* check anodechain, make if not there */
            if (file.anodechain == null)
            {
                //DB(Trace(2,"ReadFromObject","getting anodechain"));
                if ((file.anodechain = await Pfs3Anodes.GetAnodeChain(file.le.anodenr, g)) == null)
                {
                    throw new IOException("ERROR_NO_FREE_STORE");
                }
            }

            // #if ROLLOVER
            if (Pfs3Macro.IsRollover(file.le.info))
            {
                return await Pfs3Disk.ReadFromRollover(file, buffer, size, g);
            }
            else
                // #endif
            {
                return await Pfs3Disk.ReadFromFile(file, buffer, size, g);
            }
        }

        public static async Task<uint> WriteToObject(Pfs3FileEntry file, byte[] buffer, uint size, Pfs3GlobalData g)
        {
            /* check write access */
            Pfs3CheckAccess.CheckWriteAccess(file, g);

            /* check anodechain, make if not there */
            if (file.anodechain == null)
            {
                if ((file.anodechain = await Pfs3Anodes.GetAnodeChain(file.le.anodenr, g)) == null)
                {
                    throw new IOException("ERROR_NO_FREE_STORE");
                }
            }

            Pfs3Cache.ClearSearchInDirCache(file.le.dirblocknr, g);

            /* changing file -> set notify flag */
            file.checknotify = true;
            g.dirty = true;

            // #if ROLLOVER
            if (Pfs3Macro.IsRollover(file.le.info))
                return await Pfs3Disk.WriteToRollover(file, buffer, size, g);
            else
                return await Pfs3Disk.WriteToFile(file, buffer, size, g);
        }

/*
 * Updates size field of links
 */
        public static async Task UpdateLinks(Pfs3DirEntry obj, Pfs3GlobalData g)
        {
            Pfs3Canode linklist = new Pfs3Canode();
            Pfs3ObjectInfo loi = new Pfs3ObjectInfo();
            uint linknr;

            //ENTER("UpdateLinks");
            // extrafields = GetExtraFields(entries, object_);
            var extrafields = obj.GetExtraFields();
            linknr = extrafields.link;
            while (linknr != 0)
            {
                /* Pfs3Update link: get link object info and update size */
                await Pfs3Anodes.GetAnode(linklist, linknr, g);
                await Pfs3Lock.FetchObject(linklist.blocknr, linklist.nr, loi, g);
                loi.file.direntry.SetFSize(obj.fsize);
                await Pfs3Update.MakeBlockDirty(loi.file.dirblock, g);
                linknr = linklist.next;
            }
        }

        /* DeleteObject
 *
 * Specification:
 *
 * - The object referenced by the info structure is removed
 * - The object must be on currentvolume
 *
 * Implementation:
 *
 * - check deleteprotection
 * - if dir, check if directory is empty
 * - check if there are outstanding locks on object
 * - remove object from directory and free anode
 * - rearrange & store directory
 *
 * Don't check dirtycount!
 * info becomes INVALID!
 */
        public static async Task DeleteObject(Pfs3ObjectInfo info, Pfs3GlobalData g)
        {
            // ARG1 = Lock to which ARG2 is relative (BPTR)
            // ARG2 = BSTR Name of object to be deleted
            // RES1 = BOOL Success/failure (DOSTRUE/DOSFALSE)
            // RES2 = failurecode (if res1 = DOSFALSE)
            uint anodenr;

            //ENTER("DeleteObject");
            /* Check deleteprotection */
// #if DELDIR
            if (info == null || (!g.IgnoreProtectionBits && (info.deldir.special <= Pfs3Constants.SPECIAL_DELFILE ||
                                 (info.file.direntry.protection & Pfs3Constants.FIBF_DELETE) == Pfs3Constants.FIBF_DELETE)))
// #else
// 	if (!info || IsVolume(*info) || info->file.direntry->protection & FIBF_DELETE)
// #endif
            {
                throw new IOException("ERROR_DELETE_PROTECTED");
            }

            /* Check if link, links can always be removed */
            if ((info.file.direntry.type == Pfs3Constants.ST_LINKFILE) ||
                (info.file.direntry.type == Pfs3Constants.ST_LINKDIR))
            {
                await DeleteLink(info, g);
                return;
            }

            anodenr = Pfs3Macro.FIANODENR(info.file);

            /* Check if there are outstanding locks on object */
            // if (ScanLockList(HeadOf(&g->currentvolume->fileentries), anodenr))
            // {
            // 	DB(Trace(1, "Delete", "object in use"));
            // 	*error = ERROR_OBJECT_IN_USE;
            // 	return DOSFALSE;
            // }

            /* Check if object has links,
             * if it does the object should not be deleted,
             * just the direntry
             */
            if (!await RemapLinks(info, g))
            {
                /* Remove object from directory and free anode */
                if (info.file.direntry.type == Pfs3Constants.ST_USERDIR)
                {
                    await DeleteDir(info, g);
                }
                else
                {
                    /* ST_FILE or ST_SOFTLINK */
                    Pfs3AnodeChain achain;
                    var do_deldir = g.deldirenabled && info.file.direntry.type == Pfs3Constants.ST_FILE;

                    if ((achain = await Pfs3Anodes.GetAnodeChain(anodenr, g)) == null)
                    {
                        throw new IOException("ERROR_NO_FREE_STORE");
                    }

                    if (do_deldir)
                    {
                        var ddslot = await AllocDeldirSlot(g);
                        await AddToDeldir(info, ddslot, g);
                    }

                    await ChangeDirEntry(info, null, null, null, g); /* remove direntry */
                    if (do_deldir)
                    {
                    await Pfs3Allocation.FreeBlocksAC(achain, Pfs3Constants.ULONG_MAX, Pfs3FreeBlockType.keepanodes, g);
                    }
                    else
                    {
                    await Pfs3Allocation.FreeBlocksAC(achain, Pfs3Constants.ULONG_MAX, Pfs3FreeBlockType.freeanodes, g);
                        await Pfs3Anodes.FreeAnode(achain.head.an.nr, g);
                    }

                    Pfs3Anodes.DetachAnodeChain(achain, g);
                }
            }

            //return true;
        }

/*
 * Delete directory
 */
        public static async Task<bool> DeleteDir(Pfs3ObjectInfo info, Pfs3GlobalData g)
        {
            Pfs3Canode anode = new Pfs3Canode();
            Pfs3Canode chnode = new Pfs3Canode();
            var volume = g.currentvolume;
            Pfs3CachedBlock dirblk; // cdirblock
            ushort t;
            var alloc_data = g.glob_allocdata;

            anode.nr = Pfs3Macro.FIANODENR(info.file);
            if (!await DirIsEmpty(anode.nr, g))
            {
                throw new DirectoryNotEmptyException("ERROR_DIRECTORY_NOT_EMPTY");
            }
            else
            {
                /* check if tobefreedcache is sufficiently large,
                 * otherwise update disk
                 */
                chnode.next = anode.nr;
                for (t = 1; chnode.next != 0; t++)
                {
                    await Pfs3Anodes.GetAnode(chnode, chnode.next, g);
                }

                if (2 * t + alloc_data.rtbf_index > Pfs3Constants.RTBF_THRESHOLD)
                {
                    await Pfs3Update.UpdateDisk(g);
                }

                /* do it (btw: fails if dirblock contains more than 128 empty
                 * blocks)
                 */
                anode.next = anode.nr;
                while (anode.next != 0)
                {
                    await Pfs3Anodes.GetAnode(anode, anode.next, g);

                    /* remove dirblock from list if there */
                    // dirblk = Lru.CheckCache(volume.dirblks, Constants.HASHM_DIR, anode.blocknr, g);
                    dirblk = Pfs3Lru.CheckCache(volume.dirblks, anode.blocknr, g);
                    if (dirblk != null)
                    {
                        Pfs3Macro.MinRemove(dirblk, g);
                        if (dirblk.changeflag)
                            Pfs3Lru.ResToBeFreed(dirblk.oldblocknr, g);

                        Pfs3Lru.FreeLRU(dirblk, g);
                    }

                    await Pfs3Anodes.FreeAnode(anode.nr, g);
                    Pfs3Lru.ResToBeFreed(anode.blocknr, g);
                }

                await ChangeDirEntry(info, null, null, null, g); // delete entry from parentdir
                return true;
            }
        }


/* Check if directory with anodenr [anodenr] is empty
 * There can be multiple empty directory blocks
 */
        public static async Task<bool> DirIsEmpty(uint anodenr, Pfs3GlobalData g)
        {
            Pfs3Canode anode = new Pfs3Canode();
            Pfs3CachedBlock dirblok; // cdirblock

            await Pfs3Anodes.GetAnode(anode, anodenr, g);
            dirblok = await LoadDirBlock(anode.blocknr, g);

            var blk = dirblok?.dirblock;
            while (dirblok != null && (blk != null && blk.DirEntries.Count == 0) && anode.next != 0)
            {
                await Pfs3Anodes.GetAnode(anode, anode.next, g);
                dirblok = await LoadDirBlock(anode.blocknr, g);
                blk = dirblok?.dirblock;
            }

            if (dirblok != null && (blk != null && blk.DirEntries.Count == 0)) /* not empty->entries present */
                return true;
            else
                return false;
        }

/*
 * Removes link from linklist and kills direntry
 */
        public static async Task DeleteLink(Pfs3ObjectInfo link, Pfs3GlobalData g)
        {
            // struct Pfs3Canode linknode, linklist;
            // struct Pfs3ExtraFields extrafields;
            // union objectinfo object, directory;
            // UBYTE entrybuffer[MAX_ENTRYSIZE];

            var linknode = new Pfs3Canode();
            var linklist = new Pfs3Canode();
            var extrafields = new Pfs3ExtraFields();
            var object_ = new Pfs3ObjectInfo();
            var directory = new Pfs3ObjectInfo();

            /* get node to remove */
            await Pfs3Anodes.GetAnode(linknode, link.file.direntry.anode, g);
            // var linkDirBlock = link.file.dirblock.dirblock;
            // extrafields = GetExtraFields(linkDirBlock.entries, link.file.direntry);
            extrafields = link.file.direntry.GetExtraFields();

            /* delete old entry */
            await ChangeDirEntry(link, null, null, null, g);

            /* get object */
            await Pfs3Lock.FetchObject(linknode.clustersize, extrafields.link, object_, g);
            // var objectDirBlock = object_.file.dirblock.dirblock;
            // extrafields = GetExtraFields(objectDirBlock.entries, object_.file.direntry);
            extrafields = object_.file.direntry.GetExtraFields();

            /* if the object lists our link as the first link, redirect it to the next one */
            if (extrafields.link == linknode.nr)
            {
                extrafields.SetLink(linknode.next);
                // memcpy(entrybuffer, object.file.direntry, object.file.direntry->next);
                // AddExtraFields((struct Pfs3DirEntry *)entrybuffer, &extrafields);
                var entryBuffer = new Pfs3DirEntry(object_.file.direntry, g);
                entryBuffer.SetExtraFields(extrafields, g);

                if (!await GetParent(object_, directory, g))
                {
                    throw new IOException("ERROR_DISK_NOT_VALIDATED");
                    //return false;	// should never happen
                }
                else
                {
                    await ChangeDirEntry(object_, entryBuffer, directory, object_.file, g);
                }
            }
            /* otherwise simply remove the link from the list of links */
            else
            {
                await Pfs3Anodes.GetAnode(linklist, extrafields.link, g);
                while (linklist.next != linknode.nr)
                {
                    await Pfs3Anodes.GetAnode(linklist, linklist.next, g);
                }

                linklist.next = linknode.next;
                await Pfs3Anodes.SaveAnode(linklist, linklist.nr, g);
            }

            await Pfs3Anodes.FreeAnode(linknode.nr, g);
        }

/*
 * Removes head of linklist and promotes first link as
 * master (NB: object is the main object, NOT a link).
 * Returns linkstate: TRUE: a link was promoted
 *                    FALSE: there was no link to promote
 */
        public static async Task<bool> RemapLinks(Pfs3ObjectInfo object_, Pfs3GlobalData g)
        {
            Pfs3ExtraFields extrafields;
            Pfs3Canode linknode = new Pfs3Canode();
            Pfs3ObjectInfo link = new Pfs3ObjectInfo();
            Pfs3ObjectInfo directory = new Pfs3ObjectInfo();
            Pfs3DirEntry destentry;

            Pfs3Cache.ClearSearchInDirCache(object_.file.dirblock.blocknr, g);

            //ENTER("RemapLinks");
            /* get head of linklist */
            // extrafields = GetExtraFields(dirBlock.entries, object_.file.direntry);
            extrafields = object_.file.direntry.GetExtraFields();
            if (extrafields.link == 0)
            {
                return false;
            }

            /* the file has links; get head of list
             * we are going to promote this link to
             * an object
             */
            await Pfs3Anodes.GetAnode(linknode, extrafields.link, g);

            /* get direntry belonging to this linknode */
            await Pfs3Lock.FetchObject(linknode.blocknr, linknode.nr, link, g);

            /* Promote it from link to object */
            //destentry = (struct Pfs3DirEntry *)entrybuffer;
            //memcpy(destentry, link.file.direntry, link.file.direntry.next);
            destentry = link.file.direntry;
            // var linkDirBlock = link.file.dirblock.dirblock;
            // extrafields = GetExtraFields(linkDirBlock.entries, link.file.direntry);
            extrafields = link.file.direntry.GetExtraFields();
            destentry.SetType(object_.file.direntry.type); // is this necessary?
            destentry.SetFSize(object_.file.direntry.fsize); // is this necessary?
            destentry.SetAnode(object_.file.direntry.anode); // is this necessary?

            extrafields.SetLink(linknode.next);
            //AddExtraFields(linkDirBlock.entries, destentry, extrafields);
            destentry.SetExtraFields(extrafields, g);

            /* Free old linklist node */
            await Pfs3Anodes.FreeAnode(linknode.nr, g);

            /* Remove source direntry */
            await ChangeDirEntry(object_, null, null, null, g);

            /* Refetch new head (can have become invalid) */
            await Pfs3Lock.FetchObject(linknode.blocknr, linknode.nr, link, g);
            if (await GetParent(link, directory, g))
            {
                await ChangeDirEntry(link, destentry, directory, link.file, g);
            }

            /* object directory has changed; update link chain
             * new directory is the old chain head was in: linknode.linkdir (== linknode.blocknr)
             */
            await UpdateLinkDir(link.file.direntry, linknode.blocknr, g);
            return true;
        }

/*
 * Pfs3Update linklist to reflect new directory of linked to object
 */
        public static async Task UpdateLinkDir(Pfs3DirEntry object_, uint newdiran, Pfs3GlobalData g)
        {
            Pfs3Canode linklist = new Pfs3Canode();
            uint linknr;

            //ENTER("UpdateLinkDir");
            // extrafields = GetExtraFields(entries, object_);
            var extrafields = object_.GetExtraFields();
            linknr = extrafields.link;
            while (linknr != 0)
            {
                /* update linklist: change clustersize (== object dir) */
                await Pfs3Anodes.GetAnode(linklist, linknr, g);
                linklist.clustersize = newdiran;
                await Pfs3Anodes.SaveAnode(linklist, linklist.nr, g);
                linknr = linklist.next;
            }
        }

        /* RenameAndMove
 *
 * Specification:
 *
 * - rename object
 * - renaming directories into a child not allowed!
 *
 * Rename across devices tested in dd_Rename (DosToHandlerInterface)
 *
 * Implementation:
 *
 * - source ophalen
 * - check if new name allowed
 * - destination maken
 * - remove source direntry
 * - add destination direntry
 *
 * maxneeds: 2 dblk changed, 1 new an : 3 res
 *
 * sourcedi = objectinfo of source directory
 * destdi = objectinfo of destination directory
 * srcinfo = objectinfo of source
 * destinfo = objectinfo of destination
 * src- destanodenr = anodenr of source- destination directory
 */
    }
}
