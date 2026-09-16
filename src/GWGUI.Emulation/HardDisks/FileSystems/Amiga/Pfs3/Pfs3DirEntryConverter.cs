namespace Hst.Amiga.FileSystems.Pfs3
{
    using Blocks;

    public static class Pfs3DirEntryConverter
    {
        public static Entry ToEntry(Pfs3DirEntry dirEntry)
        {
            return new Entry
            {
                Name = dirEntry.Name,
                Type = GetEntryType(dirEntry.type),
                Size = dirEntry.fsize,
                ProtectionBits = ProtectionBitsConverter.ToProtectionBits(dirEntry.protection),
                Date = dirEntry.CreationDate,
                Comment = dirEntry.comment,
                LinkPath = dirEntry.LinkPath
            };
        }

        public static EntryType GetEntryType(int type)
        {
            switch (type)
            {
                case Pfs3Constants.ST_USERDIR:
                    return EntryType.Dir;
                case Pfs3Constants.ST_FILE:
                    return EntryType.File;
                case Pfs3Constants.ST_SOFTLINK:
                    return EntryType.SoftLink;
                case Pfs3Constants.ST_LINKDIR:
                    return EntryType.DirLink;
                case Pfs3Constants.ST_LINKFILE:
                    return EntryType.FileLink;
                default:
                    return EntryType.File;
            }
        }
    }
}
