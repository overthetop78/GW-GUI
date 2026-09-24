namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Blocks;

    public static class Pfs3Macro
    {
        public static bool IsSameOI(Pfs3ObjectInfo oi1, Pfs3ObjectInfo oi2)
        {
            return oi1.file?.direntry != null && oi2.file?.direntry != null &&
                   oi1.file.direntry.Equals(oi2.file.direntry) &&
                   oi1.file.dirblock.blocknr == oi2.file.dirblock.blocknr;
        }

        public static void MarkDataDirty(int i, Pfs3GlobalData g) => g.dc.ref_[i].dirty = true;

        // comment: de is struct Pfs3DirEntry *
        //public static int COMMENT(direntry de) => de.startofname + de.Name.Length;

        public static bool IsRoot(Pfs3ObjectInfo oi) => oi == null || oi.volume.root == 0;
        public static bool IsRootA(Pfs3ObjectInfo oi) => oi.volume.root == 0;

        public static uint BLOCKSIZE(Pfs3GlobalData g) => g.blocksize;
        public static uint BLOCKSIZEMASK(Pfs3GlobalData g) => g.blocksize - 1;
        public static ushort BLOCKSHIFT(Pfs3GlobalData g) => g.blockshift;
        public static int DIRECTSIZE(Pfs3GlobalData g) => g.directsize;

        public static bool IsVolumeEntry(IPfs3Entry e) => e.ListEntry.type.flags.type == Pfs3Constants.ETF_VOLUME;
        public static bool IsFileEntry(IPfs3Entry e) => e.ListEntry.type.flags.type == Pfs3Constants.ETF_FILEENTRY;
        public static bool IsLockEntry(IPfs3Entry e) => e.ListEntry.type.flags.type == Pfs3Constants.ETF_LOCK;
        public static bool SHAREDLOCK(IPfs3Entry e) => e.ListEntry.type.flags.access <= 1;

        // public static uint ANODENR(fileentry fe) => fe.anodenr;
        public static uint FIANODENR(Pfs3FileInfo fi) => fi.direntry.anode;

        // #define IsRollover(oi) ((IPTR)(oi).file.direntry>2 && ((oi).file.direntry->type==ST_ROLLOVERFILE))
        public static bool IsRollover(Pfs3ObjectInfo oi) =>
            oi.file.direntry != null && oi.file.direntry.type == Convert.ToSByte(Pfs3Constants.ST_ROLLOVERFILE);

        public static int MKBADDR(uint x) => (int)x >> 2;

        public static void Lock(Pfs3CachedBlock blk, Pfs3GlobalData g) => blk.used = g.locknr;

        public static void UnlockAll(Pfs3GlobalData g)
        {
            g.locknr++;
            if (g.locknr == ushort.MaxValue)
            {
                g.locknr = 0;
            }
        }
        public static bool IsLocked(Pfs3CachedBlock blk, Pfs3GlobalData g) => blk.used == g.locknr;

        // Note: first get dirblock from cached block: CachedBlock.direntry.
        // public static direntry FIRSTENTRY(dirblock blk) => DirEntryReader.Read(blk.entries, 0);

        /* get next directory entry */
        //public static int NEXTENTRY(direntry de) => ((struct Pfs3DirEntry*)((UBYTE*)(de) + (de)->next))
        // public static direntry NEXTENTRY(dirblock blk, direntry de)
        // {
        //     return DirEntryReader.Read(blk.entries, de.Offset + de.next);
        // }
        // public static int DB_HEADSPACE(globaldata g) => Pfs3SizeOf.Pfs3DirBlockSize.Struct(g);
        /// <summary>Gets the space available for directory entries.</summary>
        /// <param name="g">Global file-system data.</param>
        /// <returns>Available space in bytes.</returns>
        public static int DB_ENTRYSPACE(Pfs3GlobalData g) => Pfs3SizeOf.Pfs3DirBlockSize.Entries(g);

        //public static int GetAnodeBlock(uint a, uint b, globaldata g) => Pfs3Anodes.big_GetAnodeBlock() (g.getanodeblock)(a, b);

/*
;-----------------------------------------------------------------------------
;	ULONG divide (ULONG d0, UWORD d1)
;-----------------------------------------------------------------------------
*/
        public static uint divide(uint d0, ushort d1)
        {
            uint q = d0 / d1;
            /* NOTE: I doubt anything depends on this, but lets simulate 68k divu overflow anyway - Piru */
            if (q > 65535UL) return d0;
            return ((d0 % d1) << 16) | q;
        }


/* max length of filename, diskname and comment
 * FNSIZE is 108 for compatibilty. Used for searching
 * files.
 */
        public static int FNSIZE = 108;
        public static int PATHSIZE = 256;
        public static int FILENAMESIZE(Pfs3GlobalData g) => g.fnsize;
        public static int DNSIZE = 32;
        public static int CMSIZE = 80;
        public static int MAX_ENTRYSIZE = Pfs3SizeOf.Pfs3DirEntrySize.Struct + FNSIZE + CMSIZE + 34;

        /*
         * IPTR
There is another important typedef, IPTR. It is really important in AROS, as it the only way to declare a field that can contain both an integer and a pointer.

Note

AmigaOS does not know this typedef. If you are porting a program from AmigaOS to AROS, you have to search your source for occurrences of ULONG that can also contain pointers, and change them into IPTR. If you don't do this, your program will not work on systems which have pointers with more than 32 bits (for example DEC Alphas that have 64-bit pointers).

BPTR
         */

        // (IPTR)(oi).file.direntry > 2 &&
        public static bool IsDirEntry(Pfs3ObjectInfo oi) => oi?.file.direntry != null;
        public static bool IsSoftLink(Pfs3ObjectInfo oi) => IsDirEntry(oi) && oi.file.direntry.type == Pfs3Constants.ST_SOFTLINK;
        public static bool IsRealDir(Pfs3ObjectInfo oi) => IsDirEntry(oi) && oi.file.direntry.type == Pfs3Constants.ST_USERDIR;
        public static bool IsDir(Pfs3ObjectInfo oi) => IsDirEntry(oi) && oi.file.direntry.type > 0;
        public static bool IsFile(Pfs3ObjectInfo oi) => IsDirEntry(oi) && oi.file.direntry.type <= 0;
        public static bool IsVolume(Pfs3ObjectInfo oi) => oi.volume.root == 0;

/* macros on cachedblocks */
        public static bool IsDirBlock(IPfs3Block blk) => blk.id == Pfs3Constants.DBLKID;
        public static bool IsAnodeBlock(IPfs3Block blk) => blk.id == Pfs3Constants.ABLKID;
        public static bool IsIndexBlock(IPfs3Block blk) => blk.id == Pfs3Constants.IBLKID;
        public static bool IsBitmapBlock(IPfs3Block blk) => blk.id == Pfs3Constants.BMBLKID;
        public static bool IsBitmapIndexBlock(IPfs3Block blk) => blk.id == Pfs3Constants.BMIBLKID;
        public static bool IsDeldir(IPfs3Block blk) => blk.id == Pfs3Constants.DELDIRID;
        public static bool IsSuperBlock(IPfs3Block blk) => blk.id == Pfs3Constants.SBLKID;

        public static bool IsDelDir(Pfs3ObjectInfo oi) => oi.deldir.special == Pfs3Constants.SPECIAL_DELDIR;
        public static bool IsDelFile(Pfs3ObjectInfo oi) => oi.deldir.special == Pfs3Constants.SPECIAL_DELFILE;

        /* predefined anodes */
        public static int ANODE_EOF = 0;
        public static int ANODE_RESERVED_1 = 1;	// not used by MODE_BIG
        public static int ANODE_RESERVED_2 = 2;	// not used by MODE_BIG
        public static int ANODE_RESERVED_3 = 3;	// not used by MODE_BIG
        public static int ANODE_BADBLOCKS = 4;	// not used yet
        public static int ANODE_ROOTDIR = 5;
        public static int ANODE_USERFIRST = 6;

        // #define alloc_data (g->glob_allocdata)
        // #define andata (g->glob_anodedata)
        // #define lru_data (g->glob_lrudata)
        // #define FILENAMESIZE (g->fnsize)

        public static bool ReservedAreaIsLocked(Pfs3GlobalData g)
        {
            return g.glob_allocdata.res_alert;
        }

        /* macros on cachedblocks */
        public static bool IsDirBlock(Pfs3CachedBlock blk) => blk.blk.id == Pfs3Constants.DBLKID;
        public static bool IsAnodeBlock(Pfs3CachedBlock blk) => blk.blk.id == Pfs3Constants.ABLKID;
        public static bool IsIndexBlock(Pfs3CachedBlock blk) => blk.blk.id == Pfs3Constants.IBLKID;
        public static bool IsBitmapBlock(Pfs3CachedBlock blk) => blk.blk.id == Pfs3Constants.BMBLKID;
        public static bool IsBitmapIndexBlock(Pfs3CachedBlock blk) => blk.blk.id == Pfs3Constants.BMIBLKID;
        public static bool IsDeldir(Pfs3CachedBlock blk) => blk.blk.id == Pfs3Constants.DELDIRID;
        public static bool IsSuperBlock(Pfs3CachedBlock blk) => blk.blk.id == Pfs3Constants.SBLKID;

        public static void MinRemove(IPfs3Entry node, Pfs3GlobalData g)
        {
            var volume = g.currentvolume;
            volume.fileentries.Remove(node);
        }

        public static void MinRemove(Pfs3AnodeChain anodechain, Pfs3GlobalData g)
        {
            g.currentvolume.anodechainlist.Remove(anodechain);
        }

        public static void MinRemove(Pfs3CachedBlock node, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"Macro: MinRemove CachedBlock block nr {node.blocknr} ({node.GetHashCode()})");
#endif
            // #define MinRemove(node) Remove((struct Node *)node)
            // remove() removes the node from any list it' added to (Amiga MinList exec)

            var volume = g.currentvolume;
            // for (var i = 0; i < volume.anblks.Length; i++)
            // {
            //     volume.anblks[i].Remove(node);
            // }
            if (IsAnodeBlock(node) && volume.anblks.ContainsKey(node.blocknr))
            {
                volume.anblks.Remove(node.blocknr);
            }

            // for (var i = 0; i < volume.dirblks.Length; i++)
            // {
            //     volume.dirblks[i].Remove(node);
            // }
            if (IsDirBlock(node) && volume.dirblks.ContainsKey(node.blocknr))
            {
                volume.dirblks.Remove(node.blocknr);
            }

            // volume.indexblks.Remove(node);
            if (IsIndexBlock(node))
            {
                if (volume.indexblks.ContainsKey(node.blocknr))
                {
                    volume.indexblks.Remove(node.blocknr);
                }
                if (node.blk is Pfs3IndexBlock indexBlock &&
                    volume.indexblksBySeqNr.ContainsKey(indexBlock.seqnr) &&
                    volume.indexblksBySeqNr[indexBlock.seqnr] == node)
                {
                    volume.indexblksBySeqNr.Remove(indexBlock.seqnr);
                }
            }


            // volume.bmblks.Remove(node);
            if (IsBitmapBlock(node))
            {
                if (volume.bmblks.ContainsKey(node.blocknr))
                {
                    volume.bmblks.Remove(node.blocknr);
                }
                if (node.blk is Pfs3BitmapBlock bitmapBlock &&
                    volume.bmblksBySeqNr.ContainsKey(bitmapBlock.seqnr) &&
                    volume.bmblksBySeqNr[bitmapBlock.seqnr] == node)
                {
                    volume.bmblksBySeqNr.Remove(bitmapBlock.seqnr);
                }
            }

            // volume.superblks.Remove(node);
            if (IsSuperBlock(node))
            {
                if (volume.superblks.ContainsKey(node.blocknr))
                {
                    volume.superblks.Remove(node.blocknr);
                }
                if (node.blk is Pfs3IndexBlock superBlock &&
                    volume.superblksBySeqNr.ContainsKey(superBlock.seqnr) &&
                    volume.superblksBySeqNr[superBlock.seqnr] == node)
                {
                    volume.superblksBySeqNr.Remove(superBlock.seqnr);
                }
            }

            // volume.deldirblks.Remove(node);
            if (IsDeldir(node))
            {
                if (volume.deldirblks.ContainsKey(node.blocknr))
                {
                    volume.deldirblks.Remove(node.blocknr);
                }
                if (node.blk is Pfs3DelDirBlock delDirBlock &&
                    volume.deldirblksBySeqNr.ContainsKey(delDirBlock.seqnr) &&
                    volume.deldirblksBySeqNr[delDirBlock.seqnr] == node)
                {
                    volume.deldirblksBySeqNr.Remove(delDirBlock.seqnr);
                }
            }

            // volume.bmindexblks.Remove(node);
            if (IsBitmapIndexBlock(node))
            {
                if (volume.bmindexblks.ContainsKey(node.blocknr))
                {
                    volume.bmindexblks.Remove(node.blocknr);
                }
                if (node.blk is Pfs3IndexBlock bitmapIndexBlock &&
                    volume.bmindexblksBySeqNr.ContainsKey(bitmapIndexBlock.seqnr) &&
                    volume.bmindexblksBySeqNr[bitmapIndexBlock.seqnr] == node)
                {
                    volume.bmindexblksBySeqNr.Remove(bitmapIndexBlock.seqnr);
                }
            }
        }

        /// <summary>
        /// Remove cached block from lru lists containing it
        /// </summary>
        /// <param name="node"></param>
        /// <param name="g"></param>
        public static void MinRemoveLru(Pfs3CachedBlock node, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"Macro: MinRemoveLru CachedBlock block nr {node.blocknr} ({node.GetHashCode()})");
