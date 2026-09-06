using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

public static class VmdkImageWriter
{
    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null)
    {
        ContainerValidation.Validate(destination, capacity);
        using var content = SparseImageContent.Create(capacity, initialize);
        var builder = new DiscUtils.Vmdk.DiskBuilder
        {
            Content = content,
            DiskType = DiscUtils.Vmdk.DiskCreateType.MonolithicSparse,
            AdapterType = DiscUtils.Vmdk.DiskAdapterType.Ide
        };
        var specification = builder.Build("disk").Single();
        using var image = specification.OpenStream();
        image.CopyTo(destination);
        destination.Flush();
    }
}
