using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Optical.CloneCd;

/// <summary>Declares the readable CloneCD 2 and 3 descriptor profile and its associated files.</summary>
internal static class CloneCdFormat
{
    public static readonly IReadOnlySet<string> Extensions = new[]
    {
        DiskImageFileExtensions.Ccd
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlySet<string> AssociatedFileExtensions = new[]
    {
        DiskImageFileExtensions.Img,
        DiskImageFileExtensions.Sub
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static string FormatId => OpticalImageFormatIds.CloneCd;
    public static bool CanRead => true;
    public static bool CanWrite => false;
    public static bool IsSupportedVersion(int version) =>
        version is >= CloneCdConstants.MinimumVersion and <= CloneCdConstants.MaximumVersion;
}
