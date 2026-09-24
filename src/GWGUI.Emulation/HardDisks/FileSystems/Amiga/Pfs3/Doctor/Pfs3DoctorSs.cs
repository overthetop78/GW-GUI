namespace Hst.Amiga.FileSystems.Pfs3.Doctor
{
    public class Pfs3DoctorSs
    {
        /*
        struct
        {
            // state flags
            uint32 flags;			// internal flags
            uint32 opties;			// options
            int pass;
            BOOL verbose;
            BOOL unformat;

            enum {syntax, resbitmap, mainbitmap, anodebitmap, finished} stage;

            struct Pfs3DoctorMinList *doubles;
        } ss;
*/
        public bool verbose;
        public bool unformat;
    }
}