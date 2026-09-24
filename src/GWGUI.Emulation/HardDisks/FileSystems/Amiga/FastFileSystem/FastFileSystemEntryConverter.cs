namespace Hst.Amiga.FileSystems.FastFileSystem
{
    public static class FastFileSystemEntryConverter
    {
        public static Hst.Amiga.FileSystems.Entry ToEntry(FastFileSystemEntry entry)
        {
            return new Hst.Amiga.FileSystems.Entry
            {
                Name = entry.Name,
                Type = GetEntryType(entry.Type),
                Size = entry.Type == FastFileSystemConstants.ST_LFILE && entry.LinkEntryBlock != null
                    ? entry.LinkEntryBlock.ByteSize
                    : entry.Size,
                ProtectionBits = ProtectionBitsConverter.ToProtectionBits((int)entry.Access),
                Date = entry.Date,
                Comment = entry.Comment,
                LinkPath = entry.LinkPath
            };
        }

        public static EntryType GetEntryType(int type)
        {
            switch (type)
            {
                case FastFileSystemConstants.ST_DIR:
                    return EntryType.Dir;
                case FastFileSystemConstants.ST_FILE:
                    return EntryType.File;
                case FastFileSystemConstants.ST_LSOFT:
                    return EntryType.SoftLink;
                case FastFileSystemConstants.ST_LDIR:
                    return EntryType.DirLink;
                case FastFileSystemConstants.ST_LFILE:
                    return EntryType.FileLink;
                default:
                    return EntryType.File;
            }
        }
    }
}
