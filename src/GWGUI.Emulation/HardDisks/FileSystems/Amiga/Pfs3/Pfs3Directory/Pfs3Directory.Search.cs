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
        public static async Task RemoveDirEntry(Pfs3ObjectInfo info, Pfs3GlobalData g)
        {
            // int endofblok, startofblok, destofblok, startofclear;
            // ushort clearlen;
            Pfs3ObjectInfo parent = new Pfs3ObjectInfo();

            Pfs3Macro.Lock(info.file.dirblock, g);

            /* change date parent %6.5 */
            if (await GetParent(info, parent, g))
            {
                await Touch(parent, g);
            }

            /* remove direntry */
            // destofblok = (UBYTE *)info.direntry;
            // startofblok = destofblok + info.direntry->next;
            // endofblok = (UBYTE *)&(info.dirblock->blk) + g->rootblock->reserved_blksize;
            // startofclear = endofblok - info.direntry->next;
            // clearlen = info.direntry->next;
            // memmove(destofblok, startofblok, endofblok - startofblok);

            var blk = info.file.dirblock.dirblock;
            blk.DirEntries.Remove(info.file.direntry);

            // var movelen = endofblok - startofblok;
            // var temp = new byte[movelen];

            /* makes info invalid!! */
            if (info.file.direntry.Next != 0)
            {
                //memset(startofclear, 0, clearlen);
                // for (var i = 0; i < clearlen; i++)
                // {
                //     blk.entries[startofclear + i] = 0;
                // }
                info.file.direntry = new Pfs3DirEntry();
            }

            await Pfs3Update.MakeBlockDirty(info.file.dirblock, g); // %6.2
        }

// /* <FindObject>
//  *
//  * FindObject searches the object 'fname' in directory 'directory'.
//  * FindObject zoekt het object 'fname' in directory 'directory'.
//  * Interpret empty filename as parent and ":" as root.
//  * Does not use multiple-assign-list
//  *
//  * input : - [directory]: the 'root' directory of the search
//  *         - [objectname]: file to be found, including path
//  *
//  * output: - [object]: If file found : fileinfo of object
//  *                     If path found : fileinfo of directory
//  *         - [error]: Errornumber as result = DOSFALSE; otherwise 0
//  *
//  * result: DOSTRUE  (-1) = file found (->in fileinfo)
//  *          DOSFALSE (0)  = error
//  *
//  * If only a partial path is found, a pointer to the unparsed part
//  * will be stored in g->unparsed.
//  */
         public static async Task<bool> FindObject(Pfs3ObjectInfo directory, string objectname,
             Pfs3ObjectInfo obj, Pfs3GlobalData g)
         {
             var filename = await GetFullPath(directory, objectname, obj, g);

             if (filename == null)
             {
                 //DB(Trace(2, "FindObject !filename %s\n", objectname));
                 return false;
             }

             /* path only (dir or volume) */
             if (string.IsNullOrEmpty(filename))
             {
                 return true;
             }

             /* there is a filepart (file or dir)  */
             // var ok = await GetObject(filename, obj, g);
             // if (!ok && (*error == ERROR_OBJECT_NOT_FOUND))
             //     g->unparsed = filename;

             return await GetObject(filename, obj, g);
         }

