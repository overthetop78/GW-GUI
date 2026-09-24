namespace Hst.Amiga.FileSystems.Pfs3
{
#if NET6_0
    using System;
#endif
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class Pfs3EntryStream : Stream
#if NET6_0
        , IAsyncDisposable
#endif
    {
        private readonly Pfs3FileEntry fileEntry;
        private readonly Pfs3GlobalData g;
        private bool dataWritten;

        public Pfs3EntryStream(Pfs3FileEntry fileEntry, Pfs3GlobalData g)
        {
            this.fileEntry = fileEntry;
            this.g = g;
            this.dataWritten = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Pfs3File.Close(fileEntry, g).GetAwaiter().GetResult();
            if (this.dataWritten)
            {
                Pfs3Disk.UpdateDataCache(g).GetAwaiter().GetResult();
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
            {
                throw new System.NotSupportedException("Read only supports offset 0");
            }

            return (int)Pfs3Directory.ReadFromObject(fileEntry, buffer, (uint)buffer.Length, g).GetAwaiter().GetResult();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            if (offset != 0)
            {
                throw new System.NotSupportedException("Read only supports offset 0");
            }

            return (int)await Pfs3Directory.ReadFromObject(fileEntry, buffer, (uint)buffer.Length, g);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Pfs3Disk.SeekInObject(fileEntry, (int)offset, GetMode(origin), g).GetAwaiter().GetResult();
        }

        private int GetMode(SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return Pfs3Constants.OFFSET_BEGINNING;
                case SeekOrigin.End:
                    return Pfs3Constants.OFFSET_END;
                case SeekOrigin.Current:
                    return Pfs3Constants.OFFSET_CURRENT;
                default:
                    return Pfs3Constants.OFFSET_BEGINNING;
            }
        }

        public override void SetLength(long value)
        {
            throw new System.NotSupportedException("Entry stream doesn't support set length");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
            {
                throw new System.NotSupportedException("Write only supports offset 0");
            }

            this.dataWritten = true;
            Pfs3Directory.WriteToObject(fileEntry, buffer, (uint)count, g).GetAwaiter().GetResult();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (offset != 0)
            {
                throw new System.NotSupportedException("Write only supports offset 0");
            }

            this.dataWritten = true;
            await Pfs3Directory.WriteToObject(fileEntry, buffer, (uint)count, g);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => fileEntry.originalsize;

        public override long Position
        {
            get => fileEntry.offset;
            set => Seek(value, SeekOrigin.Begin);
        }

#if NET6_0
        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
        }
#endif
    }
}
