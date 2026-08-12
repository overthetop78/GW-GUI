using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Applique les normalisations d'images reconnues dans leur ordre déclaré.</summary>
internal sealed class RecognizedImageNormalizerRegistry
{
    /// <summary>Normaliseurs examinés dans leur ordre de priorité.</summary>
    private readonly IReadOnlyList<IRecognizedImageNormalizer> normalizers =
    [
        new MacRecognizedImageNormalizer(),
        new MsxRecognizedImageNormalizer(),
        new AtariRecognizedImageNormalizer()
    ];

    /// <summary>Retourne la première normalisation applicable, ou l'image initiale.</summary>
    public SectorImage Normalize(SectorImage image, string readerId, FileSystemVolume volume)
    {
        foreach (var normalizer in normalizers)
            if (normalizer.TryNormalize(image, readerId, volume, out var normalized)) return normalized;
        return image;
    }
}
