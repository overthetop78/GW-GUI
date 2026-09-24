namespace Hst.Amiga.FileSystems.Pfs3
{
    using Blocks;

    public static class Pfs3Cache
    {
        public static void LOCK(Pfs3CachedBlock blk, Pfs3GlobalData g) => blk.used = g.locknr;
        public static void UNLOCKALL(Pfs3GlobalData g) => g.locknr++;
        public static bool ISLOCKED(Pfs3CachedBlock blk, Pfs3GlobalData g) => blk.used == g.locknr;

        public static void ClearSearchInDirCache(uint dirnodenr, Pfs3GlobalData g)
        {
            if (!g.SearchInDirCache.ContainsKey(dirnodenr))
            {
                return;
            }

            g.SearchInDirCache.Remove(dirnodenr);
        }
    }
}
