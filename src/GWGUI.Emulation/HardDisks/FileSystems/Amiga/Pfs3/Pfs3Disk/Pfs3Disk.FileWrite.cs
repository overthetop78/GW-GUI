namespace Hst.Amiga.FileSystems.Pfs3
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Blocks;
    using Core.Extensions;

    public static partial class Pfs3Disk
    {
        public static async Task<uint> WriteToFile(Pfs3FileEntry file, byte[] buffer, uint size, Pfs3GlobalData g)
        {
            var BLOCKSIZE = Pfs3Macro.BLOCKSIZE(g);
            var BLOCKSHIFT = Pfs3Macro.BLOCKSHIFT(g);
            var BLOCKSIZEMASK = Pfs3Macro.BLOCKSIZEMASK(g);
            var DIRECTSIZE = Pfs3Macro.DIRECTSIZE(g);

            bool maskok;
            uint t;
            uint totalblocks, oldblocksinfile;
            uint oldfilesize;
            uint newfileoffset;
            uint newblocksinfile;
            uint bytestowrite, blockstofill;
            uint anodeoffset, blockoffset;
            byte[] data;
            int dataptr = 0;
            Pfs3AnodeChainNode chnode;
            int slotnr;

#if DEBUG
            Pfs3Logger.Instance.Debug($"Disk: WriteToFile, size = {size}, offset = {file.offset}");
#endif
            /* initialization values */
            chnode = file.currnode;
            anodeoffset = file.anodeoffset;
            blockoffset = file.blockoffset;
            totalblocks =
                (blockoffset + size + BLOCKSIZEMASK) >> BLOCKSHIFT; /* total # changed blocks */
            if ((bytestowrite = size) == 0) /* # bytes to be done */
                return 0;

            /* filesize extend */
            oldfilesize = Pfs3Directory.GetDEFileSize(file.le.info.file.direntry, g);
            newfileoffset = file.offset + size;

            /* Check if too large (QUAD) or overflowed (ULONG)? */
            if (newfileoffset > Pfs3Constants.MAX_FILE_SIZE || newfileoffset < file.offset)
            {
                throw new Hst.Amiga.FileSystems.Exceptions.DiskFullException(
                    Pfs3ErrorMessages.DiskFull);
            }

            oldblocksinfile = (oldfilesize + BLOCKSIZEMASK) >> BLOCKSHIFT;
            newblocksinfile = (newfileoffset + BLOCKSIZEMASK) >> BLOCKSHIFT;
            if (newblocksinfile > oldblocksinfile)
            {
                t = newblocksinfile - oldblocksinfile;
                    if (!await Pfs3Allocation.AllocateBlocksAC(file.anodechain, t, file.le.info.file, g))
                {
                    file.le.info.file.direntry = Pfs3Directory.SetDEFileSize(file.le.info.file.dirblock.dirblock, file.le.info.file.direntry, oldfilesize, g);
                    throw new Hst.Amiga.FileSystems.Exceptions.DiskFullException(
                        Pfs3ErrorMessages.DiskFull);
                }
            }

            /* BUG 980422: this CorrectAnodeAC mode because of AllocateBlockAC!! AND
             * because anodeoffset can be outside last block! (filepointer is
             * byte 0 new block
             */
            Pfs3Anodes.CorrectAnodeAC(ref chnode, ref anodeoffset, g);

            /* check mask */
            maskok = ((buffer.Length - blockoffset + BLOCKSIZE) & ~g.DosEnvec.de_Mask) != 0 ||
                     ((buffer.Length - blockoffset + (totalblocks << BLOCKSHIFT)) & ~g.DosEnvec.de_Mask) != 0;
            maskok = !maskok;

            /* write indirect if
             * - mask failure
             * - too small
             */
            if (!maskok || (totalblocks < 2 * DIRECTSIZE && (blockoffset + size > BLOCKSIZE * 2) &&
                            (blockoffset != 0 || totalblocks < DIRECTSIZE)))
            {
                /* indirect */
                /* allocate temporary data buffer */
                data = new byte[totalblocks << BLOCKSHIFT];
                dataptr = 0;

                /* first blockpart */
                if (blockoffset != 0)
                {
                    var dataRead = await RawRead(1, chnode.an.blocknr + anodeoffset, g);
                    Array.Copy(dataRead, 0, data, dataptr, dataRead.Length);
                    bytestowrite += blockoffset;
                    if (bytestowrite < BLOCKSIZE)
                        bytestowrite = BLOCKSIZE; /* the first could also be the last block */
                }

                /* copy all 'to be written' to databuffer */
                Array.Copy(buffer, 0, data, dataptr + blockoffset, size);
            }
            else
            {
                /* direct */
                data = buffer;
                dataptr = 0;

                /* first blockpart */
                if (blockoffset != 0 || (totalblocks == 1 && newfileoffset > oldfilesize))
                {
                    uint fbp; /* first block part */

                    if (blockoffset != 0)
                    {
                        slotnr = await CachedRead(chnode.an.blocknr + anodeoffset, false, g);
                    }
                    else
                    {
                        /* for one block no offset growing file */
                        slotnr = await CachedRead(chnode.an.blocknr + anodeoffset, true, g);
                    }

                    /* copy data to cache and mark block as dirty */
                    var firstblock = slotnr << BLOCKSHIFT;
                    fbp = BLOCKSIZE - blockoffset;
                    fbp = Math.Min(bytestowrite, fbp); /* the first could also be the last block */
                    Array.Copy(buffer, dataptr, g.dc.data, firstblock, fbp);
                    Pfs3Macro.MarkDataDirty(slotnr, g);

                    Pfs3Anodes.NextBlockAC(ref chnode, ref anodeoffset, g);
                    bytestowrite -= fbp;
                    dataptr += (int)fbp;
                    totalblocks--;
                }
            }

            /* write following blocks. If done, then blockoffset always 0 */
            if (newfileoffset > oldfilesize)
            {
                blockstofill = totalblocks;
            }
            else
            {
                blockstofill = bytestowrite >> BLOCKSHIFT;
            }

            while (blockstofill != 0)
            {
                byte[] lastpart = null;

                if (blockstofill + anodeoffset >= chnode.an.clustersize)
                    t = chnode.an.clustersize - anodeoffset; /* t is # blocks to write now */
                else
                    t = blockstofill;

                byte[] writeptr = data;
                // last write, writing to end of file and last block won't be completely filled?
                // all this just to prevent out of bounds memory read access.
                if (t == blockstofill && (bytestowrite & BLOCKSIZEMASK) != 0 && newfileoffset > oldfilesize)
                {
                    // limit indirect to max 2 * DIRECTSIZE
                    if (t > 2 * DIRECTSIZE)
                    {
                        // > 2 * DIRECTSIZE: write only last partial block indirectly
                        t--;
                    }
                    else
                    {
                        lastpart = new byte[(int)t << BLOCKSHIFT];
                        Array.Copy(data, dataptr, lastpart, 0, bytestowrite);
                        writeptr = lastpart;
                    }
                }

                if (await RawWrite(g.stream, writeptr, t, chnode.an.blocknr + anodeoffset, g))
                {
                    blockstofill -= t;
                    dataptr += (int)(t << BLOCKSHIFT);
                    bytestowrite -= t << BLOCKSHIFT;
                    anodeoffset += t;
                    Pfs3Anodes.CorrectAnodeAC(ref chnode, ref anodeoffset, g);
                }

                if (lastpart != null)
                {
                    bytestowrite = 0;
                    lastpart = null;
                }
            }

            /* write last block (RAW because cache direct), preserve block's old contents */
            if (bytestowrite != 0)
            {
                slotnr = await CachedRead(chnode.an.blocknr + anodeoffset, false, g);
                var lastBlock = slotnr << BLOCKSHIFT;
                Array.Copy(g.dc.data, lastBlock, buffer, dataptr, bytestowrite);
                Pfs3Macro.MarkDataDirty(slotnr, g);
            }

            file.anodeoffset += (blockoffset + size) >> BLOCKSHIFT;
            file.blockoffset = (blockoffset + size) & BLOCKSIZEMASK;
            Pfs3Anodes.CorrectAnodeAC(ref file.currnode, ref file.anodeoffset, g);
            file.offset += size;
            file.le.info.file.direntry = Pfs3Directory.SetDEFileSize(file.le.info.file.dirblock.dirblock, file.le.info.file.direntry, Math.Max(oldfilesize, file.offset), g);
            await Pfs3Update.MakeBlockDirty(file.le.info.file.dirblock, g);
            return size;
        }

/* check datacache. return cache slotnr or -1
 * if not found
 */
    }
}
