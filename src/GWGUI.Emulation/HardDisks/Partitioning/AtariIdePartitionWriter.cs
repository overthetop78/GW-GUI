using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Partitioning;

public static class AtariIdePartitionWriter
{
    public static Stream Create(Stream disk)
    {
        if (disk.Length < 1024 * 1024) throw new ArgumentOutOfRangeException(nameof(disk));
        var ide = new WordSwappedStream(disk);
        AhdiPartitionWriter.Write(ide, [new(512, disk.Length - 512)]);
        return new SubStream(ide, Ownership.Dispose, 512, disk.Length - 512);
    }

    private sealed class WordSwappedStream(Stream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => inner.CanWrite;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => inner.Position = value; }
        public override void Flush() => inner.Flush();
        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
        public override void SetLength(long value) => inner.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateWordAccess(count);
            var read = inner.Read(buffer, offset, count);
            if ((read & 1) != 0) throw new IOException("An Atari IDE image ended inside a 16-bit word.");
            SwapWords(buffer.AsSpan(offset, read));
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateWordAccess(count);
            var swapped = new byte[count];
            buffer.AsSpan(offset, count).CopyTo(swapped);
            SwapWords(swapped);
            inner.Write(swapped);
        }

        private void ValidateWordAccess(int count)
        {
            if ((inner.Position & 1) != 0 || (count & 1) != 0)
                throw new ArgumentException("Atari IDE image access must be aligned to 16-bit words.");
        }

        private static void SwapWords(Span<byte> bytes)
        {
            for (var i = 0; i < bytes.Length; i += 2)
                (bytes[i], bytes[i + 1]) = (bytes[i + 1], bytes[i]);
        }
    }
}
