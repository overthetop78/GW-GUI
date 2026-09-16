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
        public static async Task NewFile(bool found, Pfs3ObjectInfo directory, string filename, Pfs3ObjectInfo newfile,
            bool overwrite, Pfs3GlobalData g)
        {
            Pfs3ObjectInfo info = new Pfs3ObjectInfo();
            uint anodenr;
            int entryindex = 0;
            //byte[] entrybuffer = new byte[Macro.MAX_ENTRYSIZE];
            Pfs3ExtraFields extrafields = new Pfs3ExtraFields();
            Pfs3DirEntry destentry;
            Pfs3Canode anode = new Pfs3Canode();
            int l;
// #if VERSION23
            Pfs3AnodeChain achain;
// #endif

            if (found && overwrite && !Pfs3Macro.IsFile(newfile))
            {
                throw new NotAFileException($"Overwrite existing file '{filename}' failed, entry is not a file or link");
            }

            //DB(Trace(10, "NewFile", "%s\n", filename));
            /* check disk-writeprotection etc */
            /* check disk-writeprotection etc */
            Pfs3VolumeOperations.CheckVolume(g.currentvolume, true, g);

// #if DELDIR
            if (Pfs3Macro.IsDelDir(directory))
            {
                throw new IOException("ERROR_WRITE_PROTECTED");
            }
// #endif

            /* check reserved area lock */
            if (Pfs3Macro.ReservedAreaIsLocked(g))
            {
                throw new IOException("ERROR_DISK_FULL");
            }

            /* truncate filename to 31 characters */
            if ((l = filename.Length) == 0)
            {
                throw new IOException("ERROR_INVALID_COMPONENT_NAME");
            }

            var fileNameSize = Pfs3Macro.FILENAMESIZE(g);
            if (l > fileNameSize - 1)
            {
                filename = filename.Substring(fileNameSize - 1);
            }

            if (found)
            {
                if (!overwrite)
                {
                    throw new PathAlreadyExistsException($"Path '{filename}' already exists");
                }
                /*
                 * new version: take over direntry
                 * (used to simply delete old and make new)
                 */
                info.file = newfile.file;
                info.volume = newfile.volume;
                anodenr = Pfs3Macro.FIANODENR(info.file);

                /* Check deleteprotection */
                if (Pfs3Macro.IsVolume(info) ||
                    (!g.IgnoreProtectionBits && (info.file.direntry.protection & Pfs3Constants.FIBF_DELETE) == Pfs3Constants.FIBF_DELETE))
                {
                    throw new IOException("ERROR_DELETE_PROTECTED");
                }

                /* If link, get real object. After this it has become a
                 * ST_FILE
                 */
                if ((int)info.file.direntry.type == Pfs3Constants.ST_LINKFILE ||
                    ((int)info.file.direntry.type == Pfs3Constants.ST_LINKDIR))
                {
                    Pfs3Canode linknode = new Pfs3Canode();

                    // var dirBlock = info.file.dirblock.dirblock;
                    // extrafields = GetExtraFields(dirBlock.entries, info.file.direntry);
                    extrafields = info.file.direntry.GetExtraFields();
                    anodenr = extrafields.link;
                    await Pfs3Anodes.GetAnode(linknode, info.file.direntry.anode, g);
                    if (!await Pfs3Lock.FetchObject(linknode.clustersize, anodenr, info, g))
                    {
                        throw new IOException("ERROR_OBJECT_NOT_FOUND");
                    }

                    /* have to check protection again */
                    if (!g.IgnoreProtectionBits && (info.file.direntry.protection & Pfs3Constants.FIBF_DELETE) == Pfs3Constants.FIBF_DELETE)
                    {
                        throw new IOException("ERROR_DELETE_PROTECTED");
                    }

                    /* get parent */
                    if (!await GetParent(info, directory, g))
                    {
                        throw new IOException("ERROR_OBJECT_NOT_FOUND");
                    }
                }

                /* Check if there are outstanding locks on object */
                var node = Pfs3Macro.HeadOf(g.currentvolume.fileentries);
                if (node != null && node.Value is Pfs3ListEntry le && Pfs3Lock.ScanLockList(le, anodenr))
                {
                    //DB(Trace(1, "NewFile", "object in use"));
                    throw new IOException("ERROR_OBJECT_IN_USE");
                }

                if ((achain = await Pfs3Anodes.GetAnodeChain(anodenr, g)) == null)
                    throw new IOException("ERROR_NO_FREE_STORE");

                /* Free used space */
                if (g.deldirenabled && (int)info.file.direntry.type == Pfs3Constants.ST_FILE)
                {
                    int ddslot;

                    /* free a slot to put old version in, inter. update possible */
                    ddslot = await AllocDeldirSlot(g);

                    /* make replacement anode, because we want to reuse the old one */
                    achain.head.an.nr = await Pfs3Anodes.AllocAnode(0, g);
                    info.file.direntry.SetAnode(achain.head.an.nr);
                    await Pfs3Anodes.SaveAnode(achain.head.an, achain.head.an.nr, g);
                    await AddToDeldir(info, ddslot, g);
                    info.file.direntry.SetAnode(anodenr);
                }

                /* Rollover files are essentially just 'reset' by overwriting:
                 * only the virtualsize and offset are set to zero (extrafields)
                 * Other files are deleted and recreated as a new file.
                 */
                if (info.file.direntry.type != Pfs3Constants.ST_ROLLOVERFILE)
                {
                    /* Change directory entry */
                    info.file.direntry = SetDEFileSize(info.file.dirblock.dirblock, info.file.direntry, 0, g);
                    info.file.direntry.SetType(Pfs3Constants.ST_FILE);
                    await Pfs3Update.MakeBlockDirty(info.file.dirblock, g);

                    /* Reclaim anode */
                    anode.clustersize = 0;
                    anode.blocknr = 0xffffffff;
                    anode.next = 0;
                    await Pfs3Anodes.SaveAnode(anode, anodenr, g);

                    /* Delete old file (update possible) */
                    if (g.deldirenabled && (int)info.file.direntry.type == Pfs3Constants.ST_FILE)
                    await Pfs3Allocation.FreeBlocksAC(achain, Pfs3Constants.ULONG_MAX, Pfs3FreeBlockType.keepanodes, g);
                    else
                    await Pfs3Allocation.FreeBlocksAC(achain, Pfs3Constants.ULONG_MAX, Pfs3FreeBlockType.freeanodes, g);
                    Pfs3Anodes.DetachAnodeChain(achain, g);
                }

                /* Clear direntry extrafields */
                //destentry = (struct Pfs3DirEntry *)entrybuffer;
                //destentry = DirEntryReader.Read(entrybuffer, entryindex);
                //memcpy(destentry, info.file.direntry, info.file.direntry.next);
                // NOTE: The 3 previous out commented lines makes new destentry using blank entrybuffer,
                // then copies data from info.file.direntry to destentry.
                // This is replaced by just reading info.file.direntry as destentry.
                // destentry = DirEntryReader.Read(info.file.dirblock.dirblock.entries, info.file.direntry.Offset);
                destentry = info.file.direntry;
                // extrafields = GetExtraFields(info.file.dirblock.dirblock.entries, info.file.direntry);
                extrafields = info.file.direntry.GetExtraFields();
                extrafields.SetVirtualSize(0);
                extrafields.SetRollPointer(0);
                //AddExtraFields(info.file.dirblock.dirblock.entries, destentry, extrafields);
                destentry.SetExtraFields(extrafields, g);
                await ChangeDirEntry(info, destentry, directory, info.file, g);
                newfile.file = info.file;
                return;
            }

            /* direntry alloceren en invullen */
            var entry = await MakeDirEntry(Pfs3Constants.ST_FILE, filename, g);
            if (entry != null)
            {
                if (await AddDirectoryEntry(directory, entry, newfile.file, g))
                {
                    newfile.volume.root = 1;
                    return;
                }
                else
                {
                    await Pfs3Anodes.FreeAnode(entry.anode, g);
                }
            }

            throw new IOException("ERROR_DISK_FULL");
        }

