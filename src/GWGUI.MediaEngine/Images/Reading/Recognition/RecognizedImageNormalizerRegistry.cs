using GWGUI.MediaEngine.Interfaces.Reading.Recognition;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Reading.Recognition;

/// <summary>Applique une copie ordonnée des normaliseurs et s'arrête au premier succès.</summary>
internal sealed class RecognizedImageNormalizerRegistry
{
    private readonly IReadOnlyList<IRecognizedImageNormalizer> normalizers;

    /// <summary>Construit le registre en copiant les normaliseurs dans leur ordre d'exécution.</summary>
    public RecognizedImageNormalizerRegistry(IEnumerable<IRecognizedImageNormalizer> normalizers) => this.normalizers = Array.AsReadOnly(normalizers.ToArray());

    /// <summary>Transmet sans modification le Reader puis retourne la première normalisation ou l'image source.</summary>
    public SectorImage Normalize(SectorImage image, string readerId)
    {
        foreach (var normalizer in normalizers)
            if (normalizer.TryNormalize(image, readerId, out var normalized)) return normalized;
        return image;
    }
}
