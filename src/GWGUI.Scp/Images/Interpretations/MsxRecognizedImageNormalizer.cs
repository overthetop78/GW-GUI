using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Interpretations;

internal sealed class MsxRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        return readerId.Equals("fat12", StringComparison.OrdinalIgnoreCase) &&
               SectorImageInterpretation.TryCreateMsx(image, out normalized);
    }
}