/* NewDir
 *
 * Specification:
 *
 * - make new dir
 * - returns fileentry (!) with exclusive lock
 *
 * Implementation:
 *
 * - check if file/dir exists
 * - make direntry
 * - make first dirblock
 *
 * Similar to NewFile()
 *
 * maxneeds: 2 nd, 3 na = 2 nablk : 4 res
 */
        public static async Task<IPfs3Entry> NewDir(Pfs3ObjectInfo parent, string dirname, Pfs3GlobalData g)
        {
            Pfs3ObjectInfo info = new Pfs3ObjectInfo
            {
                file = new Pfs3FileInfo()
            };
            IPfs3Entry fileentry;
            Pfs3ListType type = new Pfs3ListType();
            Pfs3CachedBlock blk;
            uint parentnr, blocknr;
            // byte[] entrybuffer = new byte[Macro.MAX_ENTRYSIZE];
            int l;

            /* check disk-writeprotection etc */
            Pfs3VolumeOperations.CheckVolume(g.currentvolume, true, g);

// #if DELDIR
            if (Pfs3Macro.IsDelDir(parent))
            {
                //*error = ERROR_WRITE_PROTECTED;
                return null;
            }
// #endif

            /* check reserved area lock */
            if (Pfs3Macro.ReservedAreaIsLocked(g))
            {
                throw new IOException("ERROR_DISK_FULL");
            }

            /* checkvolume */
            if (Pfs3Macro.IsVolume(parent))
                parentnr = (uint)Pfs3Macro.ANODE_ROOTDIR;
            else
                parentnr = parent.file.direntry.anode;

            /* truncate dirname to 31 characters */
            if ((l = dirname.Length) == 0)
            {
                throw new IOException("ERROR_INVALID_COMPONENT_NAME");
            }

            if (l > g.fnsize - 1)
            {
                dirname = dirname.Substring(g.fnsize - 1);
            }

            /* check if object exists */
            if (await SearchInDir(parentnr, dirname, info, g))
            {
                throw new IOException("ERROR_OBJECT_EXISTS");
            }

            /* allocate directory entry, fill it. Make fileentry */
            // var entryindex = 0;
            var de = await MakeDirEntry(Pfs3Constants.ST_USERDIR, dirname, g);
            if (de == null)
            {
                // goto error1;
                throw new IOException("ERROR_DISK_FULL");
            }

            //var de = DirEntryReader.Read(entrybuffer, entryindex, g);
            if (!await AddDirectoryEntry(parent, de, info.file, g))
            {
                //FreeAnode(((struct Pfs3DirEntry *)entrybuffer)->anode, g);
                await Pfs3Anodes.FreeAnode(de.anode, g);
                // error1:
                throw new IOException("ERROR_DISK_FULL");
            }

            type.value = Pfs3Constants.ET_LOCK | Pfs3Constants.ET_EXCLREAD;
            fileentry = await Pfs3Lock.MakeListEntry(info, type, g);
            if (fileentry == null)
            {
                // goto error2;
                return await DiskFullError(info, null, g);
            }

            if (!Pfs3Lock.AddListEntry(fileentry.ListEntry, g)) /* Should never fail, accessconflict impossible */
            {
                //ErrorMsg(AFS_ERROR_NEWDIR_ADDLISTENTRY, NULL, g);
                // goto error2;
                return await DiskFullError(info, fileentry, g);
            }

            /* Make first directoryblock (needed for parentfinding) */
            if ((blocknr = Pfs3Allocation.AllocReservedBlock(g)) == 0)
            {
                //*error = ERROR_DISK_FULL;
                // error2:
                return await DiskFullError(info, fileentry, g);
                // await Pfs3Anodes.FreeAnode(info.file.direntry.anode, g);
                // await RemoveDirEntry(info, g);
                // if (fileentry != null)
                //     Pfs3Lock.FreeListEntry(fileentry, g);
                // //DB(Trace(1, "Newdir", "disk full"));
                // throw new IOException("disk full");
            }

            blk = await MakeDirBlock(blocknr, info.file.direntry.anode, info.file.direntry.anode, parentnr, g);


            //return fileentry as lockentry;
            //throw new NotImplementedException("convert fileentry to lockentry?");
            return fileentry;
        }

        private static async Task<IPfs3Entry> DiskFullError(Pfs3ObjectInfo info, IPfs3Entry fileentry, Pfs3GlobalData g)
        {
            await Pfs3Anodes.FreeAnode(info.file.direntry.anode, g);
            await RemoveDirEntry(info, g);
            if (fileentry != null)
                Pfs3Lock.FreeListEntry(fileentry, g);
            //DB(Trace(1, "Newdir", "disk full"));
            throw new DiskFullException("Disk full");
        }

