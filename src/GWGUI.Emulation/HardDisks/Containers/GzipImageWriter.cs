using System.IO.Compression;
using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

public static class GzipImageWriter
{
    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null)
    {
        ContainerValidation.Validate(destination, capacity);
        using var content = SparseImageContent.Create(capacity, initialize);
        content.Position = 0;
        using var compressed = new GZipStream(destination, CompressionLevel.Fastest, leaveOpen: true);
        content.CopyTo(compressed);
    }
}