/* GetParent
 *
 * childanodenr = anodenr of start directory (the child)
 * parentanodenr = anodenr of directory containing childanodenr (the parent)
 * childfi == parentfi can be dangerous
 * in:childfi; out:parentfi, error
 */
        public static async Task<bool> GetParent(Pfs3ObjectInfo childfi, Pfs3ObjectInfo parentfi, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug("Pfs3Directory: GetParent Enter");
#endif
            Pfs3Canode anode = new Pfs3Canode();
            Pfs3CachedBlock dirblock = null;
            Pfs3DirEntry de = null;
            uint anodeoffset = 0;
            uint childanodenr, parentanodenr;
            bool eod = false, eob = false, found = false;

            // -I- Find anode of parent
            if (childfi == null || Pfs3Macro.IsVolume(childfi)) // child is rootdir
            {
                //*error = 0x0;           /* No error; just return NULL */
                return false;
            }

#if DELDIR
	if (g->deldirenabled)
	{
		if (IsDelDir(*childfi))
			return GetRoot(parentfi, g);

		if (IsDelFile(*childfi))
		{
			parentfi->deldir.special = SPECIAL_DELDIR;
			parentfi->deldir.volume = g->currentvolume;
			return TRUE;
		}
	}
#endif

            var blk = childfi.file.dirblock.dirblock;
            childanodenr = blk.anodenr; /* the directory 'child' is in */
            parentanodenr = blk.parent; /* the directory 'childanodenr' is in */

            // -II- check if in root
            if (parentanodenr == 0) /* child is in rootdir */
            {
                parentfi.volume.root = 0;
                parentfi.volume.volume = childfi.file.dirblock.volume;
                return true;
            }

            // -III- get parentdirectory and find direntry
            await Pfs3Anodes.GetAnode(anode, parentanodenr, g);
            while (!found && !eod)
            {
                dirblock = await LoadDirBlock(anode.blocknr + anodeoffset, g);
                if (dirblock != null)
                {
                    blk = dirblock.dirblock;
                    de = blk.DirEntries.FirstOrDefault(x => x.anode == childanodenr);
                    found = de != null;
                    // var maxDirEntries = CalculateMaxDirEntries(blk);
                    // var dirEntriesNo = 0;
                    // de = Macro.FIRSTENTRY(blk);
                    // eob = false;
                    //
                    // do
                    // {
                    //     found = de.anode == childanodenr;
                    //     if (found)
                    //     {
                    //         break;
                    //     }
                    //     eob = de.next == 0;
                    //
                    //     dirEntriesNo++;
                    //     CheckReadDirEntryError(anode.blocknr + anodeoffset, blk, dirEntriesNo, maxDirEntries, -1);
                    //
                    //     de = Macro.NEXTENTRY(blk, de);
                    // } while (!eob);

                    if (!found)
                    {
                        var result = await Pfs3Anodes.NextBlock(anode, anodeoffset, g);
                        anodeoffset = result.Item2;
                        eod = !result.Item1;
                    }
                }
                else
                {
                    break;
                }
            }

            if (!found)
            {
                //DB(Trace(1, "GetParent", "DiskNotValidated %ld\n", childanodenr));
                //*error = ERROR_DISK_NOT_VALIDATED;
                return false;
            }

            parentfi.file.direntry = de;
            parentfi.file.dirblock = dirblock;
            parentfi.volume.root = de.anode != Pfs3Constants.ANODE_ROOTDIR ? 1U : 0U;
            Pfs3Macro.Lock(dirblock, g);
            return true;
        }

        // Calculates the maximum number of directory entries a directory block can store.
        // private static int CalculateMaxDirEntries(dirblock dirblock)
        // {
        //     return dirblock == null ? 0 : (dirblock.entries.Length / Pfs3SizeOf.Pfs3DirEntrySize.Struct) + 5;
        // }

        /// <summary>
        /// Check if dir entry no has exceeded max number of dir entries
        /// </summary>
        /// <param name="blocknr"></param>
        /// <param name="dirblock"></param>
        /// <param name="dirEntriesNo"></param>
        /// <param name="maxDirEntries"></param>
        /// <param name="offset"></param>
        /// <exception cref="IOException"></exception>
        private static void CheckReadDirEntryError(uint blocknr, Pfs3DirBlock dirblock, int dirEntriesNo, int maxDirEntries, int offset)
        {
            if (dirEntriesNo < maxDirEntries)
            {
                return;
            }
            throw new IOException($"Read dir entry at offset {offset} exceeded max dir entries {maxDirEntries} for dirblock block nr {blocknr}");
        }

