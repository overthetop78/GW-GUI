namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3VolumeInfo
    {
        /*
struct Pfs3VolumeInfo
{
	ULONG   root;                   // 0 =>it's a volumeinfo; <>0 => it's a fileinfo
	struct Pfs3VolumeData *volume;
};
         */

        /// <summary>
        /// Is root. 0 = root, 1 = not root
        /// pfs3aio: 0 means volumeinfo; a non-zero value means fileinfo.
        /// </summary>
        public uint root;
        public Pfs3VolumeData volume;
    }
}
