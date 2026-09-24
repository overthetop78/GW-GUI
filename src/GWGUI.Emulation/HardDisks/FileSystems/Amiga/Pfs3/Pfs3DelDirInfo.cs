namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3DelDirInfo
    {
        public uint special;                  // 0 => volumeinfo; 1 => deldirinfo; 2 => delfile; >2 => fileinfo
        public Pfs3VolumeData volume;

        public Pfs3DelDirInfo()
        {
            special = 3; // file by default
        }
    }
}
