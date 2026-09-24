namespace Hst.Amiga.FileSystems.Pfs3.Blocks
{
    public class Pfs3BootBlock
    {
        public byte[] BlockBytes { get; set; }

        public int disktype;          /* PFS\1                            */

        public Pfs3BootBlock()
        {
            disktype = Pfs3Constants.ID_PFS_DISK;
        }
    }
}
