using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Interpretations;

internal interface IRecognizedImageNormalizer
{
    bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized);
}
