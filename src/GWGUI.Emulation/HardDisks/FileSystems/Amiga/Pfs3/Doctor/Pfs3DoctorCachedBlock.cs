namespace Hst.Amiga.FileSystems.Pfs3.Doctor
{
    public class Pfs3DoctorCachedBlock
    {
        /*
typedef struct {
	uint32 blocknr;
	enum Pfs3DoctorCachedBlockMode mode;
	bitmapblock_t *data;
} c_bitmapblock_t;
        */
        public uint blocknr;
        public int mode;
        public object data;
    }
}