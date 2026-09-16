namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Blocks;
    using Exceptions;

    public partial class FastFileSystemEntryStream : Stream
    {
        private readonly FastFileSystemVolumeState volume;

        private readonly uint length;

        private bool eof;
        private readonly FastFileSystemEntryBlock fileHdr;

        private uint pos;
        private uint posInExtBlk;
        private uint posInDataBlk;
        private uint curDataPtr;
        private FastFileSystemFileExtBlock currentExt;
        private uint nDataBlock;
        private FastFileSystemDataBlock currentData;
        private readonly bool writeMode;

        public FastFileSystemEntryStream(FastFileSystemVolumeState volume, bool writeMode, bool eof, FastFileSystemEntryBlock entryBlock)
        {
            this.length = entryBlock.ByteSize;
            this.volume = volume;
            this.writeMode = writeMode;
            this.eof = eof;
            this.fileHdr = entryBlock;
            this.pos = 0;
            this.posInExtBlk = 0;
            this.posInDataBlk = 0;
            this.currentData = new FastFileSystemDataBlock
            {
                Data = new byte[volume.DataBlockSize]
            };
        }

        protected override void Dispose(bool disposing)
        {
            AdfCloseFile().GetAwaiter().GetResult();
            base.Dispose(disposing);
        }

        public override void Flush()
        {
            AdfFlushFile().GetAwaiter().GetResult();
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await AdfFlushFile();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
            {
                throw new FileSystemDiagnosticException(FileSystemErrorCode.OnlyOffsetZeroSupported,
                    FastFileSystemErrorMessages.OnlyOffsetZeroSupported);
            }

            return AdfReadFile(count, buffer).GetAwaiter().GetResult();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            if (offset != 0)
            {
                throw new FileSystemDiagnosticException(FileSystemErrorCode.OnlyOffsetZeroSupported,
                    FastFileSystemErrorMessages.OnlyOffsetZeroSupported);
            }

            return await AdfReadFile(count, buffer);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            AdfFileSeek((uint)offset).GetAwaiter().GetResult();
            return this.pos;
        }

        public override void SetLength(long value)
        {
            throw new FileSystemDiagnosticException(FileSystemErrorCode.SetLengthUnsupported,
                FastFileSystemErrorMessages.SetLengthUnsupported);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
            {
                throw new FileSystemDiagnosticException(FileSystemErrorCode.OnlyOffsetZeroSupported,
                    FastFileSystemErrorMessages.OnlyOffsetZeroSupported);
            }

            AdfWriteFile(count, buffer).GetAwaiter().GetResult();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (offset != 0)
            {
                throw new FileSystemDiagnosticException(FileSystemErrorCode.OnlyOffsetZeroSupported,
                    FastFileSystemErrorMessages.OnlyOffsetZeroSupported);
            }

            await AdfWriteFile(count, buffer);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => writeMode;
        public override long Length => length;
        public override long Position
        {
            get => this.pos;
            set => Seek(value, SeekOrigin.Begin);
        }

    }
}
