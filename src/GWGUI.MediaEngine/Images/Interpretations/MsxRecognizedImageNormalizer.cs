using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

internal sealed class MsxRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        return readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase) &&
               SectorImageInterpretation.TryCreateMsx(image, out normalized);
    }
}
