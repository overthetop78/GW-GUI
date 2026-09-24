namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3RefTable
    {
        public uint blocknr;          /* blocknr of cached block; 0 = empty slot */
        public bool dirty;            /* dirty flag (TRUE/FALSE) */
        public byte pad;
    };
}