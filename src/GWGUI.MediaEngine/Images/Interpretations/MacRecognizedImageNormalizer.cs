using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Normalise une image HFS ou MFS de géométrie Macintosh MFM 1,44 Mio.</summary>
internal sealed class MacRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    /// <inheritdoc />
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        if (!readerId.Equals(FileSystemIds.MacHfs, StringComparison.OrdinalIgnoreCase) && !readerId.Equals(FileSystemIds.MacMfs, StringComparison.OrdinalIgnoreCase)) return false;
        if (image.BlockSize != MacintoshMfm1440Geometry.SectorSize || image.BlockCount != MacintoshMfm1440Geometry.SectorCount || image.FormatId.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase)) return false;
        normalized = SectorImageInterpretation.Retag(image, DiskImageFormatIds.Mac1440);
        return true;
    }
}
