namespace Hst.Amiga.FileSystems.FastFileSystem.Extensions
{
    public static class FastFileSystemEntryExtensions
    {
        public static bool IsRoot(this FastFileSystemEntry entry) => entry.Type == FastFileSystemConstants.ST_ROOT;

        public static bool IsFile(this FastFileSystemEntry entry) => entry.Type == FastFileSystemConstants.ST_FILE;

        public static bool IsDirectory(this FastFileSystemEntry entry) => entry.Type == FastFileSystemConstants.ST_DIR;
    }
}
