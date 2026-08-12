using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Interpretation.Normalizers;

/// <summary>Réidentifie une image Macintosh MFM complète après reconnaissance HFS ou MFS.</summary>
internal sealed class MacRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    /// <summary>Normalise uniquement la géométrie complète de 1,44 Mio reconnue par un Reader Macintosh.</summary>
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        if (!readerId.Equals(FileSystemIds.MacHfs, StringComparison.OrdinalIgnoreCase) && !readerId.Equals(FileSystemIds.MacMfs, StringComparison.OrdinalIgnoreCase)) return false;
        if (image.BlockSize != MacintoshMfmGeometry.SectorSize || image.BlockCount != MacintoshMfmGeometry.SectorCount || image.FormatId.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase)) return false;
        normalized = image.WithFormatId(DiskImageFormatIds.Mac1440);
        return true;
    }
}
