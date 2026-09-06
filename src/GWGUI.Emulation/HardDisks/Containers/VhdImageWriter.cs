using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

public static class VhdImageWriter
{
    public const long MaximumCapacity = 2040L << 30;

    public static void Validate(long capacity)
    {
        if (capacity < 512 || capacity > MaximumCapacity || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    public static void Write(Stream destination, long capacity, bool fixedSize = false, Action<Stream>? initialize = null)
    {
        ContainerValidation.Validate(destination, capacity);
        Validate(capacity);
        using var content = SparseImageContent.Create(capacity, initialize);
        using var disk = fixedSize
            ? DiscUtils.Vhd.Disk.InitializeFixed(destination, Ownership.None, capacity)
            : DiscUtils.Vhd.Disk.InitializeDynamic(destination, Ownership.None, capacity);
        SparseImageContent.CopyAllocated(content, disk.Content);
        disk.Content.Flush();
    }
}
