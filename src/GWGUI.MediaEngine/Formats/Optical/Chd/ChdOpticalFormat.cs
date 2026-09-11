using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Optical.Chd;

/// <summary>Declares the autonomous CHD V5 optical profile classified by optical metadata.</summary>
internal static class ChdOpticalFormat
{
    public static readonly ReadOnlyMemory<byte> Signature =
        System.Text.Encoding.ASCII.GetBytes(ChdConstants.Signature);

    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Chd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static readonly IReadOnlySet<uint> MetadataTags =
        new[]
        {
            ChdConstants.CdTrackMetadataTag,
            ChdConstants.CdTrackMetadata2Tag,
            ChdConstants.DvdMetadataTag
        }.ToFrozenSet();

    public static string FormatId => OpticalImageFormatIds.Chd;

    public static uint Version => ChdConstants.Version5;

    public static bool CanRead => true;

    public static bool CanWrite => false;
}
