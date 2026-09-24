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
        public static async Task ChangeDirEntry(Pfs3ObjectInfo from, Pfs3DirEntry to, Pfs3ObjectInfo destdir, Pfs3FileInfo result,
            Pfs3GlobalData g)
        {
            uint destanodenr = Pfs3Macro.IsRoot(destdir) ? Pfs3Constants.ANODE_ROOTDIR : Pfs3Macro.FIANODENR(destdir.file);

            //Cache.ClearSearchInDirCache(object_.file.dirblock.blocknr, g);

            /* check whether a 'within dir' rename */
            if (to != null && destanodenr == from.file.dirblock.dirblock.anodenr)
                await RenameWithinDir(from, to, result, g);
            else
                await RenameAcrossDirs(from, to, destdir, result, g);
        }

        // public static void AddExtraFields(byte[] entries, direntry direntry, extrafields extra)
        // {
        //     direntry.ExtraFields = extra;
        //     DirEntryWriter.WriteExtraFields(entries, direntry.Offset, direntry);
        // }

/*
 * Rename file within dir
 * NULL destination not allowed
 */
        public static async Task RenameWithinDir(Pfs3ObjectInfo from, Pfs3DirEntry to, Pfs3FileInfo result, Pfs3GlobalData g)
        {
            int spaceneeded;
            Pfs3ObjectInfo mover = new Pfs3ObjectInfo
            {
                file = new Pfs3FileInfo()
            };

            Pfs3Macro.Lock(from.file.dirblock, g);
            var fromDirBlock = from.file.dirblock.dirblock;
            // mover.file.direntry = Macro.FIRSTENTRY(fromDirBlock);
            mover.file.direntry = fromDirBlock.DirEntries.FirstOrDefault();
            if (mover.file.direntry == null)
            {
                throw new IOException("RenameWithinDir: mover file direntry is null");
            }
            mover.file.dirblock = from.file.dirblock;
            mover.volume.root = mover.file.direntry.anode != Pfs3Constants.ANODE_ROOTDIR ? 1U : 0U;
            spaceneeded = to.Next - from.file.direntry.Next;
            if (spaceneeded <= 0)
            {
                await RenameInPlace(from, to, result, g);
            }
            else
            {
                /* make space in block
                 */
                while (!CheckFit(from.file.dirblock, spaceneeded, g) &&
                       !from.file.direntry.Equals(mover.file.direntry))
                {
                    // move first dir entry (mover) and update to new first dir entry
                    await MoveToPrevious(mover, mover.file.direntry, result, g);
                    mover.file.direntry = fromDirBlock.DirEntries.FirstOrDefault();
                }

                if (CheckFit(from.file.dirblock, spaceneeded, g))
                    await RenameInPlace(from, to, result, g);
                else
                    await MoveToPrevious(from, to, result, g);
            }

            Pfs3Macro.Lock(result.dirblock, g);
        }

        /// <summary>
        /// Check if direntry will fit in, returns position to place it if ok
        /// </summary>
        /// <param name="blok"></param>
        /// <param name="needed"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static bool CheckFit(Pfs3CachedBlock blok, int needed, Pfs3GlobalData g)
        {
            // struct cdirblock *blok
            Pfs3DirEntry entry;
            int i;

            /* goto end of dirblock */
            var blk = blok.dirblock;
            i = blk.DirEntries.Sum(x => x.Next);

            return needed + i + 1 <= Pfs3Macro.DB_ENTRYSPACE(g);
        }

