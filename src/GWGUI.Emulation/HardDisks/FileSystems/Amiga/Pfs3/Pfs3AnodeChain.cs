namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3AnodeChain
    {
        // struct Pfs3AnodeChain
        // {
        //     struct Pfs3AnodeChain *next;
        //     struct Pfs3AnodeChain *prev;
        //     ULONG refcount;             /* will be discarded if refcount becomes 0 */
        //     struct Pfs3AnodeChainNode head;
        // };
        public uint refcount;
        public Pfs3AnodeChainNode head;

        public Pfs3AnodeChain()
        {
            head = new Pfs3AnodeChainNode();
            refcount = 0;
        }
    }
}
