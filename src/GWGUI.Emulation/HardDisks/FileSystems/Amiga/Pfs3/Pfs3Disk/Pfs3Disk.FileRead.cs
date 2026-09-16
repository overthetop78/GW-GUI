namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;
    using Core.Extensions;

    public static partial class Pfs3Disk
    {
        public static async Task<int> SeekInFile(Pfs3FileEntry file, int offset, int mode, Pfs3GlobalData g)
        {
            int oldoffset, newoffset;
            uint anodeoffset, blockoffset;
            Pfs3DelDirEntry delfile = null;
#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: SeekInFile, offset = {offset}, mode = {mode}");
#endif
            if (Pfs3Macro.IsDelFile(file.le.info))
            {
                if ((delfile = await Pfs3Directory.GetDeldirEntryQuick(file.le.info.delfile.slotnr, g)) == null)
                    return -1;
            }
            /* do the seeking */
            oldoffset = (int)file.offset;
            newoffset = -1;

            /* TODO: 32-bit wraparound checks */

            switch (mode)
            {
                case Pfs3Constants.OFFSET_BEGINNING:
                    newoffset = offset;
                    break;

                case Pfs3Constants.OFFSET_END:
                    if (delfile != null)
                        newoffset = (int)(Pfs3Directory.GetDDFileSize(delfile, g) + offset);
                    else
                        newoffset = (int)(Pfs3Directory.GetDEFileSize(file.le.info.file.direntry, g) + offset);
                    break;

                case Pfs3Constants.OFFSET_CURRENT:
                    newoffset = oldoffset + offset;
                    break;

                default:
                    return -1;
            }

            if ((newoffset > (delfile != null
                    ? Pfs3Directory.GetDDFileSize(delfile, g)
                    : Pfs3Directory.GetDEFileSize(file.le.info.file.direntry, g))) || (newoffset < 0))
            {
                return -1;
            }

            /* calculate new values */
            anodeoffset = (uint)(newoffset >> g.blockshift);
            blockoffset = (uint)(newoffset & Pfs3Macro.BLOCKSIZEMASK(g));
            file.currnode = file.anodechain.head;
            Pfs3Anodes.CorrectAnodeAC(ref file.currnode, ref anodeoffset, g);
            /* DiskSeek(anode.blocknr + anodeoffset, g); */

            file.anodeoffset = anodeoffset;
            file.blockoffset = blockoffset;
            file.offset = (uint)newoffset;
            return newoffset;
        }

/* flush all blocks in datacache (without updating them first).
 */
        public static void FlushDataCache(Pfs3GlobalData g)
        {
            for (var i = 0; i < g.dc.size; i++)
            {
                g.dc.ref_[i].blocknr = 0;
            }
        }

/* <ReadFromFile>
**
** Specification:
**
** Reads 'size' bytes from file to buffer (if not readprotected)
** result: #bytes read; -1 = error; 0 = eof
*/
        public static async Task<uint> ReadFromFile(Pfs3FileEntry file, byte[] buffer, uint size, Pfs3GlobalData g)
        {
            var BLOCKSIZE = Pfs3Macro.BLOCKSIZE(g);
            var BLOCKSHIFT = Pfs3Macro.BLOCKSHIFT(g);
            var BLOCKSIZEMASK = Pfs3Macro.BLOCKSIZEMASK(g);
            var DIRECTSIZE = Pfs3Macro.DIRECTSIZE(g);

            uint anodeoffset, blockoffset, blockstoread;
            uint fullblks, bytesleft;
            uint t;
            uint tfs;
            byte[] data = new byte[0];
            int dataptr = 0;
            bool directread = false;
            Pfs3AnodeChainNode chnode;
            Pfs3DelDirEntry dde;

#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: ReadFromFile, size = {size}, offset = {file.offset}");
#endif
            Pfs3CheckAccess.CheckReadAccess(file, g);

            /* correct size and check if zero */
            if (Pfs3Macro.IsDelFile(file.le.info))
            {
                if ((dde = await Pfs3Directory.GetDeldirEntryQuick(file.le.info.delfile.slotnr, g)) == null)
                {
                    return UInt32.MaxValue;
                }

                tfs = Pfs3Directory.GetDDFileSize(dde, g) - file.offset;
            }
            else
            {
                tfs = Pfs3Directory.GetDEFileSize(file.le.info.file.direntry, g) - file.offset;
            }

            if ((size = Math.Min(tfs, size)) == 0)
            {
                return 0;
            }

            /* initialize */
            anodeoffset = file.anodeoffset;
            blockoffset = file.blockoffset;
            chnode = file.currnode;
            t = blockoffset + size;
            fullblks = t >> BLOCKSHIFT; /* # full blocks */
            bytesleft = t & BLOCKSIZEMASK; /* # bytes in last incomplete block */

            /* check mask, both at start and end */
            t = ((buffer.Length - blockoffset + BLOCKSIZE) & ~g.DosEnvec.de_Mask) != 0 ||
                ((buffer.Length + size - bytesleft) & ~g.DosEnvec.de_Mask) != 0
                ? 1U
                : 0U;
            t = t != 0U ? 0U : 1U;

            /* read indirect if
             * - mask failure
             * - too small
             * - larger than one block (use 'direct' cached read for just one)
             */
            if (t == 0 || (fullblks < 2 * DIRECTSIZE && (blockoffset + size > BLOCKSIZE) &&
                           (blockoffset != 0 || (bytesleft != 0 && fullblks < DIRECTSIZE))))
            {
                /* full indirect read */
                blockstoread = (uint)(fullblks + (bytesleft > 0 ? 1 : 0));
                data = new byte[blockstoread << BLOCKSHIFT];
                dataptr = 0;
            }
            else
            {
                /* direct read */
                directread = true;
                blockstoread = fullblks;
                data = buffer;
                dataptr = 0;

                /* read first blockpart */
                if (blockoffset != 0)
                {
                    var bytesRead = await CachedReadD(chnode.an.blocknr + anodeoffset, g);
                    if (bytesRead.Length > 0)
                    {
                        Pfs3Anodes.NextBlockAC(ref chnode, ref anodeoffset, g);

                        /* calc numbytes */
                        t = BLOCKSIZE - blockoffset;
                        t = Math.Min(t, size);
                        Array.Copy(bytesRead, blockoffset, data, dataptr, t);
                        dataptr += (int)t;
                        if (blockstoread != 0)
                            blockstoread--;
                        else
                            bytesleft = 0; /* single block access */
                    }
                }
            }

            /* read middle part */
            while (blockstoread != 0)
            {
                if ((blockstoread + anodeoffset) >= chnode.an.clustersize)
                    t = chnode.an.clustersize - anodeoffset; /* read length */
                else
                    t = blockstoread;

                var bytesRead = await RawRead(t, chnode.an.blocknr + anodeoffset, g);
                Array.Copy(bytesRead, 0, data, dataptr, Math.Min(bytesRead.Length, data.Length));
                blockstoread -= t;
                dataptr += (int)(t << BLOCKSHIFT);
                anodeoffset += t;
                Pfs3Anodes.CorrectAnodeAC(ref chnode, ref anodeoffset, g);
            }

            /* read last block part/ copy read data to buffer */
            if (!directread)
            {
                Array.Copy(data, blockoffset, buffer, 0, size);
            }
            else if (bytesleft > 0)
            {
                var dataRead = await CachedReadD(chnode.an.blocknr + anodeoffset, g);
                if (dataRead.Length > 0)
                {
                    Array.Copy(dataRead, 0, buffer, dataptr, bytesleft);
                }
            }

            file.anodeoffset += fullblks;
            file.blockoffset = (file.blockoffset + size) & BLOCKSIZEMASK; // not bytesleft!!
            Pfs3Anodes.CorrectAnodeAC(ref file.currnode, ref file.anodeoffset, g);
            file.offset += size;
            return size;
        }

/* <WriteToFile>
**
** Specification:
**
** - Copy data in file at current position;
** - Automatic fileextension;
** - Error = bytecount <> opdracht
** - On error no position update
**
** - Clear Archivebit -> done by Touch()
**V- directory protection (amigados does not do this)
**
** result: num bytes written; DOPUS wants -1 = error;
**
** Implementation parts
**
** - Test on writeprotection; yes -> error;
** - Initialisation
** - Extend filesize
** - Write firstblockpart
** - Write all whole blocks
** - Write last block
** - | Pfs3Update directory (if no errors)
**   | Deextent filesize (if error)
*/
    }
}
