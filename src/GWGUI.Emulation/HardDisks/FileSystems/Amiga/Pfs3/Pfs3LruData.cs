namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.Collections.Generic;
    using Blocks;

    public class Pfs3LruData
    {
        /* the LRU global data */
        // struct Pfs3LruData
        // {
        //     struct Pfs3DoctorMinList LRUqueue;
        //     struct Pfs3DoctorMinList LRUpool;
        //     ULONG poolsize;
        //     struct lru_cachedblock **LRUarray;
        //     UWORD reserved_blksize;
        // };

        public LinkedList<Pfs3LruCachedBlock> LRUqueue;
        public LinkedList<Pfs3LruCachedBlock> LRUpool;

        /// <summary>
        /// pfs3 uses lru array together num buffers for partition
        /// and increases it, if lru pool is empty, then it adds
        /// 5 new entries to lru array which is added lru pool.
        /// it means that lru array only seems to be used as a way to allocate
        /// new lru cached blocks in Lru Alloc.Lru method.
        /// therefore use lry array is disabled by default as it will
        /// expand continuously up over time with new entries added
        /// every time lru pool is empty.
        /// </summary>
        public bool useLruArray { get; set; }
        public uint poolsize;
        public Pfs3LruCachedBlock[] LRUarray;

        public ushort reservedBlksize;

        public Pfs3LruData()
        {
            useLruArray = false;
        }
    };
}