#endif
            foreach (var lruCachedBlock in g.glob_lrudata.LRUpool.Where(x => x.cblk == node).ToList())
            {
                g.glob_lrudata.LRUpool.Remove(lruCachedBlock);
            }

            foreach (var lruCachedBlock in g.glob_lrudata.LRUqueue.Where(x => x.cblk == node).ToList())
            {
                g.glob_lrudata.LRUqueue.Remove(lruCachedBlock);
            }

            for (var i = 0; i < g.glob_lrudata.LRUarray.Length; i++)
            {
                if (g.glob_lrudata.LRUarray[i] == null || g.glob_lrudata.LRUarray[i].cblk == node)
                {
                    continue;
                }

                g.glob_lrudata.LRUarray[i] = null;
            }
        }

        /// <summary>
        /// add node to head of list. Amiga MinList exec
        /// </summary>
        /// <param name="list"></param>
        /// <param name="node"></param>
        /// <typeparam name="T"></typeparam>
        public static void MinAddHead<T>(LinkedList<T> list, T node)
        {
            // #define MinAddHead(list, node)  AddHead((struct List *)(list), (struct Node *)(node))
            list.AddFirst(node);
        }

        public static void AddToIndexes(IDictionary<uint, Pfs3CachedBlock> byBlockNr,
            IDictionary<uint, Pfs3CachedBlock> bySeqNr, Pfs3CachedBlock node)
        {
            var seqBlock = node.blk as IPfs3SeqBlock;
            if (seqBlock is null)
            {
                throw new ArgumentException("Node is not a seq block", nameof(node));
            }

            // #define MinAddHead(list, node)  AddHead((struct List *)(list), (struct Node *)(node))
            byBlockNr[node.blocknr] = node;
            bySeqNr[seqBlock.seqnr] = node;
        }

        public static LinkedListNode<T> HeadOf<T>(LinkedList<T> list)
        {
            // #define HeadOf(list) ((void *)((list)->mlh_Head))
            return list.First;
        }

        public static bool IsMinListEmpty<T>(LinkedList<T> list) => list.Count == 0;

        public static async Task<Pfs3CachedBlock> GetAnodeBlock(ushort seqnr, Pfs3GlobalData g)
        {
            // #define GetAnodeBlock(a, b) (g->getanodeblock)(a,b)
            // g->getanodeblock = big_GetAnodeBlock;
            return await Pfs3Init.big_GetAnodeBlock(seqnr, g);
        }

        /// <summary>
        /// convert anodenr to subclass with seqnr and offset
        /// </summary>
        /// <param name="anodenr"></param>
        /// <returns></returns>
        public static Pfs3AnodeNr SplitAnodenr(uint anodenr)
        {
            // typedef struct
            // {
            //     UWORD seqnr;
            //     UWORD offset;
            // } anodenr_t;
            return new Pfs3AnodeNr
            {
                seqnr = (ushort)(anodenr >> 16),
                offset = (ushort)(anodenr & 0xFFFF)
            };
        }

        public static bool InPartition(uint blk, Pfs3GlobalData g)
        {
            return blk >= g.firstblock && blk <= g.lastblock;
        }

        public static void Hash(Pfs3CachedBlock blk, LinkedList<Pfs3CachedBlock>[] list, int mask)
        {
            // #define Hash(blk, list, mask)                           \
            //             MinAddHead(&list[(blk->blocknr/2)&mask], blk)
            MinAddHead(list[(blk.blocknr / 2) & mask], blk);
        }

        public static void Hash(Pfs3CachedBlock blk, IDictionary<uint, Pfs3CachedBlock> list)
        {
            list.Add(blk.blocknr, blk);
        }

        /*
         * Hashing macros
         */
        public static void ReHash(Pfs3CachedBlock blk, LinkedList<Pfs3CachedBlock>[] list, int mask, Pfs3GlobalData g)
        {
            // #define ReHash(blk, list, mask)                         \
            // {                                                       \
            //     MinRemove(blk);                                     \
            //     MinAddHead(&list[(blk->blocknr/2)&mask], blk);      \
            // }

            MinRemove(blk, g);
            MinAddHead(list[(blk.blocknr / 2) & mask], blk);
        }

        public static bool IsEmptyDBlk(Pfs3CachedBlock blk, Pfs3GlobalData g)
        {
            // #define FIRSTENTRY(blok) ((struct Pfs3DirEntry*)((blok)->blk.entries))
            // #define IsEmptyDBlk(blk) (FIRSTENTRY(blk)->next == 0)
            //return FIRSTENTRY(blk.dirblock).next == 0;
            return blk.dirblock.DirEntries.Count == 0;
        }

        public static bool IsUpdateNeeded(int rtbf_threshold, Pfs3GlobalData g)
        {
/* checks if update is needed now */
// #define IsUpdateNeeded(rtbf_threshold)                              \
//         ((alloc_data.rtbf_index > rtbf_threshold) ||                    \
//         (g->rootblock->reserved_free < RESFREE_THRESHOLD + 5 + alloc_data.tbf_resneed))         \

            var alloc_data = g.glob_allocdata;
            return ((alloc_data.rtbf_index > rtbf_threshold) ||
                    (g.RootBlock.ReservedFree < Pfs3Constants.RESFREE_THRESHOLD + 5 + alloc_data.tbf_resneed));
        }
    }
}
