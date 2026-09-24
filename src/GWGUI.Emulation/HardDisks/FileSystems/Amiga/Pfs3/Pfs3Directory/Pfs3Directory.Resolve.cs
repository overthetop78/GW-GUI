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
        public static async Task<string> GetFullPath(Pfs3ObjectInfo basispath, string filename, Pfs3ObjectInfo fullpath, Pfs3GlobalData g)
        {
	        bool eop = false, success;
	        Pfs3VolumeData volume;

            // VVV Init:getrootvolume
            //ENTER("GetFullPath");
            //g.unparsed = NULL;
            /* Set base path */
            fullpath.OverwriteWith(Pfs3Macro.IsDirEntry(basispath) && !filename.StartsWith("/")
                ? basispath : await GetRoot(g));

            /* The basispath should not be a file
             * BTW: softlink is illegal too, but not possible
             */
            if (Pfs3Macro.IsFile(fullpath) || Pfs3Macro.IsDelFile(fullpath))
	        {
		        throw new IOException("ERROR_OBJECT_WRONG_TYPE");
	        }

	        /* check if device present */
            if (Pfs3Macro.IsVolume(fullpath) || Pfs3Macro.IsDelDir(fullpath))
            {
                volume = fullpath.volume.volume;
            }
            else
            {
                volume = fullpath.file.dirblock.volume;
            }

            Pfs3VolumeOperations.CheckVolume(volume, false, g);

            /* extend base-path using filename and
             * continue until path complete (eop = end of path)
             */
            // while (!eop)
            // {
            //     pathpart = filename;
            //     index = strcspn(filename, "/:");
            //     parttype = filename[index];
            //     filename[index] = 0x0;
            //
            //     switch (parttype)
            //     {
            //         case ':':
            //             success = FALSE;
            //             break;
            //
            //         case '/':
            //             if (*pathpart == 0x0)
            //             {
            //                 // if already at root, fail with an error
            //                 if (IsVolume(*fullpath))
            //                 {
            //                     *error = ERROR_OBJECT_NOT_FOUND;
            //                     success = FALSE;
            //                     break;
            //                 }
            //                 success = GetParentOf(fullpath, error, g);
            //             }
            //             else
            //                 success = GetDir(pathpart, fullpath, error, g);
            //             break;
            //
            //         default:
            //             eop = TRUE;
            //     }
            //
            //     filename[index] = parttype;
            //
            //     if (!success)
            //     {
            //         /* return pathrest for readlink() */
            //         if (*error == ERROR_IS_SOFT_LINK)
            //             g->unparsed = filename + index;
            //         else if (*error == ERROR_OBJECT_NOT_FOUND)
            //             g->unparsed = filename;
            //         return NULL;
            //     }
            //
            //     if (!eop)
            //         filename += index + 1;
            // }
            //
            // return filename;

            if (filename.IndexOf(':') >= 0)
            {
                return null;
            }

            var isRoot = filename.StartsWith("/");
            if (isRoot)
            {
                filename = filename.Substring(1);
            }

            var pathParts = filename.Split('/');
            foreach (var pathPart in pathParts.Take(pathParts.Length - 1))
            {
                if (!await GetDir(pathPart, fullpath, g))
                {
                    return null;
                }
            }

            return pathParts[pathParts.Length - 1];
        }

        /// <summary>
        /// Find object info for path. Returns remaining parts not found
        /// </summary>
        /// <param name="current">Current directory</param>
        /// <param name="path">Relative or absolute path to object</param>
        /// <param name="g"></param>
        /// <exception cref="IOException"></exception>
        public static async Task<string[]> Find(Pfs3ObjectInfo current, string path, Pfs3GlobalData g)
        {
            var isRoot = path.StartsWith("/");
            if (isRoot)
            {
                current.OverwriteWith(await GetRoot(g));
            }

            var parts = (isRoot ? path.Substring(1) : path).Split('/');

            int i;
            for (i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                if (!await GetObject(part, current, g))
                {
                    break;
                }

                current.volume.root = 1;
            }

            return parts.Skip(i).ToArray();
        }

        public static Task<Pfs3ObjectInfo> GetRoot(Pfs3GlobalData g)
        {
            // CHANGE: Commented out UpdateCurrentDisk as this is only need when used on an Amiga
            // changing from one partition to another.
            // await Pfs3VolumeOperations.UpdateCurrentDisk(g);

            return Task.FromResult(new Pfs3ObjectInfo
            {
                deldir = new Pfs3DelDirInfo
                {
                },
                delfile = new Pfs3DelFileInfo
                {
                },
                volume = new Pfs3VolumeInfo
                {
                    root = 0,
                    volume = g.currentvolume
                },
                // file = new fileinfo
                // {
                // }
            });
        }

