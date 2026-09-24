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
        /// <summary>
        /// Get path to entry.
        /// </summary>
        /// <param name="g">Pfs3 global data.</param>
        /// <param name="entry">Entry to get path for.</param>
        /// <param name="directory">Pfs3Directory to get path from. If directory is encountered while iterating parent directories, it will stop iterating resulting in a relative path.</param>
        /// <returns></returns>
        public static async Task<string> GetPath(Pfs3GlobalData g, Pfs3ObjectInfo entry, Pfs3ObjectInfo directory = null)
        {
            var currentDirectory = entry;

            // return empty path, if directory is defined and entry is the same as directory
            if (directory != null &&
                directory.file.dirblock.dirblock != null &&
                currentDirectory.file.dirblock.dirblock != null &&
                currentDirectory.file.dirblock.dirblock.anodenr == directory.file.dirblock.dirblock.anodenr)
            {
                return string.Empty;
            }

            var parentDirectory = new Pfs3ObjectInfo();
            var pathComponents = new LinkedList<string>();

            var isRelative = false;
            do
            {
                if (!await GetParent(currentDirectory, parentDirectory, g))
                {
                    break;
                }

                if (parentDirectory.file.dirblock.dirblock == null)
                {
                    break;
                }

                currentDirectory = parentDirectory;

                pathComponents.AddFirst(currentDirectory.file.direntry.Name);

                if (directory != null &&
                    directory.file.dirblock.dirblock != null &&
                    currentDirectory.file.dirblock.dirblock != null &&
                    currentDirectory.file.dirblock.dirblock.anodenr == directory.file.dirblock.dirblock.anodenr)
                {
                    isRelative = true;
                    break;
                }
            } while (currentDirectory.file.dirblock.dirblock != null &&
                     currentDirectory.file.dirblock.dirblock.anodenr != Pfs3Constants.ANODE_ROOTDIR);

            return string.Concat(isRelative ? string.Empty : "/", string.Join("/", pathComponents.ToList()));
        }

        public static async Task<Pfs3CachedBlock> MakeDirBlock(uint blocknr, uint anodenr, uint rootanodenr, uint parentnr,
            Pfs3GlobalData g)
        {
            // struct Pfs3Canode anode;
            // struct cdirblock *blk;
            var volume = g.currentvolume;

            //DB(Trace(10,"MakeDirBlock","blocknr = %lx\n", blocknr));

            /* fill in anode (allocated by MakeDirEntry) */
            var anode = new Pfs3Canode
            {
                clustersize = 1,
                blocknr = blocknr,
                next = 0
            };
            await Pfs3Anodes.SaveAnode(anode, anodenr, g);

            var blk = await Pfs3Lru.AllocLRU(g);
            var dirblock = new Pfs3DirBlock(g);
            dirblock.anodenr = rootanodenr;
            dirblock.parent = parentnr;
            blk.blk = dirblock;
            blk.volume = volume;
            blk.blocknr = blocknr;
            blk.oldblocknr = 0;
            blk.changeflag = true;

            //Macro.Hash(blk, volume.dirblks, Constants.HASHM_DIR);
            Pfs3Macro.Hash(blk, volume.dirblks);
            Pfs3Cache.LOCK(blk, g);
            return blk;
        }

        /* Set number of deldir blocks (Has to be single threaded)
 * If 0 then deldir is disabled (but MODE_DELDIR stays;
 * InitModules() detect that the number of deldirblocks is 0)
 * There must be a currentvolume
 * Returns error (0 = success)
 */
        public static async Task SetDeldir(int nbr, Pfs3GlobalData g)
        {
            var rext = g.currentvolume.rblkextension;
            //struct cdeldirblock *ddblk, *next;
            Pfs3CachedBlock ddblk;
            Pfs3LockEntry list;
            int i;
            //ULONG error = 0;

            /* check range */
            if (nbr < 0 || nbr > Pfs3Constants.MAXDELDIR + 1)
            {
                // return ERROR_BAD_NUMBER;
                throw new Exception("ERROR_BAD_NUMBER");
            }

            /* check if there are locks on any deldir, delfile */
            for (var node = Pfs3Macro.HeadOf(g.currentvolume.fileentries); node != null; node = node.Next)
            {
                list = node.Value as Pfs3LockEntry;

                if (list == null)
                {
                    continue;
                }

                if (Pfs3Macro.IsDelDir(list.le.info) || Pfs3Macro.IsDelFile(list.le.info))
                {
                    // return ERROR_OBJECT_IN_USE;
                    throw new Exception("ERROR_OBJECT_IN_USE");
                }
            }

            await Pfs3Update.UpdateDisk(g);

            /* flush cache */
            // for (var node = Macro.HeadOf(g.currentvolume.deldirblks); node != null; node = node.Next)
            // {
            //     ddblk = node.Value;
            //     Lru.FlushBlock(ddblk, g);
            //     // MinRemove(LRU_CHAIN(ddblk));
            //     // MinAddHead(&g->glob_lrudata.LRUpool, LRU_CHAIN(ddblk));
            //     Macro.MinRemoveLru(ddblk, g);
            //     Macro.MinAddHead(g.glob_lrudata.LRUpool, new LruCachedBlock(ddblk));
            //     // i.p.v. FreeLRU((struct Pfs3DoctorCachedBlock *)ddblk, g);
            // }
            foreach (var node in g.currentvolume.deldirblks)
            {
                ddblk = node.Value;
                Pfs3Lru.FlushBlock(ddblk, g);
                // MinRemove(LRU_CHAIN(ddblk));
                // MinAddHead(&g->glob_lrudata.LRUpool, LRU_CHAIN(ddblk));
                Pfs3Macro.MinRemoveLru(ddblk, g);
                Pfs3Macro.MinAddHead(g.glob_lrudata.LRUpool, new Pfs3LruCachedBlock(ddblk));
                // i.p.v. FreeLRU((struct Pfs3DoctorCachedBlock *)ddblk, g);
            }

            /* free unwanted deldir blocks */
            var rext_blk = rext.rblkextension;
            for (i = nbr; i < rext_blk.deldirsize; i++)
            {
                    Pfs3Allocation.FreeReservedBlock(rext_blk.deldir[i], g);
                rext_blk.deldir[i] = 0;
            }

            /* allocate wanted ones */
            for (i = rext_blk.deldirsize; i < nbr; i++)
            {
                if (await NewDeldirBlock((ushort)i, g) == null)
                {
                    nbr = i + 1;
                    // error = ERROR_DISK_FULL;
                    // break;
                    throw new Exception("ERROR_DISK_FULL");
                }
            }

            /* if deldir size increases, start roving in a the new area
             * if deldir size decreases, start roving from the start
             */
            if (nbr > rext_blk.deldirsize)
                rext_blk.deldirroving = (ushort)(rext_blk.deldirsize * Pfs3Constants.DELENTRIES_PER_BLOCK);
            else
                rext_blk.deldirroving = 0;

            /* enable/disable */
            rext_blk.deldirsize = (ushort)nbr;
            g.deldirenabled = nbr > 0;

            await Pfs3Update.MakeBlockDirty(rext, g);
            await Pfs3Update.UpdateDisk(g);
        }

        public static async Task<Pfs3CachedBlock> NewDeldirBlock(ushort seqnr, Pfs3GlobalData g)
        {
            // cdeldirblock
            var volume = g.currentvolume;
            // struct crootblockextension *rext;
            Pfs3CachedBlock ddblk;
            uint blocknr;

            var rext = volume.rblkextension;

            if (seqnr > Pfs3Constants.MAXDELDIR)
            {
                // DB(Trace(5, "NewDelDirBlock", "seqnr out of range = %lx\n", seqnr));
                return null;
            }

            /* alloc block and LRU slot */
            if ((ddblk = await Pfs3Lru.AllocLRU(g)) == null || (blocknr = Pfs3Allocation.AllocReservedBlock(g)) == 0)
            {
                if (ddblk != null)
                    Pfs3Lru.FreeLRU(ddblk, g);
                return null;
            }

            /* make reference */
            var rext_blk = rext.rblkextension;
            rext_blk.deldir[seqnr] = blocknr;

            /* fill block */
            ddblk.volume = volume;
            ddblk.blocknr = blocknr;
            ddblk.used = 0;
            var ddblk_blk = new Pfs3DelDirBlock(g)
            {
                id = Pfs3Constants.DELDIRID,
                seqnr = seqnr
            };
            ddblk.blk = ddblk_blk;
            ddblk.changeflag = true;
            ddblk_blk.protection = Pfs3Constants.DELENTRY_PROT; /* re..re..re.. */
            ddblk_blk.CreationDate = g.RootBlock.CreationDate;
            // ddblk->blk.creationminute	= volume->rootblk->creationminute;
            // ddblk->blk.creationtick		= volume->rootblk->creationtick;

            /* add to cache and return */
            // Macro.MinAddHead(volume.deldirblks, ddblk);
            Pfs3Macro.AddToIndexes(volume.deldirblks, volume.deldirblksBySeqNr, ddblk);
            return ddblk;
        }

