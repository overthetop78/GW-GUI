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
        public static async Task<bool> AddDirectoryEntry(Pfs3ObjectInfo dir, Pfs3DirEntry newentry, Pfs3FileInfo newinfo,
            Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug("Pfs3Directory: AddDirectoryEntry");
#endif
            Pfs3Canode anode = new Pfs3Canode();
            uint anodeoffset = 0, diranodenr;
            Pfs3CachedBlock blok = null;
            Pfs3DirEntry entry = null;
            var done = false;
            var eof = false;
            int i;

            if (dir == null || Pfs3Macro.IsVolume(dir))
                diranodenr = (uint)Pfs3Macro.ANODE_ROOTDIR;
            else
                diranodenr = dir.file.direntry.anode;

            /* check if space in existing dirblocks */
            await Pfs3Anodes.GetAnode(anode, diranodenr, g);
            for (; !eof;)
            {
                if ((blok = await LoadDirBlock(anode.blocknr + anodeoffset, g)) == null)
                    break;

                var blk = blok.dirblock;
                i = blk.DirEntries.Sum(x => x.Next);
                // entry = DirEntryReader.Read(blk.entries, 0);
                //
                // /* goto end of dirblock; i = aantal gebruikte bytes */
                // var maxDirEntries = CalculateMaxDirEntries(blk);
                // var dirEntriesNo = 0;
                // for (i = 0; entry.next > 0; entry = DirEntryReader.Read(blk.entries, i))
                // {
                //     dirEntriesNo++;
                //     CheckReadDirEntryError(blok.blocknr, blk, dirEntriesNo, maxDirEntries, i);
                //     i += entry.next;
                // }

                /* does it fit in this block? (keep space for trailing 0) */
                var newEntryNext = newentry.Next;
                if (i + newEntryNext + 1 < Pfs3Macro.DB_ENTRYSPACE(g))
                {
                    blk.DirEntries.Add(newentry);

                    //memcpy(entry, newentry, newentry->next);
                    entry = newentry;

                    //entry.next = 0;
                    //*(UBYTE *)NEXTENTRY(entry) = 0;     // dirblock afsluiten

                    done = true;
                    break;
                }

                var result = await Pfs3Anodes.NextBlock(anode, anodeoffset, g);
                anodeoffset = result.Item2;
                eof = !result.Item1;
            }

            /* no->new dirblock (eof <=> anode is end of chain)
             * We will make the new dirblock at the >start< of
             * the chain.
             * We always allocate new anode
             */
            var newanode = new Pfs3Canode
            {
                clustersize = 1
            };
            if (!done && eof)
            {
                var parent = blok.dirblock.parent;
            if ((newanode.blocknr = Pfs3Allocation.AllocReservedBlock(g)) == 0)
                {
                    return false;
                }

                await Pfs3Anodes.GetAnode(anode, diranodenr, g);
                newanode.nr = diranodenr;
                newanode.next = anode.nr = await Pfs3Anodes.AllocAnode(anode.next > 0 ? anode.next : anode.nr, g);
                await Pfs3Anodes.SaveAnode(anode, anode.nr, g);
                blok = await MakeDirBlock(newanode.blocknr, newanode.nr, diranodenr, parent, g);
                await Pfs3Anodes.SaveAnode(newanode, newanode.nr, g);
                var blk = blok.dirblock;
                blk.DirEntries.Add(newentry);

                //memcpy(entry, newentry, newentry->next);
                //*(UBYTE *)NEXTENTRY(entry) = 0;     // mark end of dirblock

                entry = newentry;
            }

            /* fill newinfo */
            newinfo.direntry = entry;
            newinfo.dirblock = blok;

            /* update notify */
            //PFSUpdateNotify(blok->blk.anodenr, &entry->nlength, entry->anode, g);
            if (blok != null)
            {
                Pfs3Macro.Lock(blok, g);
                await Pfs3Update.MakeBlockDirty(blok, g);
            }

            await Touch(dir, g);
            return true;
        }

        public static async Task<bool> SetDate(Pfs3ObjectInfo file, DateTime date, Pfs3GlobalData g)
        {
            // ENTER("SetDate");

            // #if DELDIR
	        if (file.deldir.special <= Pfs3Constants.SPECIAL_DELFILE)
	        {
		        throw new IOException("ERROR_WRITE_PROTECTED");
	        }
            // #endif

            Pfs3VolumeOperations.CheckVolume(file.file.dirblock.volume, true, g);

            // file->file.direntry->creationday = (UWORD)date->ds_Days;
            // file->file.direntry->creationminute = (UWORD)date->ds_Minute;
            // file->file.direntry->creationtick = (UWORD)date->ds_Tick;
            file.file.direntry.SetDate(date);
            //DirEntryWriter.Write(file.file.dirblock.dirblock.entries, file.file.direntry.Offset, file.file.direntry);
            await Pfs3Update.MakeBlockDirty(file.file.dirblock, g);
            return true;
        }

        public static async Task Touch(Pfs3ObjectInfo info, Pfs3GlobalData g) // ook archiveflag..
        {
            var time = DateTime.Now;

            if (Pfs3Macro.IsVolume(info) && g.currentvolume.rblkextension != null)
            {
                var blk = g.currentvolume.rblkextension.rblkextension;
                blk.RootDate = time;
                await Pfs3Update.MakeBlockDirty(g.currentvolume.rblkextension, g);
            }
            else if (!Pfs3Macro.IsVolume(info))
            {
                info.file.direntry.SetDate(time);
                // info.direntry.creationday = (UWORD)time.ds_Days;
                // info.direntry.creationminute = (UWORD)time.ds_Minute;
                // info.direntry->creationtick = (UWORD)time.ds_Tick;
                info.file.direntry.SetProtection(
                    (byte)(info.file.direntry.protection & ~Pfs3Constants.FIBF_ARCHIVE)); // clear archivebit (eor)

                await Pfs3Update.MakeBlockDirty(info.file.dirblock, g);
            }
        }

