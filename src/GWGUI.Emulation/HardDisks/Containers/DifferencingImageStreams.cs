using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

internal static class DifferencingImageStreams
{
    internal static void Validate(Stream destination, Stream parent, string absolutePath, string relativePath, DateTime modifiedUtc)
    {
        ArgumentNullException.ThrowIfNull(parent);
        if (!parent.CanRead || !parent.CanSeek || ReferenceEquals(destination, parent))
            throw new ArgumentException("A distinct readable seekable parent stream is required.");
        if (!Path.IsPathFullyQualified(absolutePath) || string.IsNullOrWhiteSpace(relativePath) || Path.IsPathFullyQualified(relativePath) ||
            absolutePath.Any(char.IsControl) || relativePath.Any(char.IsControl) || modifiedUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Parent locators and a UTC modification time are required.");
        ContainerValidation.Validate(destination, 512);
    }

    internal static void Publish(SparseMemoryStream staged, Stream destination)
    {
        destination.SetLength(staged.Length);
        var buffer = new byte[65536];
        foreach (var extent in staged.Extents)
        {
            staged.Position = extent.Start; destination.Position = extent.Start;
            var remaining = Math.Min(extent.Length, staged.Length - extent.Start);
            while (remaining > 0)
            {
                var count = (int)Math.Min(buffer.Length, remaining); staged.ReadExactly(buffer.AsSpan(0, count));
                destination.Write(buffer, 0, count); remaining -= count;
            }
        }
        destination.Flush();
    }

    internal sealed class ReadOnlyParent(Stream source) : Stream
    {
        private readonly long initialPosition = source.Position;
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => source.Length;
        public override long Position { get => source.Position; set => source.Position = value; }
        public override int Read(byte[] buffer, int offset, int count) => source.Read(buffer, offset, count);
        public override int Read(Span<byte> buffer) => source.Read(buffer);
        public override long Seek(long offset, SeekOrigin origin) => source.Seek(offset, origin);
        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        { if (disposing) source.Position = initialPosition; base.Dispose(disposing); }
    }
}
