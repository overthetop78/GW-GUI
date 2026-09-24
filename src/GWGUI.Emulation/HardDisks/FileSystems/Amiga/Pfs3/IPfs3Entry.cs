namespace Hst.Amiga.FileSystems.Pfs3
{
    public interface IPfs3Entry
    {
        Pfs3ListEntry ListEntry { get; }
        Pfs3LockEntry LockEntry { get; }

        // //#define fe ((fileentry_t *)listentry)

        // IEntry - how to convert between these?
        // - listentry
        // - lockentry
        // - fileentry

        /*
typedef struct Pfs3ListEntry
{
	struct Pfs3ListEntry    *next;          // for linkage
        struct Pfs3ListEntry    *prev;
        struct Pfs3FileLock     lock;           // <4A> contains accesstype, dirblocknr (redundant)
        listtype            type;
        ULONG               anodenr;        // object anodenr. Always valid. Used by ACTION_SLEEP
        ULONG               diranodenr;     // anodenr of parent. Only valid during SLEEP_MODE.
        union objectinfo    info;           // refers to dir
        ULONG               dirblocknr;     // set when block flushed and info is set to NULL
        ULONG               dirblockoffset;
        struct Pfs3VolumeData   *volume;        // pointer to volume
    } listentry_t;
    */

        // typedef struct Pfs3LockEntry
        // {
        //     listentry_t le;
        //
        //     ULONG               nextanode;          // anodenr of next entry (dir/vollock only)
        //     struct Pfs3FileInfo     nextentry;          // for examine
        //     ULONG               nextdirblocknr;     // for flushed block only.. (dir/vollock only)
        //     ULONG               nextdirblockoffset;
        // } lockentry_t;

/*
 * // de specifieke structuren
        typedef struct
        {
            listentry_t le;

            struct Pfs3AnodeChain *anodechain;      // the cached anodechain of this file
            struct Pfs3AnodeChainNode *currnode;    // anode behorende bij offset in file
            ULONG   anodeoffset;        // blocknr binnen currentanode
            ULONG   blockoffset;        // byteoffset binnen huidig block
            FSIZE   offset;             // offset tov start of file
            FSIZE   originalsize;       // size of file at time of opening
            BOOL    checknotify;        // set if a notify is necessary at ACTION_END time > ALSO touch flag <
        } fileentry_t;
 */

    }
}
