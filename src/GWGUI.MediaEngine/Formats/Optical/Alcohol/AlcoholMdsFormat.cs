using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Optical.Alcohol;

/// <summary>Declares the autonomous MDS version 1 profile and its associated MDF data files.</summary>
internal static class AlcoholMdsFormat
{
    public static readonly ReadOnlyMemory<byte> HeaderSignature = System.Text.Encoding.ASCII.GetBytes(AlcoholMdsConstants.Signature);
    public static readonly IReadOnlySet<string> Extensions = new[]
    {
        DiskImageFileExtensions.Mds
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static readonly IReadOnlySet<string> AssociatedFileExtensions = new[]
    {
        DiskImageFileExtensions.Mdf
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static string FormatId => OpticalImageFormatIds.AlcoholMds;
    public static bool CanRead => true;
    public static bool CanWrite => false;
    public static bool IsSupportedVersion(byte major) => major <= AlcoholMdsConstants.MaximumSupportedMajorVersion;
}
