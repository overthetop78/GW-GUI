namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3ListEntry : IPfs3Entry
    {
        // /* de algemene structure */
        //         typedef struct Pfs3ListEntry
        //         {
        //             struct Pfs3ListEntry    *next;          /* for linkage                                      */
        //             struct Pfs3ListEntry    *prev;
        //             struct Pfs3FileLock     lock;           /* <4A> contains accesstype, dirblocknr (redundant) */
        //             listtype            type;
        //             ULONG               anodenr;        /* object anodenr. Always valid. Used by ACTION_SLEEP */
        //             ULONG               diranodenr;     /* anodenr of parent. Only valid during SLEEP_MODE. */
        //             union objectinfo    info;           /* refers to dir                                    */
        //             ULONG               dirblocknr;     /* set when block flushed and info is set to NULL   */
        //             ULONG               dirblockoffset;
        //             struct Pfs3VolumeData   *volume;        /* pointer to volume                                */
        //         } listentry_t;
        public Pfs3ListEntry next; /* for linkage                                      */
        public Pfs3ListEntry prev;
        public Pfs3FileLock filelock; /* <4A> contains accesstype, dirblocknr (redundant) */
        public Pfs3ListType type { get; set; }
        public uint anodenr; /* object anodenr. Always valid. Used by ACTION_SLEEP */
        public uint diranodenr; /* anodenr of parent. Only valid during SLEEP_MODE. */
        public Pfs3ObjectInfo info; /* refers to dir                                    */
        public uint dirblocknr; /* set when block flushed and info is set to NULL   */
        public uint dirblockoffset;
        public Pfs3VolumeData volume; /* pointer to volume                                */

        public Pfs3ListEntry ListEntry => this;

        public Pfs3LockEntry LockEntry =>
            new Pfs3LockEntry
            {
                le = this,
                nextentry = new Pfs3FileInfo
                {

                }
            };

        public Pfs3ListEntry()
        {
            filelock = new Pfs3FileLock();
        }
    }
}
