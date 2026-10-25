namespace GWGUI.Emulation.HardDisks;

public static class RawHardDiskImageWriter
{
    // A factory-new hard disk has no partition table or filesystem. These are
    // installed by the guest's disk tools, just as on the original hardware.
    public static void Write(Stream destination, long bytes, Action<Stream>? initialize = null) => Write(destination, bytes, initialize, 512);

    public static void Write(Stream destination, long bytes, Action<Stream>? initialize, int logicalSectorBytes)
    {
        if (!destination.CanSeek || !destination.CanWrite || destination.Length != 0)
            throw new ArgumentException("An empty writable seekable stream is required.", nameof(destination));
        if (logicalSectorBytes <= 0) throw new ArgumentOutOfRangeException(nameof(logicalSectorBytes));
        if (bytes < logicalSectorBytes || bytes % logicalSectorBytes != 0) throw new ArgumentOutOfRangeException(nameof(bytes));
        using var content = Containers.SparseImageContent.Create(bytes, initialize);
        destination.SetLength(bytes);
        destination.Position = 0;
        destination.Write(new byte[logicalSectorBytes]);
        Containers.SparseImageContent.CopyAllocated(content, destination);
        destination.Flush();
    }
}