/*
 * Frees anodes without freeing blocks
 */
        public static async Task FreeAnodesInChain(uint anodenr, Pfs3GlobalData g)
        {
            Pfs3Canode anode = new Pfs3Canode();
            var rext = g.currentvolume.rblkextension;

            // DB(Trace(1, "FreeAnodeInChain", "anodenr: %ld \n", anodenr));
            await Pfs3Anodes.GetAnode(anode, anodenr, g);
            while (anode.nr != 0) /* stops autom.: anode.nr of anode 0 == 0 */
            {
                if (Pfs3Macro.IsUpdateNeeded(Pfs3Constants.RTBF_THRESHOLD, g))
                {
                    if (rext != null)
                    {
                        var rext_blk = rext.rblkextension;
                        rext_blk.tobedone.operation_id = Pfs3Constants.PP_FREEANODECHAIN;
                        rext_blk.tobedone.argument1 = anode.nr;
                        rext_blk.tobedone.argument2 = 0;
                        rext_blk.tobedone.argument3 = 0;
                    }

                    await Pfs3Update.UpdateDisk(g);
                }

                await Pfs3Anodes.FreeAnode(anode.nr, g);
                await Pfs3Anodes.GetAnode(anode, anode.next, g);
            }

            if (rext != null)
            {
                var rext_blk = rext.rblkextension;
                rext_blk.tobedone.operation_id = 0;
                rext_blk.tobedone.argument1 = 0;
                rext_blk.tobedone.argument2 = 0;
                rext_blk.tobedone.argument3 = 0;
            }
        }

/* <NewFile>
 *
 * NewFile creates a new file in a [directory] on currentvolume
 *
 * input : - [directory]: directory of file;
 *         - [filename]: name (without path) of file
 *         - found: flag, file already present?. If so newfile == old
 *
 * output: - [newfile]: fileinfo of new file (struct is managed by caller)
 *		   - [directory]: fileinfo of parent (can have changed if hardlink)
 *
 * result: errornr; 0 = success
 *
 * maxneeds: 1 nd + 2 na = 3 res
 *
 * Note: 'directory' and 'newfile' may point to the same.
 */
    }
}
