namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3PostponedOperation
    {
        public uint operation_id;		/* which operation is postponed */
        public uint argument1;		/* operation arguments, e.g. number of blocks */
        public uint argument2;
        public uint argument3;
    };
}