namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Blocks;
    using Exceptions;

    public partial class FastFileSystemEntryStream
    {
        private async Task AdfFileSeek(uint position)
        {
            int i;

            var nPos = Math.Min(position, length);
            this.pos = nPos;
            var extBlock = Pos2DataBlock(nPos);
            if (extBlock == uint.MaxValue)
            {
                currentData = fileHdr.DataBlocks[volume.IndexSize - 1 - curDataPtr] == 0
                    ? new FastFileSystemDataBlock
                    {
                        Data = new byte[volume.DataBlockSize]
                    }
                    : await FastFileSystemDisk.ReadDataBlock(volume, fileHdr.DataBlocks[volume.IndexSize - 1 - curDataPtr]);
            }
            else
            {
                var nSect = fileHdr.Extension;
                i = 0;
                while (i < extBlock && nSect != 0)
                {
                    currentExt = await FastFileSystemDisk.ReadFileExtBlock(volume, nSect);
                    nSect = currentExt.Extension;
                }

                if (i != extBlock)
                {
                    throw new FileSystemDiagnosticException(FileSystemErrorCode.ReadFailed,
                        FastFileSystemErrorMessages.ReadFailed);
                }

                currentData = currentExt.Index[posInExtBlk] == 0
                    ? new FastFileSystemDataBlock
                    {
                        Data = new byte[volume.DataBlockSize]
                    }
                    : await FastFileSystemDisk.ReadDataBlock(volume, currentExt.Index[posInExtBlk]);
            }
        }

        private uint Pos2DataBlock(uint position) //, int *posInExtBlk, int *posInDataBlk, int32_t *curDataN )
        {
            posInDataBlk = (uint)(position % volume.FileSystemBlockSize);
            curDataPtr = (uint)(position / volume.FileSystemBlockSize);
            if (posInDataBlk == 0)
                curDataPtr++;
            if (curDataPtr < 72)
            {
                posInExtBlk = 0;
                return uint.MaxValue;
            }

            posInExtBlk = (uint)((position - 72 * volume.FileSystemBlockSize) % volume.FileSystemBlockSize);
            var extBlock = (uint)((position - 72 * volume.FileSystemBlockSize) / volume.FileSystemBlockSize);
            if (posInExtBlk == 0)
                extBlock++;
            return extBlock;
        }

        private async Task<int> AdfReadFile(int n, byte[] buffer)
        {
            if (n > buffer.Length)
            {
                throw new FileSystemDiagnosticException(FileSystemErrorCode.CountExceedsBuffer,
                    FastFileSystemErrorMessages.CountExceedsBuffer(n, buffer.Length), n, buffer.Length);
            }

            var blockSize = volume.DataBlockSize;
            if (pos + n > fileHdr.ByteSize)
            {
                n = (int)(fileHdr.ByteSize - pos);
            }

            if (n <= 0)
            {
                return 0;
            }

            if (pos == 0 || posInDataBlk == blockSize)
            {
                await AdfReadNextFileBlock();
                posInDataBlk = 0;
            }

            var bytesRead = 0;
            var bufPtr = 0;
            while (bytesRead < n)
            {
                var size = (int)Math.Min(n - bytesRead, blockSize - posInDataBlk);

                Array.Copy(currentData.Data, posInDataBlk, buffer, bufPtr, size);
                bufPtr += size;
                pos += (uint)size;
                bytesRead += size;
                posInDataBlk += (uint)size;
                if (posInDataBlk == blockSize && bytesRead < n)
                {
                    await AdfReadNextFileBlock();
                    posInDataBlk = 0;
                }
            }

            eof = pos == fileHdr.ByteSize;
            return bytesRead;
        }

        public async Task AdfReadNextFileBlock()
        {
            uint nSect;
            var data = currentData;

            if (nDataBlock == 0)
            {
                nSect = fileHdr.FirstData;
            }
            else if (volume.UseOfs)
            {
                nSect = data.NextData;
            }
            else
            {
                if (nDataBlock < volume.IndexSize)
                    nSect = fileHdr.DataBlocks[volume.IndexSize - 1 - nDataBlock];
                else
                {
                    if (nDataBlock == volume.IndexSize)
                    {
                        currentExt = await FastFileSystemDisk.ReadFileExtBlock(volume, fileHdr.Extension);
                        posInExtBlk = 0;
                    }
                    else if (posInExtBlk == volume.IndexSize)
                    {
                        currentExt = await FastFileSystemDisk.ReadFileExtBlock(volume, currentExt.Extension);
                        posInExtBlk = 0;
                    }

                    nSect = currentExt.Index[volume.IndexSize - 1 - posInExtBlk];
                    posInExtBlk++;
                }
            }

            currentData = await FastFileSystemDisk.ReadDataBlock(volume, nSect);
            data = currentData;

            if (volume.UseOfs && data.SeqNum != nDataBlock + 1)
            {
                throw new FileSystemDiagnosticException(FileSystemErrorCode.IncorrectDataBlockSequence,
                    FastFileSystemErrorMessages.IncorrectDataBlockSequence);
            }

            nDataBlock++;
        }

    }
}
