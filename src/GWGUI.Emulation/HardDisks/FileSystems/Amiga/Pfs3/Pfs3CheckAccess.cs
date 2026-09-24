namespace Hst.Amiga.FileSystems.Pfs3
{
    using System.IO;

    public static class Pfs3CheckAccess
    {
/* fileaccess that changes a file but doesn't write to it
 */
        public static void CheckChangeAccess(Pfs3FileEntry file, Pfs3GlobalData g)
        {
            // #if DELDIR
            /* delfiles cannot be altered */
            if (Pfs3Macro.IsDelFile(file.le.info))
            {
                throw new IOException("ERROR_WRITE_PROTECTED");
            }
            // #endif

            /* test on type */
            if (!Pfs3Macro.IsFile(file.le.info))
            {
                throw new IOException("ERROR_OBJECT_WRONG_TYPE");
            }

            /* volume must be or become currentvolume */
            Pfs3VolumeOperations.CheckVolume(file.le.volume, true, g);

            /* check reserved area lock */
            if (Pfs3Macro.ReservedAreaIsLocked(g))
            {
                throw new IOException("ERROR_DISK_FULL");
            }
        }

/* fileaccess that reads from a file
 */
        public static void CheckReadAccess(Pfs3FileEntry file, Pfs3GlobalData g)
        {
            if (g.IgnoreProtectionBits)
            {
                return;
            }

            /* Test on read-protection, type and volume */
            // #if DELDIR
	        if (!Pfs3Macro.IsDelFile(file.le.info))
	        {
                // #endif
                if (!Pfs3Macro.IsFile(file.le.info))
                {
                    throw new IOException("ERROR_OBJECT_WRONG_TYPE");
                }

                if ((file.le.info.file.direntry.protection & Pfs3Constants.FIBF_READ) == Pfs3Constants.FIBF_READ)
                {
                    throw new IOException("ERROR_READ_PROTECTED");
                }
                // #if DELDIR
	        }
            // #endif

            Pfs3VolumeOperations.CheckVolume(file.le.volume, false, g);
        }

/* fileaccess that writes to a file
 */
        public static void CheckWriteAccess(Pfs3FileEntry file, Pfs3GlobalData g)
        {
            if (g.IgnoreProtectionBits)
            {
                return;
            }

            CheckChangeAccess(file, g);

            // write protected, if write bit is set
            if ((file.le.info.file.direntry.protection & Pfs3Constants.FIBF_WRITE) == Pfs3Constants.FIBF_WRITE)
            {
                throw new IOException("ERROR_WRITE_PROTECTED");
            }
        }

/* check on operate access (like Seek)
 */
        public static void CheckOperateFile(Pfs3FileEntry file, Pfs3GlobalData g)
        {
            /* test on type */
            // #if DELDIR
            if (!Pfs3Macro.IsDelFile(file.le.info) && !Pfs3Macro.IsFile(file.le.info))
            // #else
            //             if (!IsFile(file->le.info))
            // #endif
            {
                throw new IOException("ERROR_OBJECT_WRONG_TYPE");
            }

            /* volume must be or become currentvolume */
            Pfs3VolumeOperations.CheckVolume(file.le.volume, false, g);
        }
    }
}
