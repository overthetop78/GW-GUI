namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>Initializes an empty linear CP/M 2.2 directory, without installing boot tracks or a BIOS.</summary>
public static class CpmVolumeFormatter
{
    public static void Validate(long length, CpmVolumeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var block = options.AllocationBlockBytes;
        if (block < 1024 || block > 16384 || (block & (block - 1)) != 0 ||
            options.ReservedBytes < 0 || options.ReservedBytes % 128 != 0 || options.ReservedBytes >= length ||
            options.DirectoryEntries < 4 || options.DirectoryEntries > 65536 || options.DirectoryEntries % 4 != 0 ||
            length % 128 != 0)
            throw new ArgumentException("Invalid CP/M disk parameters.");
        var blocks = (length - options.ReservedBytes) / block;
        var directoryBlocks = ((long)options.DirectoryEntries * 32 + block - 1) / block;
        if (blocks > 65536 || blocks <= directoryBlocks || directoryBlocks > 16 || (blocks > 256 && block < 2048))
            throw new ArgumentException("CP/M directory reservation or allocation addressing is out of range.");
    }
    public static void Format(Stream volume, CpmVolumeOptions options)
    {
        Validate(volume.Length, options);
        var directoryBytes = checked(options.DirectoryEntries * 32);
        var erased = new byte[Math.Min(directoryBytes, 65536)]; Array.Fill(erased, (byte)0xe5);
        volume.Position = options.ReservedBytes;
        for (var remaining = directoryBytes; remaining > 0;)
        {
            var count = Math.Min(remaining, erased.Length); volume.Write(erased, 0, count); remaining -= count;
        }
        volume.Flush();
    }
}
