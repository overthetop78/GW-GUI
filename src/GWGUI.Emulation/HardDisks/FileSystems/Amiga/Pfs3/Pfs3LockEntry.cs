namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3LockEntry : IPfs3Entry
    {
        // typedef struct Pfs3LockEntry
        // {
        //     listentry_t le;
        //
        //     ULONG               nextanode;          // anodenr of next entry (dir/vollock only)
        //     struct Pfs3FileInfo     nextentry;          // for examine
        //     ULONG               nextdirblocknr;     // for flushed block only.. (dir/vollock only)
        //     ULONG               nextdirblockoffset;
        // } lockentry_t;
        public Pfs3ListEntry le;

        public uint nextanode; // anodenr of next entry (dir/vollock only)
        public Pfs3FileInfo nextentry; // for examine
        public uint nextdirblocknr; // for flushed block only.. (dir/vollock only)
        public uint nextdirblockoffset;

        public Pfs3ListType type => le.type;
        public Pfs3ListEntry ListEntry => le;

        public Pfs3LockEntry LockEntry => this;
    }
}