/* pre: - path <> 0 and volume or directory
 * result back in path
 */
        public static async Task<bool> GetParentOf(Pfs3ObjectInfo path, Pfs3GlobalData g)
        {
            var info = path;
            return await GetParent(info, path, g);
        }

/* pre: - path <> 0 and volume of directory
 *      - dirname without path; strlen(dirname) > 0
 * result back in path
 */
        public static async Task<bool> GetDir(string dirname, Pfs3ObjectInfo path, Pfs3GlobalData g)
        {
            bool found;

            found = await GetObject(dirname, path, g);

// #if DELDIR
            if (g.deldirenabled && Pfs3Macro.IsDelDir(path))
                return true;
// #endif

            /* check if found directory */
// #if DELDIR
            if (!found || Pfs3Macro.IsFile(path) || Pfs3Macro.IsDelFile(path))
// #else
//             if (!found || IsFile(*path))
// #endif
            {
                throw new IOException("ERROR_OBJECT_NOT_FOUND"); // DOPUS doesn't like DIR_NOT_FOUND
            }

            /* check if softlink */
            if (Pfs3Macro.IsSoftLink(path))
            {
                throw new IOException("ERROR_IS_SOFT_LINK");
            }

            /* resolve links */
            if (path.file.direntry.type == Pfs3Constants.ST_LINKDIR)
            {
                Pfs3ExtraFields extrafields = new Pfs3ExtraFields();
                Pfs3Canode linknode = new Pfs3Canode();

                // var dirBlock = path.file.dirblock.dirblock;
                // extrafields = GetExtraFields(dirBlock.entries, path.file.direntry);
                extrafields = path.file.direntry.GetExtraFields();
                await Pfs3Anodes.GetAnode(linknode, path.file.direntry.anode, g);
                if (!await Pfs3Lock.FetchObject(linknode.clustersize, extrafields.link, path, g))
                    return false;
            }

            return true;
        }

/* pre: - path<>0 and volume of directory
 *      - objectname without path; strlen(objectname) > 0
 * result back in path
 */
        public static async Task<bool> GetObject(string objectname, Pfs3ObjectInfo path, Pfs3GlobalData g)
        {
            uint anodenr;
            bool found;

            // #if DELDIR
            if (Pfs3Macro.IsDelDir(path))
            {
                found = (await SearchInDeldir(objectname, path, g) != null);
                goto go_error;
            }
// #endif

            if (Pfs3Macro.IsVolume(path))
                anodenr = Pfs3Constants.ANODE_ROOTDIR;
            else
                anodenr = path.file.direntry.anode;

            //DB(Trace(1, "GetObject", "parent anodenr %lx\n", anodenr));
            found = await SearchInDir(anodenr, objectname, path, g);

            go_error:
            if (!found)
            {
// #if DELDIR
                if (g.deldirenabled && Pfs3Macro.IsVolume(path))
                {
                    if (Pfs3Constants.deldirname.Equals(objectname, StringComparison.OrdinalIgnoreCase))
                    {
                        path.deldir.special = Pfs3Constants.SPECIAL_DELDIR;
                        path.deldir.volume = g.currentvolume;
                        return true;
                    }
                }

// #endif
                //throw new IOException("ERROR_OBJECT_NOT_FOUND");
                return false;
            }

            return true;
        }

