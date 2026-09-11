using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Formats.HardDisk.Raw;

/// <summary>Declares the constraints of a headerless raw hard disk image.</summary>
internal static class RawHardDiskFormat
{
    public static readonly IReadOnlySet<string> Extensions = new[]
    {
        DiskImageFileExtensions.Raw,
        DiskImageFileExtensions.Img,
        DiskImageFileExtensions.Hdd,
        DiskImageFileExtensions.Hdf,
        DiskImageFileExtensions.Bin
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlySet<int> LogicalBlockSizes = new[]
    {
        HardDiskFormatConstants.LegacyLogicalSectorSize,
        HardDiskFormatConstants.LargeLogicalSectorSize
    }.ToFrozenSet();

    public static string FormatId => HardDiskImageFormatIds.Raw;

    public static HardDiskImageVariant Variant => HardDiskImageVariant.Raw;

    public static bool CanRead => true;

    public static bool CanWrite => true;

    public static bool IsLengthCompatible(long length, int logicalBlockSize) =>
        length > 0 && LogicalBlockSizes.Contains(logicalBlockSize) && length % logicalBlockSize == 0;
}
