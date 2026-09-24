namespace Hst.Amiga.FileSystems.Pfs3.Blocks
{
    public class Pfs3RootBlockIndex
    {
        public class Pfs3RootBlockIndexSmall
        {
            /*
            struct
            {
                ULONG bitmapindex[Constants.MAXSMALLBITMAPINDEX + 1];       // 5 bitmap indexblocks with 253 bitmap blocks each
                ULONG indexblocks[Constants.MAXSMALLINDEXNR + 1];      // 99 index blocks with 253 (more if reserved blocks > 1K) anode blocks each
            } small;
             */
            // public uint[] bitmapindex;
            // public uint[] indexblocks;
            public Pfs3RootBlockIndexUnion<uint> bitmapindex;
            public Pfs3RootBlockIndexUnion<uint> indexblocks;

            public Pfs3RootBlockIndexSmall(uint[] union)
            {
                // bitmapindex = new uint[Constants.MAXSMALLBITMAPINDEX + 1];
                // indexblocks = new uint[Constants.MAXSMALLINDEXNR + 1];
                bitmapindex = new Pfs3RootBlockIndexUnion<uint>(union, 0);
                indexblocks = new Pfs3RootBlockIndexUnion<uint>(union, Pfs3Constants.MAXSMALLBITMAPINDEX + 1);
            }
        }

        /// <summary>
        /// union to share array between offset usage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class Pfs3RootBlockIndexUnion<T>
        {
            private readonly T[] array;
            private readonly int offset;

            public Pfs3RootBlockIndexUnion(T[] array, int offset)
            {
                this.array = array;
                this.offset = offset;
            }

            public T this[uint index]
            {
                get => array[offset + index];
                set => array[offset + index] = value;
            }
        }

        public class Pfs3RootBlockIndexLarge
        {
            /*
            struct
            {
                ULONG bitmapindex[Constants.MAXBITMAPINDEX + 1];		// 104 bitmap indexblocks = max 104 G
            } large;
             */
            // public uint[] bitmapindex;
            public Pfs3RootBlockIndexUnion<uint> bitmapindex;

            public Pfs3RootBlockIndexLarge(uint[] union)
            {
                // bitmapindex = new uint[Constants.MAXSMALLINDEXNR + 1];
                bitmapindex = new Pfs3RootBlockIndexUnion<uint>(union, 0);
            }
        }

        public Pfs3RootBlockIndexSmall small;
        public Pfs3RootBlockIndexLarge large;

        /// <summary>
        /// union of uint (ULONG) for small and large structs sharing same memory area
        /// </summary>
        public uint[] union;

        public Pfs3RootBlockIndex(uint[] idxUnion)
        {
            this.union = idxUnion;
            small = new Pfs3RootBlockIndexSmall(idxUnion);
            large = new Pfs3RootBlockIndexLarge(idxUnion);
        }

        public Pfs3RootBlockIndex() : this(new uint[Pfs3Constants.MAXSMALLBITMAPINDEX + 1 + Pfs3Constants.MAXSMALLINDEXNR + 1])
        {
        }
    }
}
