namespace Hst.Amiga.FileSystems.Pfs3.Doctor
{
    public class Pfs3DoctorCacheLine
    {
        // struct Pfs3DoctorCacheLine
        // {
        //     struct Pfs3DoctorCacheLine *next;
        //     struct Pfs3DoctorCacheLine *prev;
        //     uint32 blocknr;			/* 1 == unused */
        //     bool dirty;
        //     uint8 *data;
        // };

        // struct Pfs3DoctorCacheLine *next;
        // struct Pfs3DoctorCacheLine *prev;
        public uint blocknr;			/* 1 == unused */
        public bool dirty;
        public byte[] data;
        // };
    }
}
