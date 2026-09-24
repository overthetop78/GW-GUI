namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;

    public partial class Pfs3Anodes
    {
        public static async Task<Pfs3CachedBlock> big_GetAnodeBlock(ushort seqnr, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"anodes: GetAnodeBlock, seqnr = {seqnr}");
#endif
            uint blocknr;
            uint temp;
            Pfs3CachedBlock ablock;
            Pfs3CachedBlock indexblock;
            var volume = g.currentvolume;
            var andata = g.glob_anodedata;

            temp = Pfs3Init.divide(seqnr, andata.indexperblock);

            /* not in cache, put it in */
            /* get the indexblock */
            if ((indexblock = await GetIndexBlock((ushort)temp /*& 0xffff*/, g)) == null)
            {
                return null;
            }

            /* get blocknr */
            if ((blocknr = (uint)indexblock.IndexBlock.index[temp >> 16]) == 0)
            {
                return null;
            }

            /* check cache */
            ablock = Pfs3Lru.CheckCache(volume.anblks, blocknr, g);
            if (ablock != null)
                return ablock;

            if ((ablock = await Pfs3Lru.AllocLRU(g)) == null)
            {
                return null;
            }

#if DEBUG
            Pfs3Logger.Instance.Debug($"anodes: GetAnodeBlock, seqnr = {seqnr}, blocknr = {blocknr}");
#endif

            /* read it */
            IPfs3Block blk;
            if ((blk = await Pfs3Disk.RawRead<Pfs3AnodeBlock>(g.currentvolume.rescluster, blocknr, g)) == null)
            {
                Pfs3Lru.FreeLRU(ablock, g);
                return null;
            }

            ablock.blk = blk;

            /* check it */
            if (ablock.blk.id != Pfs3Constants.ABLKID)
            {
                Pfs3Lru.FreeLRU(ablock, g);
                return null;
            }

            /* initialize it */
            ablock.volume     = volume;
            ablock.blocknr    = blocknr;
            ablock.used       = 0;
            ablock.changeflag = false;
            Pfs3Macro.Hash(ablock, volume.anblks);

            return ablock;
        }

        public static async Task<Pfs3CachedBlock> big_NewAnodeBlock(ushort seqnr, Pfs3GlobalData g)
        {
            /* MODE_BIG has difference between anodeblocks and fnodeblocks*/

            Pfs3CachedBlock blok;
            var volume = g.currentvolume;
            var andata = g.glob_anodedata;
            Pfs3CachedBlock indexblock;
            uint indexblnr;
            int blocknr;
            ushort indexoffset, oldlock;

            /* get indexblock */
            indexblnr = (uint)(seqnr / andata.indexperblock);
            indexoffset = (ushort)(seqnr % andata.indexperblock);
            if ((indexblock = await GetIndexBlock((ushort)indexblnr, g)) == null) {
                if ((indexblock = await NewIndexBlock((ushort)indexblnr, g)) == null) {
                    return null;
                }
            }

            oldlock = indexblock.used;
            Pfs3Cache.LOCK(indexblock, g);
            if ((blok = await Pfs3Lru.AllocLRU(g)) == null || (blocknr = (int)Pfs3Allocation.AllocReservedBlock(g)) == 0 ) {
                indexblock.used = oldlock;         // unlock block
                return null;
            }


            indexblock.IndexBlock.index[indexoffset] = blocknr;

            indexblock.changeflag = true;

            blok.volume     = volume;
            blok.blocknr    = (uint)blocknr;
            blok.used       = 0;
            blok.blk = new Pfs3AnodeBlock(g)
            {
                id = Pfs3Constants.ABLKID,
                seqnr = seqnr
            };
            blok.changeflag = true;
            Pfs3Macro.Hash(blok, volume.anblks);
            await Pfs3Update.MakeBlockDirty(indexblock, g);
            indexblock.used = oldlock;         // unlock block

            ReallocAnodeBitmap(seqnr, g);
            return blok;
        }

        public static async Task<Pfs3CachedBlock> NewIndexBlock(ushort seqnr, Pfs3GlobalData g)
        {
            Pfs3CachedBlock blok;
            Pfs3CachedBlock superblok = null;
            var volume = g.currentvolume;
            var andata = g.glob_anodedata;
            uint superblnr = 0;
            int blocknr;
            ushort superoffset = 0;

            if (g.SuperMode)
            {
                superblnr = (uint)(seqnr / andata.indexperblock);
                superoffset = (ushort)(seqnr % andata.indexperblock);
                if ((superblok = await GetSuperBlock((ushort)superblnr, g)) == null)
                {
                    if ((superblok = await NewSuperBlock((ushort)superblnr, g)) == null)
                    {
                        return null;
                    }
                }

                Pfs3Cache.LOCK(superblok, g);
            }
            else if (seqnr > Pfs3Constants.MAXSMALLINDEXNR) {
                return null;
            }

                if ((blok = await Pfs3Lru.AllocLRU(g)) == null || (blocknr = (int)Pfs3Allocation.AllocReservedBlock(g)) == 0)
            {
                if (blok != null)
                    Pfs3Lru.FreeLRU(blok, g);
                return null;
            }


            if (g.SuperMode) {
                superblok.IndexBlock.index[superoffset] = blocknr;
                await Pfs3Update.MakeBlockDirty(superblok, g);
            } else {
                g.RootBlock.idx.small.indexblocks[seqnr] = (uint)blocknr;
                volume.rootblockchangeflag = true;
            }

            blok.volume     = volume;
            blok.blocknr    = (uint)blocknr;
            blok.used       = 0;
            blok.blk = new Pfs3IndexBlock(g)
            {
                id = Pfs3Constants.IBLKID,
                seqnr = seqnr
            };
            blok.changeflag = true;
            Pfs3Macro.AddToIndexes(volume.indexblks, volume.indexblksBySeqNr, blok);

            return blok;
        }

/* test if new anodeseqnr causes change in anblkbitmap */
    }
}