/*
 * Pfs3Update references
 * diff is direntry size difference (new - original)
 */
        public static Task UpdateChangedRef(Pfs3FileInfo from, Pfs3FileInfo to, Pfs3GlobalData g)
        {
            var volume = from.dirblock.volume;

            // TODO: Examine when it's necessary update volume file entries, related to open files or dirs
            for (var node = Pfs3Macro.HeadOf(volume.fileentries); node != null; node = node.Next)
            {
                //throw new IOException("fileentries not empty");
                var fe = node.Value.ListEntry;
                /* only dirs and files can be in a directory, but the volume *
                 * of volumeinfos can never point to a cached block, so a
                 * type != ETF_VOLUME check is not necessary. Just check the
                 * dirblock pointer
                 */
                if (fe.info.file.dirblock == from.dirblock)
                {
                    /* is het de targetentry ? */
                    if (fe.info.file.direntry.Equals(from.direntry))
                    {
                        if (to != null)
                        {
                            fe.info.file = to;
                        }
                    }
                    else
                    {
            //             /* take only entries after target */
            //             if (fe.info.file.direntry.Position > from.direntry.Position)
            //             {
            //                 // fe.info.file.direntry = (struct Pfs3DirEntry *)((UBYTE *)fe->info.file.direntry + diff);v
            //                 var d = fe.info.file.dirblock.dirblock;
            //                 // fe.info.file.direntry = DirEntryReader.Read(d.entries, fe.info.file.direntry.Offset + diff);
            //                 var nextEntry = d.DirEntries.FirstOrDefault(x =>
            //                     x.Position == fe.info.file.direntry.Position + 1);
            //                 if (nextEntry == null)
            //                 {
            //                     throw new IOException("Next entry is null");
            //                 }
            //                 fe.info.file.direntry = nextEntry;
            //             }
            //         }
                    }

                    /* check for exnext references */
                    if (fe.type.flags.dir != 0)
                    {
                        var dle = fe.LockEntry;
            //
            //         if (dle.nextentry.dirblock == from.dirblock)
            //         {
            //             if (dle.nextentry.direntry.Position == from.direntry.Position && dle.nextentry.direntry == null)
            //             {
            //                 await GetNextEntry(dle, g);
            //             }
            //             else
            //             {
            //                 /* take only entries after target */
            //                 if (dle.nextentry.direntry.Position > from.direntry.Position)
            //                 {
            //                     //dle.nextentry.direntry = (struct Pfs3DirEntry *)((UBYTE *)dle->nextentry.direntry + diff);
            //                     var d = fe.info.file.dirblock.dirblock;
            //                     // dle.nextentry.direntry =
            //                     //     DirEntryReader.Read(d.entries, dle.nextentry.direntry.Offset + diff);
            //                     var nextEntry = d.DirEntries.FirstOrDefault(x =>
            //                         x.Position == dle.nextentry.direntry.Position);
            //                     if (nextEntry == null)
            //                     {
            //                         throw new IOException("Next entry is null");
            //                     }
            //                     dle.nextentry.direntry = nextEntry;
            //                 }
            //             }
                    }
                }
            }

            return Task.CompletedTask;
        }

        // public static async Task GetNextEntry(lockentry file, globaldata g)
        // {
        //     canode anode = new canode();
        //
        //     /* get nextentry */
        //     var d = file.nextentry.dirblock.dirblock;
        //     // file.nextentry.direntry = Macro.NEXTENTRY(d, file.nextentry.direntry);
        //     file.nextentry.direntry = d.DirEntries.FirstOrDefault(x =>
        //         x.Position == file.nextentry.direntry.Position + 1);
        //
        //     /* no next entry? -> next block */
        //     if (file.nextentry.direntry == null)
        //     {
        //         /* NB: 'nextanode' is een verwarrende naam */
        //         await Pfs3Anodes.GetAnode(anode, file.nextanode, g);
        //         file.nextanode = await GetFirstNonEmptyDE(anode.next, file.nextentry, g);
        //     }
        // }

