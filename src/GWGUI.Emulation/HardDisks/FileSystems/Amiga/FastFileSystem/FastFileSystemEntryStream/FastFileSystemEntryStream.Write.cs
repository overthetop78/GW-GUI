namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Blocks;
    using Hst.Amiga.FileSystems.Exceptions;

    public partial class FastFileSystemEntryStream
    {
        public async Task<int> AdfWriteFile(int n, byte[] buffer)
        {
            if (n == 0)
            {
                return n;
            }

            var blockSize = volume.DataBlockSize;
            var dataPtr = currentData.Data;

            if (pos == 0 || posInDataBlk == blockSize)
            {
                if (await AdfCreateNextFileBlock() == uint.MaxValue)
                {
                    /* bug found by Rikard */
                throw new FileSystemDiagnosticException(FileSystemErrorCode.NoFreeSectorAvailable,
                    FastFileSystemErrorMessages.NoFreeSectorAvailable);
                }

                posInDataBlk = 0;
            }

            var bytesWritten = 0;
            var bufPtr = 0;
            while (bytesWritten < n)
            {
                var size = (int)Math.Min(n - bytesWritten, blockSize - posInDataBlk);

                Array.Copy(buffer, bufPtr, dataPtr, posInDataBlk, size);

                bufPtr += size;
                pos += (uint)size;
                bytesWritten += size;
                posInDataBlk += (uint)size;
                if (posInDataBlk == blockSize && bytesWritten < n)
                {
                    if (await AdfCreateNextFileBlock() == uint.MaxValue)
                    {
                        /* bug found by Rikard */
                    throw new FileSystemDiagnosticException(FileSystemErrorCode.NoFreeSectorAvailable,
                        FastFileSystemErrorMessages.NoFreeSectorAvailable);
                    }

                    posInDataBlk = 0;
                }
            }

            return bytesWritten;
        }

        public async Task<uint> AdfCreateNextFileBlock()
        {
            uint nSect;
            var blockSize = volume.DataBlockSize;

            /* the first data blocks pointers are inside the file header block */
            if (nDataBlock < volume.IndexSize)
            {
                nSect = FastFileSystemBitmap.AdfGet1FreeBlock(volume);
                if (nSect == uint.MaxValue)
                {
                    return uint.MaxValue;
                }

                if (nDataBlock == 0)
                {
                    fileHdr.FirstData = nSect;
                }

                fileHdr.DataBlocks[volume.IndexSize - 1 - nDataBlock] = nSect;
                fileHdr.HighSeq++;
            }
            else
            {
                /* one more sector is needed for one file extension block */
                if (nDataBlock % volume.IndexSize == 0)
                {
                    var extSect = FastFileSystemBitmap.AdfGet1FreeBlock(volume);
                    if (extSect == uint.MaxValue)
                    {
                        return uint.MaxValue;
                    }

                    /* the future block is the first file extension block */
                    if (nDataBlock == volume.IndexSize)
                    {
                        currentExt = new FastFileSystemFileExtBlock(volume.FileSystemBlockSize);
                        fileHdr.Extension = extSect;
                    }

                    /* not the first : save the current one, and link it with the future */
                    if (nDataBlock >= 2 * volume.IndexSize)
                    {
                        currentExt.Extension = extSect;
                        await FastFileSystemDisk.WriteFileExtBlock(volume, currentExt.HeaderKey, currentExt);
                    }

                    /* initializes a file extension block */
                    for (var i = 0; i < volume.IndexSize; i++)
                        currentExt.Index[i] = 0;
                    currentExt.HeaderKey = extSect;
                    currentExt.Parent = fileHdr.HeaderKey;
                    currentExt.HighSeq = 0;
                    currentExt.Extension = 0;
                    posInExtBlk = 0;
                }

                nSect = FastFileSystemBitmap.AdfGet1FreeBlock(volume);
                if (nSect == uint.MaxValue)
                {
                    return uint.MaxValue;
                }

                currentExt.Index[volume.IndexSize - 1 - posInExtBlk] = nSect;
                currentExt.HighSeq++;
                posInExtBlk++;
            }

            /* builds OFS header */
            if (volume.UseOfs)
            {
                var data = currentData;
                /* writes previous data block and link it  */
                if (pos >= blockSize)
                {
                    data.NextData = nSect;
                    await FastFileSystemDisk.WriteDataBlock(volume, curDataPtr, currentData);
                }

                /* initialize a new data block */
                for (var i = 0; i < blockSize; i++)
                    data.Data[i] = 0;
                data.SeqNum = nDataBlock + 1;
                data.DataSize = blockSize;
                data.NextData = 0;
                data.HeaderKey = fileHdr.HeaderKey;
            }
            else if (pos >= blockSize)
            {
                await FastFileSystemDisk.WriteDataBlock(volume, curDataPtr, currentData);
            }

            curDataPtr = nSect;
            nDataBlock++;

            return nSect;
        }

        public async Task AdfCloseFile()
        {
            await AdfFlushFile();
        }

        public async Task AdfFlushFile()
        {
            if (currentExt != null)
            {
                if (writeMode)
                {
                    await FastFileSystemDisk.WriteFileExtBlock(volume, currentExt.HeaderKey, currentExt);
                }
            }

            if (currentData != null)
            {
                if (writeMode)
                {
                    fileHdr.ByteSize = pos;
                    if (volume.UseOfs)
                    {
                        currentData.DataSize = posInDataBlk;
                    }

                    if (fileHdr.ByteSize > 0)
                    {
                        await FastFileSystemDisk.WriteDataBlock(volume, curDataPtr, currentData);
                    }
                }
            }

            if (writeMode)
            {
                fileHdr.ByteSize = pos;
                fileHdr.Date = DateTime.Now;
                await FastFileSystemDisk.WriteEntryBlock(volume, fileHdr.HeaderKey, fileHdr);

                if (volume.UseDirCache)
                {
                    var parent = await FastFileSystemDisk.ReadEntryBlock(volume, fileHdr.Parent);
                    await FastFileSystemCache.UpdateCache(volume, parent, fileHdr, true);
                }

                await FastFileSystemBitmap.AdfUpdateBitmap(volume);
            }
        }
    }
}
