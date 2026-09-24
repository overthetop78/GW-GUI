namespace Hst.Amiga.FileSystems.FastFileSystem.Blocks
{
    public class FastFileSystemBootBlock
    {
        public byte[] DosType { get; set; }
        public uint RootBlockOffset { get; set; }

        public FastFileSystemBootBlock()
        {
            RootBlockOffset = 880; // floppy disk root block offset
        }
    }
}