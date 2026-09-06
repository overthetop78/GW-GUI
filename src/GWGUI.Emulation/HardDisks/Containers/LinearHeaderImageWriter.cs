using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

internal static class LinearHeaderImageWriter
{
    internal static uint Cylinders(long capacity, int heads, int sectors, int sectorBytes)
    {
        if (heads is < 1 or > 65535 || sectors is < 1 or > 65535 || sectorBytes is not (128 or 256 or 512 or 1024 or 2048 or 4096))
            throw new ArgumentException("Invalid image geometry.");
        var cylinderBytes = checked((long)heads * sectors * sectorBytes);
        if (capacity <= 0 || capacity % cylinderBytes != 0 || capacity / cylinderBytes > uint.MaxValue)
            throw new ArgumentException("Capacity must contain a whole number of cylinders within the header limits.");
        return (uint)(capacity / cylinderBytes);
    }

    internal static void Write(Stream destination, long capacity, byte[] header, Action<Stream>? initialize)
    {
        if (!destination.CanSeek || !destination.CanWrite || destination.Length != 0)
            throw new ArgumentException("An empty writable seekable stream is required.", nameof(destination));
        // Initialize separately so malformed plans do not publish a partial header.
        using var content = SparseImageContent.Create(capacity, initialize);
        destination.SetLength(checked(capacity + header.Length));
        destination.Position = 0; destination.Write(header);
        var buffer = new byte[65536];
        foreach (var extent in content.Extents)
        {
            var end = Math.Min(capacity, extent.Start + extent.Length);
            for (var offset = extent.Start; offset < end;)
            {
                var length = (int)Math.Min(buffer.Length, end - offset);
                content.Position = offset; content.ReadExactly(buffer.AsSpan(0, length));
                destination.Position = header.Length + offset; destination.Write(buffer.AsSpan(0, length));
                offset += length;
            }
        }
        destination.Flush();
    }
}
