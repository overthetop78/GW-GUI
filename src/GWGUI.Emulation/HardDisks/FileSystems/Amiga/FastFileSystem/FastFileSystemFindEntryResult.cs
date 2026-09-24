namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using System.Collections.Generic;

    public class FastFileSystemFindEntryResult
    {
        public string Name { get; set; }
        public uint Sector { get; set; }
        public IEnumerable<FastFileSystemEntry> Entries { get; set; }
        public IEnumerable<string> PartsNotFound { get; set; }

        public FastFileSystemFindEntryResult()
        {
            Entries = new List<FastFileSystemEntry>();
            PartsNotFound = new List<string>();
        }
    }
}
