namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using Blocks;

    public class FastFileSystemNameToEntryBlockResult
    {
        public uint NSect { get; set; }
        public FastFileSystemEntryBlock EntryBlock { get; set; }
        public uint? NUpdSect { get; set; }
    }
}
