using GWGUI.MediaEngine.Interfaces.Reading.Recognition;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Reading.Recognition.Normalizers;

/// <summary>Réidentifie une image Macintosh MFM complète après reconnaissance HFS ou MFS.</summary>
internal sealed class MacRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    /// <summary>Normalise uniquement la géométrie complète de 1,44 Mio reconnue par un Reader Macintosh.</summary>
    public bool TryNormalize(SectorImage image, string readerId, out SectorImage normalized)
    {
        normalized = image;
        if (!readerId.Equals(FileSystemIds.MacHfs, StringComparison.OrdinalIgnoreCase) && !readerId.Equals(FileSystemIds.MacMfs, StringComparison.OrdinalIgnoreCase)) return false;
        if (image.BlockSize != MacintoshMfmGeometry.SectorSize || image.BlockCount != MacintoshMfmGeometry.SectorCount || image.FormatId.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase)) return false;
        normalized = image.WithFormatId(DiskImageFormatIds.Mac1440);
        return true;
    }
}
