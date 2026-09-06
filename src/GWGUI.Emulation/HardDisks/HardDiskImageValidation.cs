namespace GWGUI.Emulation.HardDisks;

public static class HardDiskImageValidation
{
    public const int SectorBytes = 512;

    public static void Validate(string path, long length, HardDiskImageFormat format)
    {
        ValidateExtension(path, format);
        if (length < SectorBytes || length > format.MaximumBytes || length % SectorBytes != 0)
            throw new ArgumentOutOfRangeException(nameof(length), $"512 … {format.MaximumBytes} bytes; sectors: 512 bytes.");
    }

    public static void ValidateExisting(string path, HardDiskImageFormat format)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        ValidateExisting(stream, path, format);
    }

    public static void ValidateExisting(Stream stream, string path, HardDiskImageFormat format)
    {
        if (!stream.CanRead || !stream.CanSeek) throw new ArgumentException("A readable seekable image is required.");
        var position = stream.Position;
        try { stream.Position = 0; ValidateContent(stream, path, format); }
        finally { stream.Position = position; }
    }

    private static void ValidateContent(Stream stream, string path, HardDiskImageFormat format)
    {
        ValidateExtension(path, format);
        if (format.Container == Containers.DiskContainerKind.Gzip)
        {
            using var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress, leaveOpen: true);
            var buffer = new byte[65536]; long length = 0; int read;
            var head = new byte[80]; var footer = new byte[512];
            while ((read = gzip.Read(buffer)) != 0)
            {
                if (length < head.Length) buffer.AsSpan(0, (int)Math.Min(read, head.Length - length)).CopyTo(head.AsSpan((int)length));
                if (read >= 512) buffer.AsSpan(read - 512, 512).CopyTo(footer);
                else { footer.AsSpan(read).CopyTo(footer); buffer.AsSpan(0, read).CopyTo(footer.AsSpan(512 - read)); }
                length += read;
                if (length > format.MaximumBytes) throw new InvalidDataException("The expanded disk exceeds the supported capacity.");
            }
            Validate(path, length, format);
            RejectContainer(Containers.DiskContainerSignatures.Identify(head, footer));
            return;
        }
        if (format.Container != Containers.DiskContainerKind.Raw)
            throw new NotSupportedException("Existing-image validation is not registered for this container.");
        Validate(path, stream.Length, format);
        RejectContainer(Containers.DiskContainerSignatures.Identify(stream));
    }

    private static void RejectContainer(string? container)
    {
        if (container is not null) throw new InvalidDataException($"The image contains a {container} container instead of raw disk sectors.");
    }

    private static void ValidateExtension(string path, HardDiskImageFormat format)
    {
        if (!string.Equals(Path.GetExtension(path), format.Extension, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"{format.InterfaceName}: {format.Extension}");
    }
}
