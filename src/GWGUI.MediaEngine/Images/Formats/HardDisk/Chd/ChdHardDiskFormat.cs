using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Chd;

/// <summary>Declares the autonomous, uncompressed CHD V5 hard disk profile supported by MediaEngine.</summary>
internal static class ChdHardDiskFormat
{
    public static readonly ReadOnlyMemory<byte> Signature =
        System.Text.Encoding.ASCII.GetBytes(ChdConstants.Signature);

    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Chd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static string FormatId => HardDiskImageFormatIds.Chd;

    public static uint Version => ChdConstants.Version5;

    public static uint RequiredMetadataTag => ChdConstants.HardDiskMetadataTag;

    public static bool CanRead => true;

    public static bool CanWrite => true;
}