/*
 * Moves firstentry to previous block, changing it to to. To can point to de.direntry
 * if wanted.
 * Return new fileinfo in 'result'
 * NB: no need to touch parent: MTP is always followed by another function
 * on the block.
 */
        public static async Task<bool> MoveToPrevious(Pfs3ObjectInfo de, Pfs3DirEntry to, Pfs3FileInfo result, Pfs3GlobalData g)
        {
            Pfs3DirEntry dest;
            Pfs3CachedBlock prevblock = new Pfs3CachedBlock(); // cdirblock
            Pfs3Canode anode = new Pfs3Canode();
            int removedlen;
            uint prev;

            Pfs3Macro.Lock(de.file.dirblock, g);

            /* get previous block */
            var dirblockBlk = de.file.dirblock.dirblock;
            await Pfs3Anodes.GetAnode(anode, dirblockBlk.anodenr, g);
            prev = 0;
            while (anode.blocknr != de.file.dirblock.blocknr && anode.next != 0)
            {
                prev = anode.nr;
                await Pfs3Anodes.GetAnode(anode, anode.next, g);
            }

            /* savety check */
            if (anode.blocknr != de.file.dirblock.blocknr)
            {
                throw new IOException("AFS_ERROR_CACHE_INCONSISTENCY");
            }

            /* Get dirblock in question
             * Special case : previous == 0!!->add new head!!
             */
            if (prev != 0)
            {
                await Pfs3Anodes.GetAnode(anode, prev, g);
                if ((prevblock = await LoadDirBlock(anode.blocknr, g)) == null)
                    return false;
            }

            /* Add new entry */
            if (prev != 0 && CheckFit(prevblock, to.Next, g))
            {
                // memcpy(dest, to, to->next);
                // *(UBYTE*)NEXTENTRY(dest) = 0; /* end of dirblock */
                // overwrite dest with to
                dest = to;
                var dirBlock = prevblock.dirblock;
                dirBlock.DirEntries.Add(to);

                result.direntry = dest;
                result.dirblock = prevblock;
            }
            else
            {
                /* make new dirblock .. */
                uint parent;
                Pfs3Canode newanode = new Pfs3Canode();
                Pfs3CachedBlock newblock; // struct cdirblock *newblock;

                newanode.clustersize = 1;
                var dirBlockBlk = de.file.dirblock.dirblock;
                parent = dirblockBlk.parent;
            if ((newanode.blocknr = Pfs3Allocation.AllocReservedBlock(g)) == 0)
                {
                    return false;
                }

                if (prev == 0)
                {
                    await Pfs3Anodes.GetAnode(anode, dirBlockBlk.anodenr, g);
                    newanode.nr = anode.nr;
                    newanode.next = anode.nr = await Pfs3Anodes.AllocAnode(anode.next != 0 ? anode.next : anode.nr, g);
                }
                else
                {
                    newanode.nr = await Pfs3Anodes.AllocAnode(anode.nr, g);
                    newanode.next = anode.next;
                    anode.next = newanode.nr;
                }

                await Pfs3Anodes.SaveAnode(anode, anode.nr, g);
                newblock = await MakeDirBlock(newanode.blocknr, newanode.nr, dirBlockBlk.anodenr, parent, g);
                await Pfs3Anodes.SaveAnode(newanode, newanode.nr, g); /* MUST be done AFTER MakeDirBlock */

                /* add entry */
                var newBlockBlk = newblock.dirblock;
                // dest = Macro.FIRSTENTRY(newBlockBlk);
                // memcpy(dest, to, to->next);
                dest = to;
                newBlockBlk.DirEntries.Add(dest);

                result.direntry = dest;
                result.dirblock = newblock;
            }

            Pfs3Macro.Lock(result.dirblock, g);
            await Pfs3Update.MakeBlockDirty(result.dirblock, g);

            /* remove old entry & make blocks dirty */
            //removedlen = de.file.direntry.next;
            await RemoveDirEntry(de, g);

            /* update references */
            await UpdateChangedRef(de.file, result, g);
            return true;
        }

/*
 * There HAS to be sufficient space!!
 */
        public static async Task RenameInPlace(Pfs3ObjectInfo from, Pfs3DirEntry to, Pfs3FileInfo result, Pfs3GlobalData g)
        {
            // int dest, start, end;
            // int movelen;
            // int diff;
            Pfs3ObjectInfo parent = new Pfs3ObjectInfo();

            Pfs3Macro.Lock(from.file.dirblock, g);

            /* change date parent */
            if (await GetParent(from, parent, g))
            {
                await Touch(parent, g);
            }

            // /* make place for new entry */
            //diff = to.next - from.file.direntry.next;
            // dest = from.file.direntry.Offset + to.next;
            // start = from.file.direntry.Offset + from.file.direntry.next;
            // //end = (UBYTE *)&(from.dirblock->blk) + g.RootBlock.ReservedBlksize;
            // end = Pfs3SizeOf.Pfs3DirBlockSize.Entries(g);
            // movelen = diff > 0 ? end - dest : end - start;
            //
            // // memmove(dest, start, movelen);
            var dirBlock = from.file.dirblock.dirblock;
            // replace from with to by removing existing from dir entries and add new to dir entries
            var existingDirEntry = dirBlock.DirEntries.FirstOrDefault(x => x.Name == from.file.direntry.Name);
            if (existingDirEntry == null)
            {
                throw new IOException(
                    $"Dir entry '{from.file.direntry.Name}' not found in dir block '{from.file.dirblock.blocknr}'");
            }
            dirBlock.DirEntries.Remove(existingDirEntry);
            dirBlock.DirEntries.Add(to);

            /* fill in new entry */
            // memcpy((UBYTE *)from.direntry, to, to.next);
            from.file.direntry = to;

            /* fill in result and make block dirty */
            result.direntry = from.file.direntry;
            result.dirblock = from.file.dirblock;
            await Pfs3Update.MakeBlockDirty(from.file.dirblock, g);

            /* update references */
            await UpdateChangedRef(from.file, result, g);
        }

/*
 * Move a file from one dir to another
 * NULL = delete allowed
 */
        public static async Task RenameAcrossDirs(Pfs3ObjectInfo from, Pfs3DirEntry to, Pfs3ObjectInfo destdir, Pfs3FileInfo result,
            Pfs3GlobalData g)
        {
            ushort removedlen;

            /* remove old entry (invalidates 'destdir') */
            //removedlen = from.file.direntry.next;
            await RemoveDirEntry(from, g);
            if (to != null)
            {
                // removing the direntry from the dirblock is handled by "RemoveDirEntry"
                // which only has a simple list of entries.
                // the original pfs3 code modifies the dirblock bytes using pointers.

                /* test on volume is not necessary, because file.dirblock = volume.volume !=
                 * from.dirblock
                 * restore 'destdir' (can be invalidated by RemoveDirEntry)
                 */
                // if (destdir->file.dirblock == from.dirblock &&
                //     destdir->file.direntry > from.direntry)
                // {
                //     destdir->file.direntry = (struct Pfs3DirEntry *)
                //     ((UBYTE *)destdir->file.direntry - removedlen);
                // }

                /* add new entry */
                await AddDirectoryEntry(destdir, to, result, g);
            }

            await UpdateChangedRef(from.file, result, g);
            if (result != null)
            {
                Pfs3Macro.Lock(result.dirblock, g);
            }
        }

    }
}
