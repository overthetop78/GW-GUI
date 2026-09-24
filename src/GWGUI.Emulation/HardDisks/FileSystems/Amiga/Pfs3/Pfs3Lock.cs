namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Blocks;

    public static class Pfs3Lock
    {
/* MakeListEntry
**
** Allocated filentry structure and fill it with data from objectinfo and
** listtype. The result should be freed with FreeListEntry.
**
** input : - info: objectinfo of object
**		 - type: desired type (readlock, writelock, readfe, writefe)
**
** result: the fileentry, or NULL if failure
*/
        public static async Task<IPfs3Entry> MakeListEntry(Pfs3ObjectInfo info, Pfs3ListType type, Pfs3GlobalData g)
        {
            var newinfo = new Pfs3ObjectInfo();
            //uint size;
            var extrafields = new Pfs3ExtraFields();
//#if DELDIR
            Pfs3DelDirEntry dde = new Pfs3DelDirEntry();
//#endif

            //ENTER("MakeListEntry");

            // alloceren fileentry
            // switch (type.flags.type)
            // {
            //     case Constants.ETF_FILEENTRY:
            //         size = Pfs3SizeOf.FileEntry;
            //         break;
            //     case Constants.ETF_VOLUME:
            //     case Constants.ETF_LOCK:
            //         size = (uint)Pfs3SizeOf.LockEntry.Struct;
            //         break;
            //     default:
            //         return null;
            // }

            // DB(Trace(1,"MakeListEntry","size = %lx\n",size));

//#if DELDIR
            if (Pfs3Macro.IsDelDir(info) || Pfs3Macro.IsVolume(info) || Pfs3Macro.IsDir(info))
//#else
//	if (Macro.IsVolume(info) || Macro.IsDir(info))
//#endif
            {
                type.flags.dir = 1;
            }

            /* softlinks cannot directly be opened */
//#if DELDIR
	        if (info.deldir.special > Pfs3Constants.SPECIAL_DELFILE && info.file.direntry.type == Pfs3Constants.ST_SOFTLINK)
// #else
//             if (!Macro.IsVolume(info) && info.file.direntry.type == Constants.ST_SOFTLINK)
// #endif
            {
                throw new IOException("ERROR_IS_SOFT_LINK");
            }

            var listentry = new Pfs3ListEntry();
            //var fileentry = new fileentry();
            // if (!(listentry = AllocMemP (size, g)))
            // {
            // 	*error = ERROR_NO_FREE_STORE;
            // 	return NULL;
            // }

            /* go after link and fetch the fileinfo of the real object
             * (stored in 'newinfo'
             */
//#if DELDIR
            if (info.deldir.special > Pfs3Constants.SPECIAL_DELFILE && (
//#else
//	if (!Macro.IsVolume(info) && (
//#endif
                    (info.file.direntry.type == Pfs3Constants.ST_LINKFILE) ||
                    (info.file.direntry.type == Pfs3Constants.ST_LINKDIR)))
            {
                var linknode = new Pfs3Canode();

                /* The clustersize of the linknode (direntry.anode)
                 * actually is the anodenr of the directory the linked to
                 * object is in. The object can be found by searching for
                 * 'anode == objectid'. This objectid can be found in
                 * the extrafields
                 */
                // var dirBlock = info.file.dirblock.dirblock;
                // extrafields = Pfs3Directory.GetExtraFields(dirBlock.entries, info.file.direntry);
                extrafields = info.file.direntry.GetExtraFields();
                await Pfs3Anodes.GetAnode(linknode, info.file.direntry.anode, g);
                if (!await FetchObject(linknode.clustersize, extrafields.link, newinfo, g))
                {
                    throw new IOException("ERROR_OBJECT_NOT_FOUND");
                }
            }
            else
            {
                newinfo = info;
            }

            // general
            listentry.type = type;
//#if DELDIR
            switch (newinfo.delfile.special)
            {
                case 0:
                    listentry.anodenr = Pfs3Constants.ANODE_ROOTDIR;
                    break;

                case Pfs3Constants.SPECIAL_DELDIR:
                    listentry.anodenr = 0;
                    break;

                case Pfs3Constants.SPECIAL_DELFILE:
                    dde = await Pfs3Directory.GetDeldirEntryQuick(newinfo.delfile.slotnr, g);
                    listentry.anodenr = dde.anodenr;
                    break;

                default:
                    listentry.anodenr = newinfo.file.direntry.anode;
                    break;
            }
// #else
// 	listentry->anodenr = (newinfo.file.direntry) ? (newinfo.file.direntry->anode) : ANODE_ROOTDIR;
// #endif

            listentry.info = newinfo;

            // TODO: DOS communication, not sure this is needed
            //listentry.filelock.fl_Access = (type.flags.access & 2) != 0 ? Constants.EXCLUSIVE_LOCK : Constants.SHARED_LOCK;
            //listentry.filelock.fl_Task = g.msgport;
            //listentry.filelock.fl_Volume = Macro.MKBADDR(g.currentvolume.devlist);

            listentry.volume = g.currentvolume;

            // type specific
            switch (type.flags.type)
            {
                case Pfs3Constants.ETF_VOLUME:
                    listentry.filelock.fl_Key = 0;
                    // listentry->lock.fl_Volume = MKBADDR(newinfo.volume.volume->devlist);
                    listentry.volume		  = newinfo.volume.volume;
                    break;

                case Pfs3Constants.ETF_LOCK:
                    /* every dirlock MUST have a different fl_Key (DOPUS!) */
                    listentry.filelock.fl_Key = (int)listentry.anodenr;
                    // listentry->lock.fl_Volume = MKBADDR(newinfo.file.dirblock->volume->devlist);
                    listentry.volume = newinfo.file.dirblock.volume;
                    break;

                case Pfs3Constants.ETF_FILEENTRY:
//#define fe ((fileentry_t *)listentry)
                    //var fe = listentry as fileentry;
                    var fileentry = new Pfs3FileEntry
                    {
                        le = listentry
                    };
                    listentry.filelock.fl_Key = (int)listentry.anodenr;
                    // listentry->lock.fl_Volume = MKBADDR(MKBADDR(newinfo.file.dirblock->volume->devlist);
                    listentry.volume = newinfo.file.dirblock.volume;
                    fileentry.originalsize = Pfs3Macro.IsDelFile(newinfo)
                        ? Pfs3Directory.GetDDFileSize(dde, g)
                        : Pfs3Directory.GetDEFileSize(newinfo.file.direntry, g);

                    /* Get anodechain. If it fails anodechain will become NULL. This has to be
                     * taken into account by functions that use the chain
                     */
                    fileentry.anodechain = await Pfs3Anodes.GetAnodeChain(listentry.anodenr, g);
                    fileentry.currnode = fileentry.anodechain.head;

//#if ROLLOVER
                    /* Rollover file: set offset to rollfileoffset */
                    /* check for rollover files */
                    if (Pfs3Macro.IsRollover(newinfo))
                    {
                        // var dirBlock = newinfo.file.dirblock.dirblock;
                        // extrafields = Pfs3Directory.GetExtraFields(dirBlock.entries, newinfo.file.direntry);
                        extrafields = newinfo.file.direntry.GetExtraFields();
                        await Pfs3Disk.SeekInFile(fileentry, (int)extrafields.rollpointer, Pfs3Constants.OFFSET_BEGINNING, g);
                    }

// #endif /* ROLLOVER */
// #undef fe
                    return fileentry;

                default:
                    // listentry = null;
                    return null;
            }

            return listentry;
        }

/* AddListEntry
**
** Checks if the listentry causes access conflicts
** Adds the entry to the locklist
*/
        public static bool AddListEntry(Pfs3ListEntry entry, Pfs3GlobalData g)
        {
            //DB(Trace(1,"AddListEntry","fe = %lx\n", entry->volume->fileentries.mlh_Head));

            if (entry==null)
                return false;

            if (AccessConflict(entry))
            {
                //DB(Trace(1,"AddListEntry","found accessconflict!"));
                return false;
            }

            var volume = entry.volume;

            /* add to head of list; als link locks using BPTRs */
            if (!Pfs3Macro.IsMinListEmpty(volume.fileentries))
            {
            //     entry.filelock.fl_Link = MKBADDR(&(((listentry_t *)Macro.HeadOf(volume.fileentries))->lock))
            }
            else
            {
                entry.filelock.fl_Link = 0;
            }

            Pfs3Macro.MinAddHead(volume.fileentries, entry);

            return true;
        }

/*
 * Search object by anodenr in directory
 * in: diranodenr, target: anodenr of target and the anodenr of the directory to search in
 * out: result: an objectinfo to the object, if found
 * returns: success
 */
        public static async Task<bool> FetchObject(uint diranodenr, uint target, Pfs3ObjectInfo result, Pfs3GlobalData g)
        {
            Pfs3Canode anode = new Pfs3Canode();
            Pfs3CachedBlock dirblock = null;
            Pfs3DirEntry de = null;
            uint anodeoffset = 0;
            bool eod = false, found = false;

            /* Get directory and find object */
            await Pfs3Anodes.GetAnode(anode, diranodenr, g);
            while (!found && !eod)
            {
                dirblock = await Pfs3Directory.LoadDirBlock(anode.blocknr + anodeoffset, g);
                if (dirblock != null)
                {
                    var blk = dirblock.dirblock;
                    de = blk.DirEntries.FirstOrDefault(x => x.anode == target);
                    found = de != null;
                    // de = Macro.FIRSTENTRY(blk);
                    // while (de.next > 0)
                    // {
                    //     if (!(found = de.anode == target))
                    //         de = Macro.NEXTENTRY(blk, de);
                    //     else
                    //         break;
                    // }

                    if (!found)
                    {
                        var nextBlockResult = await Pfs3Anodes.NextBlock(anode, anodeoffset, g);
                        anodeoffset = nextBlockResult.Item2;
                        eod = !nextBlockResult.Item1;
                    }
                }
                else
                    break;
            }

            if (!found)
                return false;

            result.file.dirblock = dirblock;
            result.file.direntry = de;
            result.volume.root = de.anode != Pfs3Constants.ANODE_ROOTDIR ? 1U : 0U;

            Pfs3Macro.Lock(dirblock, g);
            return true;
        }

        public static void FreeListEntry(IPfs3Entry entry, Pfs3GlobalData g)
        {
            //#define fe ((fileentry_t *)entry)
            var fe = entry as Pfs3FileEntry;
            if (Pfs3Macro.IsFileEntry(entry) && fe?.anodechain != null)
            {
                Pfs3Anodes.DetachAnodeChain(fe.anodechain, g);
            }
            //FreeMemP(entry, g);
            //#undef fe
        }

/* AccessConflict
**
** input : - [entry]: the object to be granted access
**    This object should contain valid references
**
** result: TRUE = accessconflict; FALSE = no accessconflict
**    All locks on same ANODE are checked. So a lock on a link can
**    be denied if the linked to file is locked exclusively.
**
** Because UpdateReference always updates all references to a dirblock,
** and the match object is valid, a flushed reference CANNOT point to
** the same dirblock. If it is a link, it CAN reference the same
** object
**
** Returns FALSE if there is an exclusive lock or if there is
** write access on a shared lock
**
*/
        public static bool AccessConflict (Pfs3ListEntry entry)
        {
            uint anodenr;
            Pfs3ListEntry fe;
            Pfs3VolumeData volume;

            //DB(Trace(1,"Accessconflict","entry %lx\n",entry));

            // -I- get anodenr
            anodenr =  entry.anodenr;
            volume  = entry.volume;

            // -II- zoek locks naar zelfde object
            //for (var node = Macro.HeadOf(volume.bmindexblks); node != null; node = node.Next)

            for(var node = Pfs3Macro.HeadOf(volume.fileentries); node != null; node = node.Next)
            {
                fe = node.Value.ListEntry;
                if(fe.type.flags.type == Pfs3Constants.ETF_VOLUME)
                {
                    if(entry.type.flags.type == Pfs3Constants.ETF_VOLUME &&
                       (!Pfs3Macro.SHAREDLOCK(fe) || !Pfs3Macro.SHAREDLOCK(entry)))
                    {
                        //DB(Trace(1,"Accessconflict","on volume\n"));
                        return true;
                    }
                }
                else if(fe.anodenr == anodenr)
                {
                    // on of the two wants or has an exclusive lock?
                    if(!Pfs3Macro.SHAREDLOCK(fe) || !Pfs3Macro.SHAREDLOCK(entry))
                    {
                        //DB(Trace(1,"Accessconflict","exclusive lock\n"));
                        return true;
                    }

                    // new & old shared lock, both write?
                    else if(fe.type.flags.access == Pfs3Constants.ET_SHAREDWRITE &&
                            entry.type.flags.access == Pfs3Constants.ET_SHAREDWRITE)
                    {
                        //DB(Trace(1,"Accessconflict","two write locks\n"));
                        return true;
                    }
                }
            }

            return false;	// no conflicting locks
        }

/* ScanLockList
 *
 * checks <list> for a lock on the file with anode anodenr
 * ONLY FOR LOCKS TO CURRENTVOLUME
 */
        public static bool ScanLockList (Pfs3ListEntry list, uint anodenr)
        {
            for (;list.next != null; list=list.next)
            {
                if (list.anodenr == anodenr)
                    return true;
            }
            return false;
        }

/* RemoveListEntry
**
** removes 'entry' from the list and frees entry with FreeFileEntry
**
** also think about empty lists: kill volume if empty and not present
** also	makes lock-links
*/
        public static void RemoveListEntry (IPfs3Entry entry, Pfs3GlobalData g)
        {
            // struct Pfs3VolumeData *volume;
            // struct Pfs3DoctorMinList *previous;

            /* get volume */
            //var volume = entry.volume;

            /* remove from list */
            Pfs3Macro.MinRemove(entry, g);

            /* update FileLock link */
            // previous = (struct Pfs3DoctorMinList *)entry->prev;
            // if (!IsHead(entry))
            // {
            //     if (!IsTail(entry))
            //         ((listentry_t *)previous)->lock.fl_Link =
            //         MKBADDR(&(((listentry_t *)entry)->next->lock));
            //     else
            //     ((listentry_t *)previous)->lock.fl_Link = 0;
            // }
            // entry->lock.fl_Task = NULL;
            FreeListEntry (entry, g);

            // UNUSED: Commented out as implementation doesn't switch between volumes
//             if (g.currentvolume != volume)
//             {
//                 /* check if last lock; yes:kill (only if not current disk)
//                 */
//                 if (IsMinListEmpty (&volume->fileentries))
//                 {
//                     DB(Trace(1,"RemoveListEntry", "killing volumedata\n"));
//
// //			LockDosList (LDF_VOLUMES|LDF_READ);
//                     Forbid ();
//                     RemDosEntry ((struct DosList *)volume->devlist);
//                     FreeDosEntry ((struct DosList *)volume->devlist);
//                     FreeVolumeResources (volume, g);
// //			UnLockDosList (LDF_VOLUMES|LDF_READ);
//                     Permit ();
//                 }
//                 /* update doslist->dl_LockList if necessary
//                 */
//                 else if (previous == (struct Pfs3DoctorMinList *)&volume->fileentries)
//                 {
// //			LockDosList (LDF_VOLUMES|LDF_READ);
//                     Forbid ();
//                     volume->devlist->dl_LockList = MKBADDR(&(((fileentry_t *)HeadOf(&volume->fileentries))->le.lock));
// //			UnLockDosList (LDF_VOLUMES|LDF_READ);
//                     Permit ();
//                 }
//             }
        }
    }
}
