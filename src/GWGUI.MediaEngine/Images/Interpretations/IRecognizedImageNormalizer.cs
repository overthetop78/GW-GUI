using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

internal interface IRecognizedImageNormalizer
{
    bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized);
}
