using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Interpretations;

internal sealed class RecognizedImageNormalizerRegistry
{
    private readonly IReadOnlyList<IRecognizedImageNormalizer> normalizers =
    [
        new MacRecognizedImageNormalizer(),
        new MsxRecognizedImageNormalizer(),
        new AtariRecognizedImageNormalizer()
    ];

    public SectorImage Normalize(SectorImage image, string readerId, FileSystemVolume volume)
    {
        foreach (var normalizer in normalizers)
            if (normalizer.TryNormalize(image, readerId, volume, out var normalized)) return normalized;
        return image;
    }
}