/* SearchInDeldir
 *
 * Search an object in the del-directory and return the objectinfo if found
 *
 * input : - delname: name of object to be searched for
 * output: - result: the searched for object
 * result: deldirentry * or NULL
 */
        public static async Task<Pfs3DelDirEntry> SearchInDeldir(string delname, Pfs3ObjectInfo result, Pfs3GlobalData g)
        {
            Pfs3DelDirEntry dde;
            Pfs3CachedBlock dblk;
            int delnumptr;
            //UBYTE intl_name[PATHSIZE];
            uint slotnr, offset;

            //ENTER("SearchInDeldir");
            if ((delnumptr = delname.LastIndexOf(Pfs3Constants.DELENTRY_SEP)) <= -1)
                return null; /* no delentry seperator */
            //stcd_i(delnumptr + 1, (int *) &slotnr);  /* retrieve the slotnr  */
            slotnr = (uint)(delnumptr + 1);

            delnumptr = 0; /* patch string to get filename part  */
            //ctodstr(delname, intl_name);
            var intl_name = delname;

            /* truncate to maximum length */
            if (intl_name.Length > g.fnsize)
            {
                intl_name = intl_name.Substring(g.fnsize);
            }

            // intltoupper(intl_name);     /* international uppercase objectname */
            intl_name = AmigaTextHelper.ToUpper(intl_name, true);
            delnumptr = Pfs3Constants.DELENTRY_SEP;

            /* 4.3: get deldir block */
            if ((dblk = await GetDeldirBlock((ushort)(slotnr / Pfs3Constants.DELENTRIES_PER_BLOCK), g)) == null)
            {
                return null;
            }

            offset = slotnr % Pfs3Constants.DELENTRIES_PER_BLOCK;

            var blk = dblk.deldirblock;
            dde = blk.entries[offset];
            if (intl_name.Equals(dde.filename, StringComparison.OrdinalIgnoreCase))
            {
                if (!await IsDelfileValid(dde, dblk, g))
                    return null;

                result.delfile.special = Pfs3Constants.SPECIAL_DELFILE;
                result.delfile.slotnr = slotnr;
                Pfs3Macro.Lock(dblk, g);
                return dde;
            }

            return null;
        }

        /*
 * Test if delfile is valid by scanning it's blocks
 */
        public static async Task<bool> IsDelfileValid(Pfs3DelDirEntry dde, Pfs3CachedBlock ddblk, Pfs3GlobalData g)
        {
            Pfs3Canode anode = new Pfs3Canode();

            /* check if deldirentry actually used */
            if (dde.anodenr == 0)
            {
                return false;
            }

            /* scan all blocks in the anodelist for validness */
            for (anode.nr = dde.anodenr; anode.nr > 0; anode.nr = anode.next)
            {
                await Pfs3Anodes.GetAnode(anode, anode.nr, g);
                if (await BlockTaken(anode, g))
                {
                    /* free attached anodechain */
                    await FreeAnodesInChain(dde.anodenr, g); /* only FREE anodes, not blocks!! */
                    dde.anodenr = 0;
                    await Pfs3Update.MakeBlockDirty(ddblk, g);
                    return false;
                }
            }

            return true;
        }

        /*
 * Check if the blocks referenced by an anode are taken
 */
        public static async Task<bool> BlockTaken(Pfs3Canode anode, Pfs3GlobalData g)
        {
            uint size, bmoffset, bmseqnr, field, i, j, blocknr;
            Pfs3CachedBlock bitmap;
            var allocData = g.glob_allocdata;

            i = (anode.blocknr - allocData.bitmapstart) / 32; // longwordnr
            size = (anode.clustersize + 31) / 32;
            bmseqnr = i / allocData.longsperbmb;
            bmoffset = i % allocData.longsperbmb;

            while (size > 0)
            {
                /* get first bitmapblock */
            bitmap = await Pfs3Allocation.GetBitmapBlock(bmseqnr, g);

                /* check all blocks */
                while (bmoffset < allocData.longsperbmb)
                {
                    var blk = bitmap.BitmapBlock;
                    /* check all bits in field */
                    field = blk.bitmap[bmoffset];
                    for (i = 0, j = (uint)1 << 31; i < 32; j >>= 1, i++)
                    {
                        if ((field & j) != 0)
                        {
                            /* block is taken, check it out */
                            blocknr = (bmseqnr * allocData.longsperbmb + bmoffset) * 32 + i +
                                      allocData.bitmapstart;
                            if (blocknr >= anode.blocknr && blocknr < anode.blocknr + anode.clustersize)
                                return true;
                        }
                    }

                    bmoffset++;
                    if ((--size) == 0)
                        break;
                }

                /* get ready for next block */
                bmseqnr = (bmseqnr + 1) % (allocData.no_bmb);
                bmoffset = 0;
            }

            return false;
        }

        /*
 * Get a >valid< deldirentry starting from deldirentrynr ddnr
 * deldir is assumed present and enabled
 */
        public static async Task<Pfs3DelDirEntry> GetDeldirEntry(int ddnr, Pfs3GlobalData g)
        {
            var rext = g.currentvolume.rblkextension;
            Pfs3CachedBlock ddblk;
            Pfs3DelDirEntry dde;
            var blk = rext.rblkextension;
            var maxdelentrynr = blk.deldirsize * Pfs3Constants.DELENTRIES_PER_BLOCK - 1;
            ushort oldlock;

            while (ddnr <= maxdelentrynr)
            {
                /* get deldirentry */
                if ((ddblk = await GetDeldirBlock((ushort)(ddnr / Pfs3Constants.DELENTRIES_PER_BLOCK), g)) == null)
                    break;

                oldlock = ddblk.used;
                Pfs3Macro.Lock(ddblk, g);
                var ddblk_blk = ddblk.deldirblock;
                //dde = DelDirEntryReader.Read(ddblk_blk.entries, ddnr % Constants.DELENTRIES_PER_BLOCK);
                dde = ddblk_blk.entries[ddnr % Pfs3Constants.DELENTRIES_PER_BLOCK];

                /* check if dde valid */
                if (await IsDelfileValid(dde, ddblk, g))
                {
                    /* later --> check if blocks retaken !! */
                    /* can be done by scanning bitmap!!     */
                    return dde;
                }

                ddnr++;
                ddblk.used = oldlock;
            }

            /* nothing found */
            return null;
        }

        public static async Task<IEnumerable<Pfs3DirEntry>> GetDirEntries(uint dirnodenr, Pfs3GlobalData g)
        {
            Pfs3Canode anode = new Pfs3Canode();
            var eod = false;
            uint anodeoffset;
            var dirEntries = new List<Pfs3DirEntry>();

            await Pfs3Anodes.GetAnode(anode, dirnodenr, g);
            anodeoffset = 0;
            var dirblock = await LoadDirBlock(anode.blocknr, g);
            var blk = dirblock.dirblock;
            //var maxDirEntries = CalculateMaxDirEntries(blk);

            while (blk != null && !eod) /* eod stands for end-of-dir */
            {
                foreach(var dirEntry in blk.DirEntries)
                {
                    if (g.ResolveLinkPaths &&
                        (dirEntry.type == Pfs3Constants.ST_LINKFILE || dirEntry.type == Pfs3Constants.ST_LINKDIR))
                    {
                        // create link object from dir entry
                        var linkObject = new Pfs3ObjectInfo
                        {
                            file = new Pfs3FileInfo
                            {
                                direntry = dirEntry,
                                dirblock = dirblock
                            }
                        };

                        var type = new Pfs3ListType
                        {
                            value = Pfs3Constants.ET_FILEENTRY
                        };

                        // get object the dir entry links to
                        IPfs3Entry fileFe;
                        if ((fileFe = await Pfs3Lock.MakeListEntry(linkObject, type, g)) == null)
                        {
                            throw new IOException("make list entry error");
                        }
                        Pfs3Lock.RemoveListEntry(fileFe, g);

                        // create current dir object
                        var currentDirEntry = new Pfs3DirEntry(0);
                        currentDirEntry.SetAnode(dirnodenr);
                        var currentDirObject = new Pfs3ObjectInfo
                        {
                            file = new Pfs3FileInfo
                            {
                                direntry = currentDirEntry,
                                dirblock = dirblock
                            }
                        };

                        // get path of linked object
                        var linkPath = await GetPath(g, fileFe.ListEntry.info, currentDirObject);
                        dirEntry.SetLinkPath(string.Concat(linkPath, string.IsNullOrEmpty(linkPath) ? string.Empty : "/",
                            fileFe.ListEntry.info.file.direntry.Name));
                    }

                    dirEntries.Add(dirEntry);
                }

                /* load next block */
                var result = await Pfs3Anodes.NextBlock(anode, anodeoffset, g);
                anodeoffset = result.Item2;
                if (result.Item1)
                {
                    dirblock = await LoadDirBlock(anode.blocknr + anodeoffset, g);
                    blk = dirblock.dirblock;
                }
                else
                {
                    eod = true;
                }
            }

            return dirEntries;
        }