/* Get first non empty direntry starting from anode [anodenr]
 * Returns {NULL, NULL} if end of dir
 */
        public static async Task<uint> GetFirstNonEmptyDE(uint anodenr, Pfs3FileInfo info, Pfs3GlobalData g)
        {
            Pfs3Canode anode = new Pfs3Canode();
            uint nextsave;
            var found = false;

            anode.next = anodenr;
            while (!found)
            {
                nextsave = anode.next;
                if (nextsave != 0)
                {
                    await Pfs3Anodes.GetAnode(anode, anode.next, g);
                    info.dirblock = await LoadDirBlock(anode.blocknr, g);
                    if (info.dirblock != null)
                    {
                        var d = info.dirblock.dirblock;
                        // info.direntry = Macro.FIRSTENTRY(d);
                        info.direntry = d.DirEntries.FirstOrDefault();
                    }
                }

                if (nextsave == 0 || info.dirblock == null)
                {
                    info.direntry = null;
                    info.dirblock = null;
                    found = true;
                }
                else if (info.direntry != null)
                {
                    Pfs3Macro.Lock(info.dirblock, g);
                    found = true;
                }
            }

            return anode.nr;
        }

        /* MakeDirEntry
 *
 * Used by L2.NewFile, L2.NewDir
 *
 * Make a new directoryentry. The filename is not checked. Allocates anode
 * for file/dir
 *
 * input :
 *        - type: ST_FILE, ST_DIR etc ..
 *        - name: objectname
 *        - entrybuffer: place to put direntry (char buffer of size MAX_ENTRYSIZE)
 *
 * output: - info: objectinfo of new directoryentry
 */
        public static async Task<Pfs3DirEntry> MakeDirEntry(int type, string name,
            Pfs3GlobalData g)
        {
            //ushort entrysize;
            //direntry *direntry;
            //DateTime time;
            //MUFS(struct Pfs3ExtraFields extrafields);

            // entrysize = ((sizeof(struct Pfs3DirEntry) + strlen(name)) & 0xfffe);
            // if (g.dirextension)
            //     entrysize += 2;
            // var direntry = (struct Pfs3DirEntry *)entrybuffer;
            // memset(direntry, 0, entrysize);
            //var direntry = new direntry((byte)Blocks.direntry.EntrySize(name, string.Empty, new extrafields(), g));

#if MULTIUSER
	if (g->muFS_ready)
	{
		extrafields.link = 0;
		extrafields.uid = g->user->uid;
		extrafields.gid = g->user->gid;
		extrafields.prot = muGetDefProtection(g->action->dp_Port->mp_SigTask);
		direntry->protection = extrafields.prot;
		extrafields.prot &= 0xffffff00;
	}
#endif

            uint anode;
            if ((anode = await Pfs3Anodes.AllocAnode(0, g)) == 0)
            {
                return null;
            }

            // direntry.type = (sbyte)type;
            // direntry.fsize = 0;
            // direntry.CreationDate = DateTime.Now;
            // direntry.creationday = (UWORD)time.ds_Days;
            // direntry.creationminute = (UWORD)time.ds_Minute;
            // direntry.creationtick = (UWORD)time.ds_Tick;
            // direntry.protection  = 0x00;    // RWED
            //direntry.nlength = (byte)name.Length;

            // the trailing 0 of strcpy() creates the empty comment!
            // the flags field following this is 0 by the memset call
            //strcpy((UBYTE *)&direntry->startofname, name);
            // direntry.Name = name;

#if MULTIUSER
	if (g->dirextension && g->muFS_ready)
		AddExtraFields(direntry, &extrafields);
#endif
            //DirEntryWriter.Write(entrybuffer, entryIndex, direntry, g);
            // direntry.next = (byte)direntry.EntrySize(direntry, g);

            return new Pfs3DirEntry(0, (sbyte)type, anode, 0, 0, DateTime.Now, name, string.Empty, new Pfs3ExtraFields(), g);
        }

        /* NULL => failure
 * The loaded dirblock is locked immediately (prevents flushing)
 *
 */
        public static async Task<Pfs3CachedBlock> LoadDirBlock(uint blocknr, Pfs3GlobalData g)
        {
            //struct cdirblock *dirblk;
            Pfs3CachedBlock dirblk;
            var volume = g.currentvolume;

            //DB(Trace(1, "LoadDirBlock", "loading block %lx\n", blocknr));
#if DEBUG
            Pfs3Logger.Instance.Debug($"Pfs3Directory: LoadDirBlock Enter, loading block {blocknr}");
#endif
            // -I- check if already in cache
            // if ((dirblk = Lru.CheckCache(volume.dirblks, Constants.HASHM_DIR, blocknr, g)) == null)
            if ((dirblk = Pfs3Lru.CheckCache(volume.dirblks, blocknr, g)) == null)
            {
                // -II- not in cache -> put it in
                dirblk = await Pfs3Lru.AllocLRU(g);

                //DB(Trace(10, "LoadDirBlock", "loading block %lx from disk\n", blocknr));
                //var blk =
                if ((dirblk.blk = await Pfs3Disk.RawRead<Pfs3DirBlock>(g.currentvolume.rescluster, blocknr, g)) != null)
                {
                    if (dirblk.blk.id == Pfs3Constants.DBLKID)
                    {
                        dirblk.volume = g.currentvolume;
                        dirblk.blocknr = blocknr;
                        dirblk.used = 0;
                        dirblk.changeflag = false;
                        //Macro.Hash(dirblk, volume.dirblks, Constants.HASHM_DIR);
                        Pfs3Macro.Hash(dirblk, volume.dirblks);
                        Pfs3Lru.UpdateReference(blocknr, dirblk, g); // %10
                    }
                    else
                    {
                        // ULONG args[2];
                        // args[0] = dirblk->blk.id;
                        // args[1] = blocknr;
                        Pfs3Lru.FreeLRU(dirblk, g);
                        // ErrorMsg(AFS_ERROR_DNV_WRONG_DIRID, args, g);
                        // return NULL;
                    }
                }
                else
                {
                    Pfs3Lru.FreeLRU(dirblk, g);
                    //DB(Trace(5, "LoadDirBlock", "loading block %lx failed\n", blocknr));
                    // ErrorMsg(AFS_ERROR_DNV_LOAD_DIRBLOCK, NULL, g);    // #$%^&??
                    // DebugOn;DebugMsgNum("blocknr", blocknr);
                    return null;
                }
            }

            // EXIT("LoadDirBlock");
#if DEBUG
            Pfs3Logger.Instance.Debug("Pfs3Directory: LoadDirBlock Exit");
#endif
            return dirblk;
        }

