namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3DelFileInfo
    {
        public uint special;					// 2
        public uint slotnr;					// het slotnr voor deze deldirentry

        public Pfs3DelFileInfo()
        {
            special = 3; // file by default
        }
    }
}