using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

using GWGUI.MediaEngine.Recognition.Definitions;

namespace GWGUI.MediaEngine.Images.Interpretations;

internal sealed class MacRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        if (!readerId.Equals("mac-hfs", StringComparison.OrdinalIgnoreCase) &&
            !readerId.Equals("mac-mfs", StringComparison.OrdinalIgnoreCase)) return false;
        if (image.BlockSize != 512 || image.BlockCount != 2880 ||
            image.FormatId.Equals(DiskImageFormatIds.Mac1440, StringComparison.OrdinalIgnoreCase)) return false;
        normalized = SectorImageInterpretation.Retag(image, DiskImageFormatIds.Mac1440);
        return true;
    }
}