/*
 * GetExtraFields (normal file only)
 */
        // public static extrafields GetExtraFields(byte[] entries, direntry direntry)
        // {
        //     var extrafields = DirEntryReader.ReadExtraFields(entries, direntry.Offset, direntry);
        //
        //     /* patch protection lower 8 bits */
        //     extrafields.prot |= direntry.protection;
        //
        //     return extrafields;
        // }

        /// <summary>
        /// Get del dir entry file size
        /// </summary>
        /// <param name="dde"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static uint GetDDFileSize(Pfs3DelDirEntry dde, Pfs3GlobalData g)
        {
            if (!Pfs3Constants.LARGE_FILE_SIZE || !g.largefile || dde.filename[0] > Pfs3Constants.DELENTRYFNSIZE)
                return dde.fsize;
// #if LARGE_FILE_SIZE
// 	else
// 		return dde->fsize | ((FSIZE)dde->fsizex << 32);
// #endif
            return 0;
        }

        /// <summary>
        /// Get dir entry file size
        /// </summary>
        /// <param name="direntry"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static uint GetDEFileSize(Pfs3DirEntry direntry, Pfs3GlobalData g)
        {
            if (!Pfs3Constants.LARGE_FILE_SIZE || !g.largefile)
                return direntry.fsize;
// #if LARGE_FILE_SIZE
// 	else {
// 		struct Pfs3ExtraFields extrafields;
// 		GetExtraFields(direntry, &extrafields);
// 		return direntry->fsize | ((FSIZE)extrafields.fsizex << 32);
// 	}
// #endif
            return 0;
        }

        /* GetFullPath converts a relative path to an absolute path.
 * The fileinfo of the new path is returned in [result].
 * The return value is the filename without path.
 * Error: return 0
 *
 * Parsing Syntax:
 * : after '/' or at the beginning ==> root
 * : after [name] ==> volume [name]
 * / after / or ':' or at the beginning ==> parent
 * / after dir ==> get dir
 * / after file ==> error (ALWAYS) (AMIGADOS ok if LAST file)
 *
 * IN basispath, filename, g
 * OUT fullpath, error
 *
 * If only a partial path is found, a pointer to the unparsed part
 * will be stored in g->unparsed.
 */
    }
}
