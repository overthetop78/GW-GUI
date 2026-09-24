namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;

    public class Pfs3FileEntry : IPfs3Entry
    {
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
        public Pfs3ListEntry le;
        public Pfs3AnodeChain anodechain; // the cached anodechain of this file
        public Pfs3AnodeChainNode currnode; // anode behorende bij offset in file
        public uint anodeoffset; // blocknr binnen currentanode
        public uint blockoffset; // byteoffset binnen huidig block
        public uint offset; // offset tov start of file
        public uint originalsize; // size of file at time of opening
        public bool checknotify; // set if a notify is necessary at ACTION_END time > ALSO touch flag <
        public Pfs3ListEntry ListEntry => le;
        public Pfs3LockEntry LockEntry => throw new NotImplementedException();

        public Pfs3FileEntry()
        {

        }
    }
}
