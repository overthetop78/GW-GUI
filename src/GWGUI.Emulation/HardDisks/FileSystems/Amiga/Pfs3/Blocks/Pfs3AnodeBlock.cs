namespace Hst.Amiga.FileSystems.Pfs3.Blocks
{
    public class Pfs3AnodeBlock : IPfs3Block
    {
        // typedef struct Pfs3AnodeBlock
        // {
        //     UWORD id;               /* AB                               */
        //     UWORD not_used;
        //     ULONG datestamp;
        //     ULONG seqnr;
        //     ULONG not_used_2;
        //     struct Pfs3Anode nodes[0];
        // } anodeblock_t;

        public byte[] BlockBytes { get; set; }

        public ushort id { get; set; }
        public ushort not_used_1 { get; set; }
        public uint datestamp { get; set; }
        public uint seqnr;
        public uint not_used_2;
        public Pfs3Anode[] nodes;

        public Pfs3AnodeBlock(Pfs3GlobalData g)
        {
            id = Pfs3Constants.ABLKID; /* AB                               */
            nodes = new Pfs3Anode[(g.RootBlock.ReservedBlksize - Amiga.SizeOf.UWord * 2 - Amiga.SizeOf.ULong * 3) / Pfs3Anode.Size];
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Pfs3Anode();
            }
        }
    }
}
