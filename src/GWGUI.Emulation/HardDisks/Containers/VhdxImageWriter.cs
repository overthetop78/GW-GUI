using DiscUtils.Streams;

namespace GWGUI.Emulation.HardDisks.Containers;

public static class VhdxImageWriter
{
    public const long MaximumCapacity = 64L << 40;

    public static void Validate(long capacity)
        => Validate(capacity, 512);

    public static void Validate(long capacity, int logicalSectorBytes)
    {
        if (logicalSectorBytes is not (512 or 4096)) throw new ArgumentOutOfRangeException(nameof(logicalSectorBytes));
        if (capacity < logicalSectorBytes || capacity > MaximumCapacity || capacity % logicalSectorBytes != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    public static void Write(Stream destination, long capacity, bool fixedSize = false, Action<Stream>? initialize = null,
        int logicalSectorBytes = 512)
    {
        ContainerValidation.Validate(destination, capacity);
        Validate(capacity, logicalSectorBytes);
        using var content = SparseImageContent.Create(capacity, initialize);
        var geometry = DiscUtils.Geometry.FromCapacity(capacity, logicalSectorBytes);
        using var disk = fixedSize
            ? DiscUtils.Vhdx.DiskImageFile.InitializeFixed(destination, Ownership.None, capacity, geometry)
            : DiscUtils.Vhdx.DiskImageFile.InitializeDynamic(destination, Ownership.None, capacity, geometry);
        using var logical = disk.OpenContent(null!, Ownership.None);
        SparseImageContent.CopyAllocated(content, logical);
        logical.Flush();
    }
}
