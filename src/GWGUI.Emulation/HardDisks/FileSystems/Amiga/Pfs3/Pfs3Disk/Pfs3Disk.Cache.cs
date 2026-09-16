namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;
    using Core.Extensions;

    public static partial class Pfs3Disk
    {
        public static int CheckDataCache(uint blocknr, Pfs3GlobalData g)
        {
            int i;

            for (i = 0; i < g.dc.size; i++)
            {
                if (g.dc.ref_[i].blocknr == blocknr)
                {
                    return i;
                }
            }

            return -1;
        }

/* get block from cache or put it in cache if it wasn't
 * there already. return cache slotnr. errors are indicated by 'error'
 * (null = ok)
 */
        public static async Task<int> CachedRead(uint blocknr, bool fake, Pfs3GlobalData g)
        {
            int i;

            i = CheckDataCache(blocknr, g);
            if (i != -1) return i;
            i = g.dc.roving;
            if (g.dc.ref_[i].dirty && g.dc.ref_[i].blocknr != 0)
            {
                await UpdateSlot(i, g);
            }

            if (fake)
            {
                for (var f = i << (int)Pfs3Macro.BLOCKSHIFT(g); f < Pfs3Macro.BLOCKSIZE(g); f++)
                {
                    g.dc.data[f] = 0xAA;
                }
            }
            else
            {
                var data = await RawRead(1, blocknr, g);
                Array.Copy(data, 0, g.dc.data, i << Pfs3Macro.BLOCKSHIFT(g), data.Length);
            }

            g.dc.roving = (ushort)((g.dc.roving + 1) & g.dc.mask);
            g.dc.ref_[i].dirty = false;
            g.dc.ref_[i].blocknr = blocknr;
            return i;
        }

        public static async Task<byte[]> CachedReadD(uint blknr, Pfs3GlobalData g)
        {
            var i = await CachedRead(blknr, false, g);
            var blockSize = Pfs3Macro.BLOCKSIZE(g);
            var buffer = new byte[blockSize];
            Array.Copy(g.dc.data, i<<Pfs3Macro.BLOCKSHIFT(g), buffer, 0, blockSize);
            return buffer;
        }

/* Read from rollover: at end of file,
 * goto start
 */
    }
}
