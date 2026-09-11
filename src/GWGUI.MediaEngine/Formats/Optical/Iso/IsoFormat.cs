using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Formats.Optical.Iso;

/// <summary>Declares the supported single-track, 2048-byte-sector ISO image profile.</summary>
internal static class IsoFormat
{
    public const int SectorSize = OpticalSectorConstants.Data2048Size;
    public const int FirstVolumeDescriptorSector = 16;

    public static readonly IReadOnlySet<string> Extensions = new[]
    {
        DiskImageFileExtensions.Iso
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static string FormatId => OpticalImageFormatIds.Iso;

    public static bool CanRead => true;

    public static bool CanWrite => true;

    public static bool IsLengthCompatible(long length) => length > 0 && length % SectorSize == 0;
}
