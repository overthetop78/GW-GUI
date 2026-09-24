namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.Threading.Tasks;
    using Blocks;
    using Exceptions;

    public partial class Pfs3Anodes
    {
        /**********************************************************************/
        /* indexblocks                                                        */
        /**********************************************************************/

        /*
         * get indexblock nr
         * returns NULL if failure
         */
        public static async Task<Pfs3CachedBlock> GetIndexBlock(ushort nr, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"anodes: GetIndexBlock, seqnr = {nr}");
#endif
            uint blocknr, temp;
            Pfs3CachedBlock indexblk;
            Pfs3CachedBlock superblk;
            var volume = g.currentvolume;
            var andata = g.glob_anodedata;

            /* check cache (can be empty) */
            if (volume.indexblksBySeqNr.ContainsKey(nr))
            {
                indexblk = volume.indexblksBySeqNr[nr];
                Pfs3Lru.MakeLRU(indexblk, g);
                return indexblk;
            }

            /* not in cache, put it in
	 * first, get blocknr
	 */
            if (g.SuperMode)
            {
                /* temp is chopped by auto cast */
                temp = Pfs3Init.divide(nr, andata.indexperblock);
                if ((superblk = await GetSuperBlock((ushort)temp, g)) == null)
                {
                    return null;
                }

                if ((blocknr = (uint)superblk.IndexBlock.index[temp >> 16]) == 0)
                {
                    return null;
                }
            }
            else
            {
                if (nr > Pfs3Constants.MAXSMALLINDEXNR || (blocknr = g.RootBlock.idx.small.indexblocks[nr]) == 0)
                    return null;
            }

            /* allocate space from cache */
            if ((indexblk = await Pfs3Lru.AllocLRU(g)) == null)
            {
                return null;
            }

#if DEBUG
            Pfs3Logger.Instance.Debug($"anodes: GetIndexBlock, seqnr = {nr}, block nr {blocknr}");
#endif

            IPfs3Block blk;
            if ((blk = await Pfs3Disk.RawRead<Pfs3IndexBlock>(g.currentvolume.rescluster, blocknr, g)) == null)
            {
                Pfs3Lru.FreeLRU(indexblk, g);
                return null;
            }

            indexblk.blk = blk;

            if (blk.id == Pfs3Constants.IBLKID)
            {
                indexblk.volume = volume;
                indexblk.blocknr = blocknr;
                indexblk.used = 0;
                indexblk.changeflag = false;
                Pfs3Macro.AddToIndexes(volume.indexblks, volume.indexblksBySeqNr, indexblk);
            }
            else
            {
                Pfs3Lru.FreeLRU(indexblk, g);
                return null;
            }

            return indexblk;
        }

        public static async Task<Pfs3CachedBlock> GetSuperBlock(ushort nr, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"anodes: GetSuperBlock, seqnr = {nr}");
#endif
            uint blocknr;
            Pfs3CachedBlock superblk;
            var volume = g.currentvolume;


            /* check supermode */
            if (!g.SuperMode)
            {
                return null;
            }

            /* check cache (can be empty) */
            if (volume.superblksBySeqNr.ContainsKey(nr))
            {
                superblk = volume.superblksBySeqNr[nr];
                Pfs3Lru.MakeLRU(superblk, g);
                return superblk;
            }

            /* not in cache, put it in
             * first, get blocknr
             */
            if (nr > Pfs3Constants.MAXSUPER || (blocknr = volume.rblkextension.rblkextension.superindex[nr]) == 0)
            {
                return null;
            }

            /* allocate space from cache */
            if ((superblk = await Pfs3Lru.AllocLRU(g)) == null)
            {
                return null;
            }

#if DEBUG
            Pfs3Logger.Instance.Debug($"anodes: GetSuperBlock, seqnr = {nr}, block nr = {blocknr}");
