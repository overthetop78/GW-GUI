namespace Hst.Amiga.FileSystems.FastFileSystem
{
    public static class FastFileSystemMacro
    {
        public static bool UseFfs(int c)
        {
            if (c <= 5)
            {
                return (c & FastFileSystemConstants.FSMASK_FFS) != 0;
            }

            return c == 7; // DOS\7
        }

        public static bool UseOfs(int c)
        {
            if (c <= 5)
            {
                return (c & FastFileSystemConstants.FSMASK_FFS) == 0;
            }

            return c == 6; // DOS\6
        }

        public static bool UseDirCache(int c)
        {
            return c < 6 && (c & FastFileSystemConstants.FSMASK_DIRCACHE) != 0;
        }

        public static bool UseLnfs(int c)
        {
            return c >= 6;
        }

        public static bool UseIntl(int c)
        {
            if (c <= 5)
            {
                return (c & FastFileSystemConstants.FSMASK_INTL) != 0;
            }

            return true; // DOS\6 + DOS\7 (LNFS) always uses the "international" directory entry name hashing operation.
        }

        public static bool hasD(uint c) => (c & FastFileSystemConstants.ACCMASK_D) != 0;
        public static bool hasE(uint c) => (c & FastFileSystemConstants.ACCMASK_E) != 0;
        public static bool hasW(uint c) => (c&FastFileSystemConstants.ACCMASK_W) != 0;

        /// <summary>
        /// check if entry access has read flag set
        /// </summary>
        /// <param name="c"></param>
        /// <returns>True if read flag is not set</returns>
        public static bool hasR(uint c) => (c&FastFileSystemConstants.ACCMASK_R) != 0;
        public static bool hasA(uint c) => (c&FastFileSystemConstants.ACCMASK_A) != 0;
        public static bool hasP(uint c) => (c&FastFileSystemConstants.ACCMASK_P) != 0;
        public static bool hasS(uint c) => (c&FastFileSystemConstants.ACCMASK_S) != 0;
        public static bool hasH(uint c) => (c&FastFileSystemConstants.ACCMASK_H) != 0;
    }
}
