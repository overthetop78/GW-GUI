namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;
    using Core.Extensions;
    using Exceptions;

    public static partial class Pfs3Disk
    {
        public static async Task<int> SeekInObject(Pfs3FileEntry file, int offset, int mode, Pfs3GlobalData g)
        {
            /* check access */
            Pfs3CheckAccess.CheckOperateFile(file, g);

            /* check anodechain, make if not there */
            if (file.anodechain == null)
            {
                if ((file.anodechain = await Pfs3Anodes.GetAnodeChain(file.le.anodenr, g)) == null)
                {
                    throw new OutOfMemoryException(Pfs3ErrorMessages.NoFreeStore);
                }
            }

	        if (Pfs3Macro.IsRollover(file.le.info))
		        return SeekInRollover(file,offset,mode,g);
	        else
                return await SeekInFile(file,offset,mode,g);
        }

        static int SeekInRollover(Pfs3FileEntry file, int offset, int mode, Pfs3GlobalData g)
        {
            var BLOCKSHIFT = Pfs3Macro.BLOCKSHIFT(g);
            var BLOCKSIZEMASK = Pfs3Macro.BLOCKSIZEMASK(g);

            var filesize_m = Pfs3Directory.GetDEFileSize(file.le.info.file.direntry, g);
            var direntry_m = file.le.info.file.direntry;

            Pfs3ExtraFields extrafields = new Pfs3ExtraFields();
            int oldvirtualoffset, virtualoffset;
            uint anodeoffset, blockoffset;

#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: SeekInRollover, offset = {offset}, mode = {mode}");
#endif
            extrafields = direntry_m.GetExtraFields();

            /* do the seeking */
            oldvirtualoffset = (int)(file.offset - extrafields.rollpointer);
            if (oldvirtualoffset < 0)
            {
                oldvirtualoffset += (int)filesize_m;
            }

            switch (mode)
            {
                case Pfs3Constants.OFFSET_BEGINNING:
                    virtualoffset = offset;
                    break;

                case Pfs3Constants.OFFSET_END:
                    virtualoffset = (int)(extrafields.virtualsize + offset);
                    break;

                case Pfs3Constants.OFFSET_CURRENT:
                    virtualoffset = oldvirtualoffset + offset;
                    break;

                default:
                    throw new FileSystemDiagnosticException(FileSystemErrorCode.SeekError,
                        Pfs3ErrorMessages.SeekError);
            }

            if (virtualoffset > extrafields.virtualsize || virtualoffset < 0)
            {
                throw new FileSystemDiagnosticException(FileSystemErrorCode.SeekError,
                    Pfs3ErrorMessages.SeekError);
            }

            /* calculate real offset */
            file.offset = (uint)(virtualoffset + extrafields.rollpointer);
            if (file.offset > filesize_m)
            {
                file.offset -= filesize_m;
            }

            /* calculate new values */
            anodeoffset = file.offset >> BLOCKSHIFT;
            blockoffset = file.offset & BLOCKSIZEMASK;
            file.currnode = file.anodechain.head;
            Pfs3Anodes.CorrectAnodeAC(ref file.currnode, ref anodeoffset, g);

            file.anodeoffset  = anodeoffset;
            file.blockoffset  = blockoffset;

            return oldvirtualoffset;

        }
    }
}
