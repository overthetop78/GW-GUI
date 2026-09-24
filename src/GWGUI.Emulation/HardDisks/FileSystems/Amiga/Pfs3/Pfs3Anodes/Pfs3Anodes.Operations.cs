namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;
    using Exceptions;

    public partial class Pfs3Anodes
    {
        public static async Task GetAnode(Pfs3Canode anode, uint anodenr, Pfs3GlobalData g)
        {
            uint temp;
            ushort seqnr, anodeoffset;
            Pfs3CachedBlock ablock;
            var andata = g.glob_anodedata;

            if(g.anodesplitmode)
            {
                var split = Pfs3Macro.SplitAnodenr(anodenr);
                seqnr = split.seqnr;
                anodeoffset = split.offset;
            }
            else
            {
                temp		 = Pfs3Init.divide(anodenr, andata.anodesperblock);
                seqnr        = (ushort)temp;				// 1e block = 0
                anodeoffset  = (ushort)(temp >> 16);
            }

            ablock = await big_GetAnodeBlock(seqnr, g);
            if(ablock != null)
            {
                var ablock_blk = ablock.ANodeBlock;
                anode.clustersize = ablock_blk.nodes[anodeoffset].clustersize;
                anode.blocknr     = ablock_blk.nodes[anodeoffset].blocknr;
                anode.next        = ablock_blk.nodes[anodeoffset].next;
                anode.nr          = anodenr;
            }
            else
            {
                anode.clustersize = anode.next = 0;
                throw new FileSystemDiagnosticException(FileSystemErrorCode.AnodeMissing,
                    Pfs3ErrorMessages.AnodeMissing(anodenr), anodenr);
            }
        }

/* saves and anode..
*/
        public static async Task SaveAnode(Pfs3Canode anode, uint anodenr, Pfs3GlobalData g)
        {
            uint temp;
            ushort seqnr, anodeoffset;
            var andata = g.glob_anodedata;

            if (g.anodesplitmode)
            {
                var split = Pfs3Macro.SplitAnodenr(anodenr);
                seqnr = split.seqnr;
                anodeoffset = split.offset;
            }
            else
            {
                temp = Pfs3Init.divide(anodenr, andata.anodesperblock);
                seqnr = (ushort)temp; // 1e block = 0
                anodeoffset = (ushort)(temp >> 16);
            }

            anode.nr = anodenr;

            /* Save Anode */
            var ablock = await Pfs3Macro.GetAnodeBlock(seqnr, g);
            if (ablock != null)
            {
                var anode_blk = ablock.ANodeBlock;
                anode_blk.nodes[anodeoffset].clustersize = anode.clustersize;
                anode_blk.nodes[anodeoffset].blocknr = anode.blocknr;
                anode_blk.nodes[anodeoffset].next = anode.next;
                await Pfs3Update.MakeBlockDirty(ablock, g);
            }
            else
            {
                throw new FileSystemDiagnosticException(FileSystemErrorCode.AnodeBlockAllocationFailed,
                    Pfs3ErrorMessages.AnodeBlockAllocationFailed);
            }
        }

/* allocates an anode and marks it as reserved
 * connect is anodenr to connect to (0 = no connection)
 */
        public static async Task<uint> AllocAnode(uint connect, Pfs3GlobalData g)
        {
#if DEBUG
            Pfs3Logger.Instance.Debug($"anodes: AllocAnode, connect = {connect}");
#endif
            int i, j, k = 0;
            Pfs3CachedBlock ablock = null;
            Pfs3Anode[] anodes = null;
            bool found = false;
            uint seqnr = 0, field;

            var andata = g.glob_anodedata;

            if (connect != 0 && g.anodesplitmode)
            {
                /* try to place new anode in same block */
                ablock = await Pfs3Init.big_GetAnodeBlock((ushort)(seqnr = connect >> 16), g);
                if (ablock != null)
                {
                    anodes = ablock.ANodeBlock.nodes;
                    for (k = andata.anodesperblock - 1; k > -1 && !found; k--)
                        found = (anodes[k].clustersize == 0 &&
                                 anodes[k].blocknr == 0 &&
                                 anodes[k].next == 0);
                }
            }
            else
            {
                for (i = andata.curranseqnr / 32; i < andata.maxanseqnr / 32 + 1; i++)
                {

                    field = andata.anblkbitmap[i];
                    if (field != 0)
                    {
                        for (j = 31; j >= 0; j--)
                        {
                            if ((field & (1 << j)) != 0)
                            {
                                seqnr = (uint)(i * 32 + 31 - j);
                                ablock = await Pfs3Init.big_GetAnodeBlock((ushort)seqnr, g);
                                if (ablock != null)
                                {
                                    anodes = ablock.ANodeBlock.nodes;
                                    for (k = 0; k < andata.reserved && !found; k++)
                                        found = (anodes[k].clustersize == 0 &&
                                                 anodes[k].blocknr == 0 &&
                                                 anodes[k].next == 0);

                                    if (found)
                                        goto found_it;
                                    else
                                        /* mark anodeblock as full */
                                        andata.anblkbitmap[i] &= (uint)(~(1 << j));
                                }
                                /* anodeblock does not exist */
                                else goto found_it;
                            }
                        }
                    }
                }

                seqnr = andata.maxanseqnr + 1;
            }

            found_it:

            if (!found)
            {
                /* give up connect mode and try again */
                if (connect != 0)
                {
                    return await AllocAnode(0, g);
                }

                /* start over if not started from start of list;
                 * else make new block
                 */
                if (andata.curranseqnr != 0)
                {
                    andata.curranseqnr = 0;
                    return await AllocAnode(0, g);
                }
                else
                {
                    if ((ablock = await big_NewAnodeBlock((ushort)seqnr, g)) == null)
                    {
                        return 0;
                    }
                    anodes = ablock.ANodeBlock.nodes;
                    k = 0;
                }
            }
            else
            {
                if (connect != 0)
                    k++;
                else
                    k--;
            }

            anodes[k].clustersize = 0;
            anodes[k].blocknr = 0xffffffff;
            anodes[k].next = 0;

            await Pfs3Update.MakeBlockDirty(ablock, g);
            andata.curranseqnr = (ushort)seqnr;

            if (g.anodesplitmode)
                return (seqnr << 16 | (uint)k);
            else
                return (uint)(seqnr * andata.anodesperblock + k);
        }

/* MODE_BIG has indexblocks, and negative blocknrs indicating freenode
** blocks instead of anodeblocks
*/
    }
}
