namespace Hst.Amiga.FileSystems.Pfs3.Blocks
{
    using System.Collections.Generic;

    public class Pfs3LruCachedBlock : IEqualityComparer<Pfs3LruCachedBlock>
    {
        public Pfs3CachedBlock cblk;

        public Pfs3LruCachedBlock(Pfs3CachedBlock cblk)
        {
            this.cblk = cblk;
        }

        public bool Equals(Pfs3LruCachedBlock x, Pfs3LruCachedBlock y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return Equals(x.cblk, y.cblk);
        }

        public int GetHashCode(Pfs3LruCachedBlock obj)
        {
            return (obj.cblk != null ? obj.cblk.GetHashCode() : 0);
        }
    }
}
