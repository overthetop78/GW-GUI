namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.Collections.Generic;
    using System.IO;
    using Blocks;

    public class Pfs3GlobalData
    {
        public Pfs3RootBlock RootBlock;
        public Pfs3DosEnvec DosEnvec { get; set; }
        public uint NumBuffers;
        public Pfs3LruData glob_lrudata;

        public bool IgnoreProtectionBits { get; set; }
        public bool ResolveLinkPaths { get; set; }

        /* LRU stuff */
        public bool uip;                           /* update in progress flag              */
        public ushort locknr;                       /* prevents blocks from being flushed   */

        // // ULONG de_SizeBlock;	     /* in longwords: Physical disk block size */
        // public uint SizeBlock;

        /// <summary>
        /// Physical disk block size (512)
        /// </summary>
        public uint blocksize;                    /* g->dosenvec->de_SizeBlock << 2       */
        public ushort blockshift;                   /* 2 log van block size                 */
        public ushort fnsize;						/* filename size (18+)					*/
        public int directsize;                   /* number of blocks after which direct  */

        public uint firstblock;/* first and last block of partition    */
        public uint lastblock;

        /* disktype: ID_PFS_DISK/NO_DISK_PRESENT/UNREADABLE_DISK
         * (only valid if currentvolume==NULL)
         */
        public uint disktype;

        /* state of currentvolume (ID_WRITE_PROTECTED/VALIDATED/VALIDATING) */
        public uint diskstate;

        /* 1 if 'ACTION_WRITE_PROTECTED'     	*/
        public bool softprotect;

        public Pfs3VolumeData currentvolume;
        public bool dirty;
        public long protectkey;
        public bool harddiskmode;
        public bool anodesplitmode;
        public bool dirextension;
        public bool largefile;
        public bool deldirenabled;
        public Pfs3AnodeData glob_anodedata;
        public Pfs3AllocationData glob_allocdata;

        // stream for data io
        public Stream stream;

        public ushort infoblockshift;
        public bool updateok;

        public Pfs3GlobalData()
        {
            glob_lrudata = new Pfs3LruData();
            glob_anodedata = new Pfs3AnodeData();
            glob_allocdata = new Pfs3AllocationData();
            dc = new Pfs3DiskCache();
            SearchInDirCache = new Dictionary<uint, Pfs3SearchInDirCacheItem>();
            ResolveLinkPaths = true;
        }

        public uint TotalSectors { get; set; }
        public bool SuperMode { get; set; }

        public Pfs3DiskCache dc;                /* cache to make '196 byte mode' faster */


        public readonly IDictionary<uint, Pfs3SearchInDirCacheItem> SearchInDirCache;
    }

    public class Pfs3SearchInDirCacheItem
    {
        public readonly uint dirnodenr;
        public readonly Pfs3CachedBlock DirBlock;
        public readonly IDictionary<string, Pfs3DirEntry> DirEntriesCache;

        public Pfs3SearchInDirCacheItem(uint dirnodenr, Pfs3CachedBlock dirBlock)
        {
            this.dirnodenr = dirnodenr;
            DirBlock = dirBlock;
            DirEntriesCache = new Dictionary<string, Pfs3DirEntry>();
        }
    }
}
