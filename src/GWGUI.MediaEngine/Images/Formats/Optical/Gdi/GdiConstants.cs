using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Images.Formats.Optical.Gdi;

/// <summary>Définit les constantes structurelles du descripteur de pistes Dreamcast GDI.</summary>
internal static class GdiConstants
{
    public const int DescriptorFieldCount = 6;
    public const int AudioControl = 0;
    public const int DataControl = 4;
    public const int DataSectorSize = 2048;
    public const int RawSectorSize = 2352;
    public const int DefaultTrackNumberWidth = 2;
    public const string FormatId = OpticalImageFormatIds.Gdi;
    public const string MediaProfile = "Dreamcast GD-ROM";
    public const string TrackCountMetadata = "trackCount";
    public const string TrackPrefixMetadata = "track.";
    public const string FileMetadataSuffix = ".file";
    public const string LbaMetadataSuffix = ".lba";
    public const string ControlMetadataSuffix = ".control";
    public const string SectorSizeMetadataSuffix = ".sectorSize";
    public static readonly IReadOnlySet<string> Extensions =
        new[] { DiskImageFileExtensions.Gdi }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
}
