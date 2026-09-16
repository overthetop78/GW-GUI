namespace Hst.Amiga.FileSystems.FastFileSystem.Blocks
{
    public class FastFileSystemDataBlock : IFastFileSystemBlock
    {
        public uint Offset { get; set; }
        public byte[] BlockBytes { get; set; }

        public int Type { get; }
        public uint HeaderKey { get; set; }
        public uint SeqNum { get; set; }
        public uint DataSize { get; set; }
        public uint NextData { get; set; }
        public int Checksum { get; set; }
        public byte[] Data { get; set; }

        public FastFileSystemDataBlock()
        {
            Type = FastFileSystemConstants.T_DATA;
        }
    }
}
