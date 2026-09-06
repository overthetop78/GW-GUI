using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

public static class VdiImageWriter
{
    public static void Write(Stream destination, long capacity, bool fixedSize = false, Action<Stream>? initialize = null)
    {
        ContainerValidation.Validate(destination, capacity);
        using var content = SparseImageContent.Create(capacity, initialize);
        using var disk = fixedSize
            ? DiscUtils.Vdi.Disk.InitializeFixed(destination, Ownership.None, capacity)
            : DiscUtils.Vdi.Disk.InitializeDynamic(destination, Ownership.None, capacity);
        SparseImageContent.CopyAllocated(content, disk.Content);
        disk.Content.Flush();
    }
}
