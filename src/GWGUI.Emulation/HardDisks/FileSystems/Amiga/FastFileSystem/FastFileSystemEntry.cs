namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using System;
    using System.Collections.Generic;
    using Blocks;

    public class FastFileSystemEntry
    {
        public int Type { get; set; }
        public uint Parent { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public uint Size { get; set; }
        public uint Access { get; set; }
        public DateTime Date { get; set; }
        public uint Real { get; set; }
        public uint Sector { get; set; }
        public IEnumerable<FastFileSystemEntry> SubDir { get; set; }
        public FastFileSystemEntryBlock EntryBlock { get; set; }
        public FastFileSystemEntryBlock LinkEntryBlock { get; set; }
        public string LinkPath { get; set; }
    }
}
