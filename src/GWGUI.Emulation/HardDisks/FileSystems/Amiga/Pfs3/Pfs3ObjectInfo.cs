namespace Hst.Amiga.FileSystems.Pfs3
{
    public class Pfs3ObjectInfo
    {
        /*
union objectinfo
{
	struct Pfs3FileInfo file;
	struct Pfs3VolumeInfo volume;
#if DELDIR
	struct Pfs3DelDirInfo deldir;
	struct Pfs3DelFileInfo delfile;
#endif
};
         */

        public Pfs3FileInfo file;
        public Pfs3VolumeInfo volume;

        // deldir
        public Pfs3DelDirInfo deldir;
        public Pfs3DelFileInfo delfile;

        public Pfs3ObjectInfo()
        {
	        file = new Pfs3FileInfo();
	        volume = new Pfs3VolumeInfo();
	        deldir = new Pfs3DelDirInfo();
	        delfile = new Pfs3DelFileInfo();
        }

        public void OverwriteWith(Pfs3ObjectInfo other)
		{
	        file = other.file;
	        volume = other.volume;
	        deldir = other.deldir;
	        delfile = other.delfile;
		}

        public Pfs3ObjectInfo Clone()
        {
	        return new Pfs3ObjectInfo
	        {
		        file = new Pfs3FileInfo
		        {
			        dirblock = file.dirblock,
			        direntry = file.direntry?.Clone()
		        },
		        volume = new Pfs3VolumeInfo
		        {
			        root = volume.root,
			        volume = volume.volume
		        },
		        deldir = new Pfs3DelDirInfo
		        {
			        special = deldir.special,
			        volume = deldir.volume
		        },
		        delfile = new Pfs3DelFileInfo
		        {
			        slotnr = delfile.slotnr,
			        special = delfile.special
		        }
	        };
        }
    }
}