#endif

            IPfs3Block blk;
            if ((blk = await Pfs3Disk.RawRead<Pfs3IndexBlock>(g.currentvolume.rescluster, blocknr, g)) == null)
            {
                Pfs3Lru.FreeLRU(superblk, g);
                return null;
            }

            superblk.blk = blk;

            if (superblk.blk.id == Pfs3Constants.SBLKID)
            {
                superblk.volume = volume;
                superblk.blocknr = blocknr;
                superblk.used = 0;
                superblk.changeflag = false;
                Pfs3Macro.AddToIndexes(volume.superblks, volume.superblksBySeqNr, superblk);
            }
            else
            {
                Pfs3Lru.FreeLRU(superblk, g);
                return null;
            }

            return superblk;
        }

        public static async Task<Pfs3CachedBlock> NewSuperBlock(ushort seqnr, Pfs3GlobalData g)
        {
            Pfs3CachedBlock blok;
            var volume = g.currentvolume;


            if ((seqnr > Pfs3Constants.MAXSUPER) || (blok = await Pfs3Lru.AllocLRU(g)) == null)
            {
                return null;
            }

            if ((volume.rblkextension.rblkextension.superindex[seqnr] = Pfs3Allocation.AllocReservedBlock(g)) == 0)
            {
                Pfs3Lru.FreeLRU(blok, g);
                return null;
            }


            volume.rblkextension.changeflag = true;

            blok.volume     = volume;
            blok.blocknr    = volume.rblkextension.rblkextension.superindex[seqnr];
            blok.used       = 0;

            if (blok.IndexBlock == null)
            {
                blok.blk = new Pfs3IndexBlock(g);
            }

            var blok_cblk = blok.IndexBlock;
            blok_cblk.id     = Pfs3Constants.SBLKID;
            blok_cblk.seqnr  = seqnr;
            blok.changeflag = true;
            Pfs3Macro.AddToIndexes(volume.superblks, volume.superblksBySeqNr, blok);

            return blok;
        }

        /* Find out how large the anblkbitmap must be, allocate it and
 * initialise it. Free any preexisting anblkbitmap
 *
 * The anode bitmap is used for allocating Pfs3Anodes. It has the
 * following properties:
 * - It is  maintained in memory only (not on disk).
 * - Intialization is lazy: all anodes are marked as available
 * - When allocation anodes (see AllocAnode), this bitmap is used
 *   to find available Pfs3Anodes. It then checks with the actual
 *   anode (which should be 0,0,0 if available). If it isn't really
 *   available, the anodebitmap is updated, otherwise the anode is
 *   taken.
 */
        public static async Task MakeAnodeBitmap(bool formatting, Pfs3GlobalData g)
        {
            Pfs3CachedBlock iblk;
            Pfs3CachedBlock sblk;
            int i, j, s = 0;
            uint size;
            var andata = g.glob_anodedata;


            /* count number of anodeblocks and allocate bitmap */
            if (formatting)
            {
                i = 0;
                s = 0;
                j = 1;
            }
            else
            {
                if (g.SuperMode)
                {
                    for (s = Pfs3Constants.MAXSUPER; s >= 0 && g.currentvolume.rblkextension.rblkextension.superindex[s] == 0; s--)
                    {
                    }

                    if (s < 0)
                    {
                        throw new FileSystemDiagnosticException(FileSystemErrorCode.AnodeOperationFailed,
                            Pfs3ErrorMessages.AnodeOperationFailed);
                    }

                    sblk = await GetSuperBlock((ushort)s, g);


                    var sblk_blk = sblk.IndexBlock;
                    for (i = andata.indexperblock - 1; i >= 0 && sblk_blk.index[i] == 0; i--)
                    {
                    }
                }
                else
                {
                    for (s = 0, i = Pfs3Constants.MAXSMALLINDEXNR; i >= 0 && g.RootBlock.idx.small.indexblocks[(uint)i] == 0; i--)
                    {
                    }
                }

                if (i < 0)
                {
                    throw new FileSystemDiagnosticException(FileSystemErrorCode.AnodeOperationFailed,
                        Pfs3ErrorMessages.AnodeOperationFailed);
                }

                iblk = await GetIndexBlock((ushort)(s * andata.indexperblock + i), g);

                if (iblk == null)
                {
                            throw new FileSystemDiagnosticException(
                                FileSystemErrorCode.AnodeBitmapIndexBlockMissing,
                                Pfs3ErrorMessages.AnodeBitmapIndexBlockMissing(s, i), s, i);
                }

                var iblk_blk = iblk.IndexBlock;
                for (j = andata.indexperblock - 1; j >= 0 && iblk_blk.index[j] == 0; j--)
                {
                }
            }

            if (g.SuperMode)
            {
                andata.maxanseqnr =
                    (uint)(s * andata.indexperblock * andata.indexperblock + i * andata.indexperblock + j);
                size = (uint)(((s * andata.indexperblock + i + 1) * andata.indexperblock + 7) / 8);
            }
            else
            {
                andata.maxanseqnr = (uint)(i * andata.indexperblock + j);
                size = (uint)(((i + 1) * andata.indexperblock + 7) / 8);
            }

            andata.anblkbitmapsize = (uint)((size + 3) & ~3);
            andata.anblkbitmap = new uint[andata.anblkbitmapsize];

            for (i = 0; i < andata.anblkbitmapsize / 4; i++)
            {
                andata.anblkbitmap[i] = 0xffffffff; /* all available */
            }
        }

/*
 * Retrieve an anode from disk
 */
    }
}
