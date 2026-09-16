namespace Hst.Amiga.FileSystems.Pfs3
{
    public static class Pfs3SizeOf
    {
        public const int INDEXBLOCK_T = 2 * Amiga.SizeOf.UWord + 2 * Amiga.SizeOf.ULong;
        public const int ANODEBLOCK_T = 2 * Amiga.SizeOf.UWord + 3 * Amiga.SizeOf.ULong;
        public const int ANODE_T = 3 * Amiga.SizeOf.ULong;

        public static class Pfs3RootBlockSize
        {
            public static int IdxUnion => Pfs3Constants.MAXSMALLBITMAPINDEX + 1 + Pfs3Constants.MAXSMALLINDEXNR + 1;
        }

        public static class Pfs3DirBlockSize
        {
            public static int Struct(Pfs3GlobalData g) => Amiga.SizeOf.UWord * 4 + Amiga.SizeOf.ULong * 3 + Entries(g);

            public static int Entries(Pfs3GlobalData g) =>
                g.RootBlock.ReservedBlksize - Amiga.SizeOf.UWord * 4 - Amiga.SizeOf.ULong * 3;
        }

        public static class Pfs3DelDirBlockSize
        {

            public static int Entries(Pfs3GlobalData g) =>
                (g.RootBlock.ReservedBlksize - Amiga.SizeOf.UWord * 2 - Amiga.SizeOf.ULong * 2 -
                Amiga.SizeOf.UWord * 5 -
                Amiga.SizeOf.ULong - Amiga.SizeOf.UWord * 3) / Pfs3DelDirEntrySize.Struct;
        }

        public static class Pfs3DelDirEntrySize
        {
            public const int Struct = Amiga.SizeOf.ULong * 2 + Amiga.SizeOf.UWord * 3 + 16 + Amiga.SizeOf.UWord;
        }

        public static class Pfs3DirEntrySize
        {
            public static int Struct => Amiga.SizeOf.UByte + Amiga.SizeOf.Byte + (Amiga.SizeOf.ULong * 2) +
                                        (Amiga.SizeOf.UWord * 3) + (Amiga.SizeOf.UByte * 4);
        }

        public static class Pfs3FileInfoSize
        {
            public static int Struct => Pfs3DirEntrySize.Struct;
        }

        // public static class LockEntry
        // {
        //     public static int Struct => Amiga.SizeOf.ULong * 3 + ListEntry.Struct + FileInfo.Struct;
        // }
        //
        // public static class FileEntry
        // {
        //     public static int Struct => ListEntry.Struct + Amiga.SizeOf.ULong * 4 + Amiga.SizeOf.Bool;
        // }

        public static class Pfs3ExtraFieldsSize
        {
            public static int Struct => Amiga.SizeOf.ULong + (Amiga.SizeOf.UWord * 2) +
                                        (Amiga.SizeOf.ULong * 3) + Amiga.SizeOf.UWord;
        }
    }
}
