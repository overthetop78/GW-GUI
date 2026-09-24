namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;
    using Core.Extensions;

    public static partial class Pfs3Disk
    {
        public static async Task<uint> ReadFromRollover(Pfs3FileEntry file, byte[] buffer, uint size, Pfs3GlobalData g)
        {
            var direntry_m = file.le.info.file.direntry;
            var filesize_m = Pfs3Directory.GetDEFileSize(file.le.info.file.direntry, g);

            uint read = 0;
            int q; // quantity
            int end, virtualoffset, virtualend, t;

#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: ReadFromRollover, size = {size}, offset = {file.offset}");
#endif
            if (size == 0)
            {
                return 0;
            }

            var extrafields = direntry_m.GetExtraFields();

            /* limit access to end of file */
            virtualoffset = (int)(file.offset - extrafields.rollpointer);
            if (virtualoffset < 0)
            {
                virtualoffset += (int)filesize_m;
            }

            virtualend = (int)(virtualoffset + size);
            virtualend = (int)Math.Min(virtualend, extrafields.virtualsize);
            end = (int)(virtualend - virtualoffset + file.offset);

            int bufferPos = 0;
            byte[] bufferRead;

            if (end > filesize_m)
            {
                q = (int)(filesize_m - file.offset);
                bufferRead = new byte[q];
                if ((read = await ReadFromFile(file, bufferRead, (uint)q, g)) != q)
                {
                    return read;
                }

                end -= (int)filesize_m;
                Array.Copy(bufferRead, 0, buffer, bufferPos, q);
                bufferPos += q;
                await SeekInFile(file, 0, Pfs3Constants.OFFSET_BEGINNING, g);
            }

            q = (int)(end - file.offset);
            bufferRead = new byte[q];
            t = (int)await ReadFromFile(file, bufferRead, (uint)q, g);
            Array.Copy(bufferRead, 0, buffer, bufferPos, q);
            if (t == -1)
                return (uint)t;
            else
                read += (uint)t;

            return read;

        }

/* Write to rollover file. First write upto end of rollover. Then
 * flip to start.
 * Max virtualsize = filesize-1
 */
        public static async Task<uint> WriteToRollover(Pfs3FileEntry file, byte[] buffer, uint size, Pfs3GlobalData g)
        {
            var direntry_m = file.le.info.file.direntry;
            var filesize_m = Pfs3Directory.GetDEFileSize(file.le.info.file.direntry, g);

            Pfs3ExtraFields extrafields;
            Pfs3DirEntry destentry;
            Pfs3ObjectInfo directory = new Pfs3ObjectInfo();
            Pfs3FileInfo fi = new Pfs3FileInfo();
            int written = 0;
            int q; // quantity
            int end, virtualend, virtualoffset, t;
            bool extend = false;

#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: WriteToRollover, size = {size}, offset = {file.offset}");
#endif
            extrafields = direntry_m.GetExtraFields();
            end = (int)(file.offset + size);

            /* new virtual size */
            virtualoffset = (int)(file.offset - extrafields.rollpointer);
            if (virtualoffset < 0)
            {
                virtualoffset += (int)filesize_m;
            }

            virtualend = (int)(virtualoffset + size);
            if (virtualend >= extrafields.virtualsize)
            {
                extrafields.SetVirtualSize((uint)Math.Min(filesize_m - 1, virtualend));
                extend = true;
            }

            byte[] writeBuffer;
            int bufferPos = 0;
            while (end > filesize_m)
            {
                q = (int)(filesize_m - file.offset);
                writeBuffer = new byte[q];
                Array.Copy(buffer, bufferPos, writeBuffer, 0, q);
                t = (int)await WriteToFile(file, writeBuffer, (uint)q, g);
                if (t == -1) return (uint)t;
                written += t;
                if (t != q) return (uint)written;
                end -= (int)filesize_m;
                bufferPos += q;
                await SeekInFile(file, 0, Pfs3Constants.OFFSET_BEGINNING, g);
            }

            q = (int)(end - file.offset);
            writeBuffer = new byte[q];
            Array.Copy(buffer, bufferPos, writeBuffer, 0, q);
            t = (int)await WriteToFile(file, buffer, (uint)q, g);
            if (t == -1)
                return (uint)t;
            else
                written += t;

            /* change rollpointer etc */
            if (extend && extrafields.virtualsize == filesize_m - 1)
            {
                extrafields.SetRollPointer((uint)(end + 1)); /* byte PAST eof is offset 0 */
            }
            destentry = new Pfs3DirEntry(direntry_m, g);
            destentry.SetExtraFields(extrafields, g);

            /* commit changes */
            if (!await Pfs3Directory.GetParent(file.le.info, directory, g))
            {
                return 0;
            }
            else
            {
                await Pfs3Directory.ChangeDirEntry(file.le.info, destentry, directory, fi, g);
            }

            return (uint)written;

        }

    }
}