/* SearchInDir
 *
 * Search an object in a directory and return the fileinfo
 *
 * input : - dirnodenr: anodenr of directory to search in
 *         - objectname: found object (without path)
 *
 * output: - info: objectinfo of found object
 *
 * result: success
 */
        public static async Task<bool> SearchInDir(uint dirnodenr, string objectname, Pfs3ObjectInfo info, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug("Pfs3Directory: SearchInDir Enter");
#endif
            Pfs3Canode anode = new Pfs3Canode();
            Pfs3CachedBlock dirblock;
            //direntry entry = null;
            var found = false;
            var eod = false;
            uint anodeoffset;
            //byte[] intl_name = new byte[Macro.PATHSIZE];

            //ENTER("SearchInDir");
            //ctodstr(objectname, intl_name);
            //var t = AmigaTextHelper.GetBytes(objectname);
            var intl_name = AmigaTextHelper.ToUpper(objectname, true);

            /* truncate */
            if (intl_name.Length > g.fnsize)
            {
                intl_name = intl_name.Substring(g.fnsize);
            }

            if (g.SearchInDirCache.ContainsKey(dirnodenr) && g.SearchInDirCache[dirnodenr].DirEntriesCache.ContainsKey(intl_name))
            {
                var cacheItem = g.SearchInDirCache[dirnodenr];

                info.file.direntry = cacheItem.DirEntriesCache[intl_name];
                info.file.dirblock = cacheItem.DirBlock;
                info.volume.root = 1;
                info.volume.volume = cacheItem.DirBlock.volume;
                Pfs3Macro.Lock(cacheItem.DirBlock, g);
                return true;
            }

            //intltoupper(intl_name);     /* international uppercase objectname */
            await Pfs3Anodes.GetAnode(anode, dirnodenr, g);
            anodeoffset = 0;
            dirblock = await LoadDirBlock(anode.blocknr, g);
            var blk = dirblock.dirblock;


            // var maxDirEntries = CalculateMaxDirEntries(blk);
            Pfs3DirEntry entry = null;
            while (blk != null && !found && !eod) /* eod stands for end-of-dir */
            {
#if DEBUG
                Pfs3Logger.Instance.Debug($"Pfs3Directory: SearchInDir cached block nr {dirblock.blocknr}, block type '{(dirblock.blk == null ? "null" : dirblock.blk.GetType().Name)}', found = {found}, eod = {eod}");
#endif

                // entry = (struct Pfs3DirEntry *)(&dirblock->blk.entries);
                entry = blk.DirEntries.FirstOrDefault(x => AmigaTextHelper.ToUpper(x.Name, true) == intl_name);
                found = entry != null;

                /* scan block */
//                 var entryIndex = 0;
//                 var dirEntriesNo = 0;
//                 do
//                 {
// #if DEBUG
//                     Pfs3Logger.Instance.Debug($"Pfs3Directory: SearchInDir entryIndex = {entryIndex}, found = {found}, eod = {eod}");
// #endif
//                     entry = DirEntryReader.Read(blk.entries, entryIndex);
//                     if (entry.next == 0)
//                     {
//                         break;
//                     }
//                     found = intl_name == AmigaTextHelper.ToUpper(entry.Name, true);
//                     if (found)
//                     {
//                         break;
//                     }
//
//                     dirEntriesNo++;
//                     CheckReadDirEntryError(dirblock.blocknr, blk, dirEntriesNo, maxDirEntries, entryIndex);
//                     entryIndex += entry.next;
//                 } while (entryIndex < blk.entries.Length);

                /* load next block */
                if (!found)
                {
                    var result = await Pfs3Anodes.NextBlock(anode, anodeoffset, g);
                    anodeoffset = result.Item2;
                    if (result.Item1)
                    {
                        dirblock = await LoadDirBlock(anode.blocknr + anodeoffset, g);
                        blk = dirblock.dirblock;
                        // entry = DirEntryReader.Read(blk.entries, 0);
                    }
                    else
                    {
                        eod = true;
                    }
                }
            }

            /* make fileinfo */
            if (dirblock == null)
            {
                return false;
            }
            else if (found)
            {
                // if (!g.SearchInDirCache.ContainsKey(dirnodenr))
                // {
                //     g.SearchInDirCache.Add(dirnodenr, new SearchInDirCacheItem(dirnodenr, dirblock));
                // }
                //
                // g.SearchInDirCache[dirnodenr].DirEntriesCache.Add(intl_name, entry);

                info.file.direntry = entry;
                info.file.dirblock = dirblock;
                info.volume.root = 1;
                info.volume.volume = dirblock.volume;
                Pfs3Macro.Lock(dirblock, g);
                return true;
            }
            else
                return false;
        }

/* AddDirectoryEntry
 *
 * Add a directoryentry to a directory.
 * Tries to add the directoryentry at the end of an existing directoryblock. If
 * that fails, create a new one.
 *
 * Operates on currentvolume
 *
 * input : - dir: directory to add directoryentry too
 *          - newentry: the new directoryentry
 *
 * output: - newinfo: pointer to direntry and directoryblock the entry
 *          was added to
 *
 * NB: A) there should ALWAYS be at least one dirblock
 *     B) assumes CURRENTVOLUME
 */
    }
}
