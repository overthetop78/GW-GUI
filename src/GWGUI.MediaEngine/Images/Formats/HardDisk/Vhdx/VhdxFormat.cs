using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Vhdx;

/// <summary>Declares the autonomous VHDX layouts supported by MediaEngine.</summary>
internal static class VhdxFormat
{
    public static readonly ReadOnlyMemory<byte> FileSignature = System.Text.Encoding.ASCII.GetBytes("vhdxfile");
    public static readonly ReadOnlyMemory<byte> HeaderSignature = System.Text.Encoding.ASCII.GetBytes("head");
    public static readonly ReadOnlyMemory<byte> RegionTableSignature = System.Text.Encoding.ASCII.GetBytes("regi");
    public static readonly ReadOnlyMemory<byte> MetadataTableSignature = System.Text.Encoding.ASCII.GetBytes("metadata");
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Vhdx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<int> LogicalSectorSizes =
        new[] { HardDiskFormatConstants.LegacyLogicalSectorSize, HardDiskFormatConstants.LargeLogicalSectorSize }.ToFrozenSet();
    public static readonly IReadOnlySet<HardDiskImageVariant> Variants = new[]
    {
        HardDiskImageVariant.Fixed,
        HardDiskImageVariant.Dynamic
    }.ToFrozenSet();

    public static string FormatId => HardDiskImageFormatIds.Vhdx;

    public static bool CanRead => true;

    public static bool CanWrite => true;
}
