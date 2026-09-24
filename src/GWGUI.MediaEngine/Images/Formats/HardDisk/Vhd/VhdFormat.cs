using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Vhd;

/// <summary>Declares the autonomous VHD layouts supported by MediaEngine.</summary>
internal static class VhdFormat
{
    public static readonly ReadOnlyMemory<byte> FooterSignature =
        System.Text.Encoding.ASCII.GetBytes(HardDiskFormatConstants.VhdFooterCookie);
    public static readonly ReadOnlyMemory<byte> DynamicHeaderSignature =
        System.Text.Encoding.ASCII.GetBytes(HardDiskFormatConstants.VhdDynamicHeaderCookie);
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Vhd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<HardDiskImageVariant> Variants = new[]
    {
        HardDiskImageVariant.Fixed,
        HardDiskImageVariant.Dynamic
    }.ToFrozenSet();

    public static string FormatId => HardDiskImageFormatIds.Vhd;

    public static bool CanRead => true;

    public static bool CanWrite => true;
}