/* Allocate deldirslot. Free anodechain attached to slot and clear it.
 * An intermediate update is possible, due to FreeAnodesInChain()
 */
        public static async Task<int> AllocDeldirSlot(Pfs3GlobalData g)
        {
            Pfs3CachedBlock rext = g.currentvolume.rblkextension; // crootblockextension
            Pfs3CachedBlock ddblk; // cdeldirblock
            Pfs3DelDirEntry dde;
            int ddnr = 0;
            uint anodenr;

            /* get deldirentry and update roving ptr */
            var rextBlk = rext.rblkextension;
            ddnr = rextBlk.deldirroving;
            if ((ddblk = await GetDeldirBlock((ushort)(ddnr / Pfs3Constants.DELENTRIES_PER_BLOCK), g)) == null)
            {
                rextBlk.deldirroving = 0;
                return 0;
            }

            var ddblkBlk = ddblk.deldirblock;
            dde = ddblkBlk.entries[ddnr % Pfs3Constants.DELENTRIES_PER_BLOCK];
            rextBlk.deldirroving =
                (ushort)((rextBlk.deldirroving + 1) % (rextBlk.deldirsize * Pfs3Constants.DELENTRIES_PER_BLOCK));
            await Pfs3Update.MakeBlockDirty(ddblk, g);

            anodenr = dde.anodenr;
            if (anodenr != 0)
            {
                /* clear it for reuse */
                dde.anodenr = 0;

                /* free attached anodechain */
                await FreeAnodesInChain(anodenr, g); /* only FREE anodes, not blocks!! */
            }

            // DB(Trace(1, "AllocDelDirSlot", "Allocate slot %ld\n", ddnr));
            return ddnr;
        }

