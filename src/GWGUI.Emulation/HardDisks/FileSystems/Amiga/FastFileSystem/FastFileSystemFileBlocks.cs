namespace Hst.Amiga.FileSystems.FastFileSystem
{
    public class FastFileSystemFileBlocks
    {
        public uint header;
        public uint nbExtens;
        public uint[] extens;
        public uint nbData;
        public uint[] data;
    }
}