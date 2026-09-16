namespace Hst.Amiga.FileSystems.Pfs3.Blocks
{
    /// <summary>
    /// cache block.
    /// cbitmapblock, cindexblock, canodeblock, cdirblock, cdeldirblock, crootblockextension, (cachedblock)
    /// </summary>
    public class Pfs3CachedBlock
    {
        /* Cached blocks in general
        */
        // struct Pfs3DoctorCachedBlock
        // {
        //     struct Pfs3DoctorCachedBlock	*next;
        //     struct Pfs3DoctorCachedBlock	*prev;
        //     struct Pfs3VolumeData	*volume;
        //     ULONG	blocknr;				// the current (new) blocknumber of the block
        //     ULONG	oldblocknr;				// the blocknr before reallocation. NULL if not reallocated.
        //     UWORD	used;					// block locked if used == g->locknr
        //     UBYTE	changeflag;				// dirtyflag
        //     UBYTE	dummy;					// pad to make offset even
        //     UBYTE	data[0];				// the datablock;
        // };

        public Pfs3VolumeData volume { get; set; }

        /// <summary>
        /// the current (new) blocknumber of the block
        /// </summary>
        public uint blocknr { get; set; }

        /// <summary>
        /// the blocknr before reallocation. NULL if not reallocated.
        /// </summary>
        public uint oldblocknr { get; set; }

        /// <summary>
        /// block locked if used == g->locknr
        /// </summary>
        public ushort used { get; set; }

        /// <summary>
        /// dirtyflag
        /// </summary>
        public bool changeflag { get; set; }

        //UBYTE	dummy;					// pad to make offset even

        public IPfs3Block blk { get; set; }

        public Pfs3AnodeBlock ANodeBlock => blk as Pfs3AnodeBlock;

        public Pfs3IndexBlock IndexBlock => blk as Pfs3IndexBlock;

        public Pfs3RootBlockExtension rblkextension => blk as Pfs3RootBlockExtension;

        public Pfs3DelDirBlock deldirblock => blk as Pfs3DelDirBlock;

        public Pfs3DirBlock dirblock => blk as Pfs3DirBlock;

        public Pfs3BitmapBlock BitmapBlock => blk as Pfs3BitmapBlock;
    }
}
