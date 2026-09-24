namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;

    public partial class Pfs3Anodes
    {
        public static void ReallocAnodeBitmap(uint newseqnr, Pfs3GlobalData g)
        {
            uint newsize;
            int t;
            var andata = g.glob_anodedata;

            if (newseqnr > andata.maxanseqnr)
            {
                andata.maxanseqnr = newseqnr;
                newsize = ((newseqnr/andata.indexperblock + 1) * andata.indexperblock + 7) / 8;
                if (newsize > andata.anblkbitmapsize)
                {
                    newsize = (uint)((newsize + 3) & ~3);   /* longwords */
                    var newbitmap = new uint[newsize];
                    for (t = 0; t < newsize / 4; t++)
                        newbitmap[t] = 0xffffffff;
                    andata.anblkbitmap = newbitmap;
                    andata.anblkbitmapsize = newsize;
                }
            }
        }

/* Remove anode from anodechain
 * If previous==0, anode->next becomes head.
 * Otherwise previous->next becomes anode->next.
 * Anode is freed.
 *
 * Arguments:
 * anode = anode to be removed
 * previous = previous in chain; or 0 if anode is head
 * head = anodenr of head of list
 */
        public static async Task RemoveFromAnodeChain(Pfs3Canode anode, uint previous, uint head, Pfs3GlobalData g)
        {
            Pfs3Canode sparenode = new Pfs3Canode();

            if(previous != 0)
            {
                await GetAnode(sparenode, previous, g);
                sparenode.next = anode.next;
                await SaveAnode(sparenode, sparenode.nr, g);
                await FreeAnode(anode.nr, g);
            }
            else
            {
                /* anode is head of list (check both tails here) */
                if (anode.next != 0)
                {
                    /* There is a next entry -> becomes head */
                    await GetAnode(sparenode, anode.next, g);
                    await SaveAnode(sparenode, head, g); // overwrites [anode]
                    await FreeAnode(anode.next, g);
                }
                else
                {
                    /* No anode->next: Free list. */
                    await FreeAnode(head, g);
                }
            }
        }

/*
 * frees an anode for later reuse
 * universal version
 */
        public static async Task FreeAnode(uint anodenr, Pfs3GlobalData g)
        {
            Pfs3Canode anode = new Pfs3Canode();
            var andata = g.glob_anodedata;

            /* don't kill reserved anodes */
            if (anodenr < Pfs3Constants.ANODE_USERFIRST)
            {
                anode.blocknr = UInt32.MaxValue;
            }

            await SaveAnode(anode, anodenr, g);
            andata.anblkbitmap[(anodenr>>16)/32] |= (uint)(1 << (31 - (int)((anodenr>>16) % 32)));
        }

/*
 * makes anodechain
 */
        public static async Task<Pfs3AnodeChain> MakeAnodeChain(uint anodenr, Pfs3GlobalData g)
        {

            var ac = new Pfs3AnodeChain
            {
                refcount = 0
            };

            var node = ac.head;
            await GetAnode(node.an, anodenr, g);
            while (node.an != null && node.an.next != 0)
            {
                var newnode = new Pfs3AnodeChainNode();
                node.next = newnode;
                await GetAnode(newnode.an, node.an.next, g);
                node = newnode;
            }

            Pfs3Macro.MinAddHead(g.currentvolume.anodechainlist, ac);
            return ac;

        }

        /*
 * Get anodechain of anodenr, making it if necessary.
 * Returns chain or NULL for failure
 */
        public static async Task<Pfs3AnodeChain> GetAnodeChain(uint anodenr, Pfs3GlobalData g)
        {
            Pfs3AnodeChain ac;

            if ((ac = FindAnodeChain(anodenr, g)) == null)
                ac = await MakeAnodeChain(anodenr, g);
            if (ac != null)
                ac.refcount++;

            return ac;
        }

        /*
 * search anodechain.
 * Return anodechain found, or 0 if not found
 */
        public static Pfs3AnodeChain FindAnodeChain (uint anodenr, Pfs3GlobalData g)
        {
            Pfs3AnodeChain chain;

            for (var node = Pfs3Macro.HeadOf(g.currentvolume.anodechainlist); node != null; node = node.Next)
            {
                chain = node.Value;
                if (chain.head.an.nr == anodenr)
                    return chain;
            }

            return null;
        }

/*
 * Tries to fetch the block that follows after anodeoffset. Returns
 * success and anodeoffset is updated.
 */
        public static async Task<Tuple<bool, uint>> NextBlock(Pfs3Canode anode, uint anodeoffset, Pfs3GlobalData g)
        {
            anodeoffset++;
            return await CorrectAnode(anode, anodeoffset, g);
        }

/*
 * Correct anodeoffset overflow
 */
        /// <summary>
        /// Note: anodeoffset can be changed in method and async doesn't allow ref,
        /// therefore both boolean and updated anodeoffset is returned
        /// </summary>
        /// <param name="anode"></param>
        /// <param name="anodeoffset"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static async Task<Tuple<bool, uint>> CorrectAnode (Pfs3Canode anode, uint anodeoffset, Pfs3GlobalData g)
        {
            while(anodeoffset >= anode.clustersize)
            {
                if (anode.next == 0)
                {
                    return new Tuple<bool, uint>(false, anodeoffset);
                }

                anodeoffset -= anode.clustersize;
                await GetAnode(anode, anode.next, g);
            }

            return new Tuple<bool, uint>(true, anodeoffset);
        }

        /*
 * Correct anodeoffset overflow. Corrects anodechainnode pointer pointed to by acnode.
 * Returns success. If correction was not possible, acnode will be the tail of the
 * anodechain. Anodeoffset is updated to point to a block within the current
 * anodechainnode.
 */
        public static bool CorrectAnodeAC(ref Pfs3AnodeChainNode acnode, ref uint anodeoffset, Pfs3GlobalData g)
        {
            while (anodeoffset >= acnode.an.clustersize)
            {
                if (acnode.next == null)
                    return false;

                anodeoffset -= acnode.an.clustersize;
                acnode = acnode.next;
            }

            return true;
        }

/*
 * Called when a reference to an anodechain ceases to exist
 */
        public static void DetachAnodeChain(Pfs3AnodeChain chain, Pfs3GlobalData g)
        {
            chain.refcount--;
            if (chain.refcount == 0)
                FreeAnodeChain(chain, g);
        }

/*
 * Free an anodechain. Anodechain will be removed from list if it is
 * in the list.
 */
        public static void FreeAnodeChain(Pfs3AnodeChain chain, Pfs3GlobalData g)
        {

            if (chain != null)
            {
                Pfs3Macro.MinRemove(chain, g); // remove from any list
            }
        }

/*
 * Tries to fetch the block that follows after anodeoffset. Returns success,
 * updates anodechainpointer and anodeoffset. If failed, acnode will point to
 * the tail of the anodechain
 */
        public static bool NextBlockAC(ref Pfs3AnodeChainNode acnode, ref uint anodeoffset, Pfs3GlobalData g)
        {
            anodeoffset++;
            return CorrectAnodeAC(ref acnode, ref anodeoffset, g);
        }
    }
}
