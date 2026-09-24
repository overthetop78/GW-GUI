namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3AnodeChainNode
    {
        // struct Pfs3AnodeChainNode
        // {
        //     struct Pfs3AnodeChainNode *next;
        //     struct Pfs3Canode an;
        // };

        public Pfs3AnodeChainNode next;
        public Pfs3Canode an;

        public Pfs3AnodeChainNode()
        {
            an = new Pfs3Canode();
        }
    }
}
