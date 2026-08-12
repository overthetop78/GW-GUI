using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Normalise une image FAT reconnue vers sa géométrie MSX.</summary>
internal sealed class MsxRecognizedImageNormalizer : IRecognizedImageNormalizer
{
    /// <inheritdoc />
    public bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized)
    {
        normalized = image;
        return readerId.Equals(FileSystemIds.Fat12, StringComparison.OrdinalIgnoreCase) && SectorImageInterpretation.TryCreateMsx(image, out normalized);
    }
}