/* Add a file to the deldir.
 * Deldir assumed enabled here, and info assumed a file (ST_FILE)
 * ddnr is deldir slot to use. Slot is assumed to be allocated by
 * AllocDeldirSlot()
 */
        public static async Task AddToDeldir(Pfs3ObjectInfo info, int ddnr, Pfs3GlobalData g)
        {
            Pfs3CachedBlock ddblk; // cdeldirblock
            Pfs3DelDirEntry dde;
            Pfs3DirEntry de = info.file.direntry;
            Pfs3CachedBlock rext; // crootblockextension
            //struct DateStamp time;

            //DB(Trace(1, "AddToDeldir", "slotnr %ld\n", ddnr));
            /* get deldirentry to put it in */
            ddblk = await GetDeldirBlock((ushort)(ddnr / Pfs3Constants.DELENTRIES_PER_BLOCK), g);
            var ddblkBlk = ddblk.deldirblock;
            dde = ddblkBlk.entries[ddnr % Pfs3Constants.DELENTRIES_PER_BLOCK];

            /* put new one in */
            dde.anodenr = de.anode;
            SetDDFileSize(dde, GetDEFileSize(de, g), g);
            // dde->creationday = de->creationday;
            // dde->creationminute = de->creationminute;
            // dde->creationtick = de->creationtick;
            dde.CreationDate = de.CreationDate;
            dde.filename = de.Name.Substring(0, Math.Min(Pfs3Constants.DELENTRYFNSIZE - 1, (int)de.Name.Length));
            //strncpy(&dde->filename[1], &de->startofname, dde->filename[0]);

            /* Touch deldir block. Inserted here, simply because this the only
             * place touching the deldir will be needed.
             * Note: Only this copy is touched ...
             */
            // DateStamp(&time);
            rext = g.currentvolume.rblkextension;

            // ddblk->blk.creationday = rext->blk.dd_creationday = (UWORD)time.ds_Days;
            // ddblk->blk.creationminute = rext->blk.dd_creationminute = (UWORD)time.ds_Minute;
            // ddblk->blk.creationtick = rext->blk.dd_creationtick = (UWORD)time.ds_Tick;
            var rextBlk = rext.rblkextension;
            ddblkBlk.CreationDate = rextBlk.dd_creationdate;

            /* dirtify block */
            await Pfs3Update.MakeBlockDirty(ddblk, g);
        }

        public static void SetDDFileSize(Pfs3DelDirEntry dde, uint size, Pfs3GlobalData g)
        {
            dde.fsize = size;
#if LARGE_FILE_SIZE
	if (!LARGE_FILE_SIZE || !g->largefile)
		return;
	dde->fsizex = (UWORD)(size >> 32);
#endif
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dirBlock"></param>
        /// <param name="direntry"></param>
        /// <param name="size"></param>
        /// <param name="g"></param>
        /// <returns>Updated direntry</returns>
        public static Pfs3DirEntry SetDEFileSize(Pfs3DirBlock dirBlock, Pfs3DirEntry direntry, uint size, Pfs3GlobalData g)
        {
            //var de = DirEntryReader.Read(dirBlock.entries, direntry.Offset);
            var de = direntry;
            if (!g.largefile)
            {
                de.SetFSize(size);
            }
#if LARGE_FILE_SIZE
	else {
		struct Pfs3ExtraFields extrafields;
		UWORD high = (UWORD)(size >> 32);
		GetExtraFields(direntry, &extrafields);
		if (extrafields.fsizex != high) {
			extrafields.fsizex = high;
			AddExtraFields(direntry, &extrafields);
		}
		direntry->fsize = (ULONG)size;
	}
#endif
            //DirEntryWriter.Write(dirBlock.entries, de.Offset, de);
            return de;
        }

/* Change a directoryentry. Covers all reference changing too

 * If direntry==NULL no new direntry is to be added, only removed.
 * result may be NULL then as well
 *
 * in: from, to, destdir
 * out: result
 *
 * from can become INVALID..
 */

    }
}