/*
 * Get deldirentry deldirentrynr (NO CHECK ON VALIDITY
 * deldir is assumed present and enabled
 */
        public static async Task<Pfs3DelDirEntry> GetDeldirEntryQuick(uint ddnr, Pfs3GlobalData g)
        {
            Pfs3CachedBlock ddblk;

            /* get deldirentry */
            if ((ddblk = await GetDeldirBlock((ushort)(ddnr / Pfs3Constants.DELENTRIES_PER_BLOCK), g)) == null)
                return null;

            var blk = ddblk.deldirblock;
            return blk.entries[ddnr % Pfs3Constants.DELENTRIES_PER_BLOCK];
        }

        public static async Task<Pfs3CachedBlock> GetDeldirBlock(ushort seqnr, Pfs3GlobalData g)
        {
            var volume = g.currentvolume;
            Pfs3CachedBlock rext;
            Pfs3CachedBlock ddblk;
            uint blocknr;

            rext = volume.rblkextension;

            if (seqnr > Pfs3Constants.MAXDELDIR)
            {
                //DB(Trace(5,"GetDeldirBlock","seqnr out of range = %lx\n", seqnr));
                throw new IOException("AFS_ERROR_DELDIR_INVALID");
            }

            /* get blocknr */
            var rext_blk = rext.rblkextension;
            if ((blocknr = rext_blk.deldir[seqnr]) == 0)
            {
                //DB(Trace(5,"GetDeldirBlock","ERR: index zero\n"));
                throw new IOException("AFS_ERROR_DELDIR_INVALID");
            }

            /* check cache */
            // for (var node = Macro.HeadOf(volume.deldirblks); node != null; node = node.Next)
            // {
            //     ddblk = node.Value;
            //     var ddblk_blk = ddblk.deldirblock;
            //     if (ddblk_blk.seqnr == seqnr)
            //     {
            //         Lru.MakeLRU(ddblk, g);
            //         return ddblk;
            //     }
            // }
            if (volume.deldirblksBySeqNr.ContainsKey(seqnr))
            {
                ddblk = volume.deldirblksBySeqNr[seqnr];
                Pfs3Lru.MakeLRU(ddblk, g);
                return ddblk;
            }

            /* alloc cache */
            if ((ddblk = await Pfs3Lru.AllocLRU(g)) == null)
            {
                //DB(Trace(5,"GetDeldirBlock","ERR: alloclru failed\n"));
                return null;
            }

            /* read block */
            if ((ddblk.blk = await Pfs3Disk.RawRead<Pfs3DelDirBlock>(g.currentvolume.rescluster, blocknr, g)) == null)
            {
                Pfs3Lru.FreeLRU(ddblk, g);
                return null;
            }

            /* check it */
            if (ddblk.deldirblock.id != Pfs3Constants.DELDIRID)
            {
                // ErrorMsg (AFS_ERROR_DELDIR_INVALID, NULL, g);
                Pfs3Lru.FreeLRU(ddblk, g);
                //volume.rootblk.Options ^= Constants.MODE_DELDIR;
                g.RootBlock.Options ^= Pfs3RootBlock.Pfs3DiskOptions.MODE_DELDIR;
                g.deldirenabled = false;
            }

            /* initialize it */
            ddblk.volume = volume;
            ddblk.blocknr = blocknr;
            ddblk.used = 0;
            ddblk.changeflag = false;

            /* add to cache and return */
            // Macro.MinAddHead(volume.deldirblks, ddblk);
            Pfs3Macro.AddToIndexes(volume.deldirblks, volume.deldirblksBySeqNr, ddblk);
            return ddblk;
        }

/* RemoveDirEntry
 *
 * Simply shift the directryentry out with memmove(dest, src, len)
 * References are not corrected (see changedirentry)
 *
 * makes all fileinfo's in same block invalid !!
 */
    }
}
