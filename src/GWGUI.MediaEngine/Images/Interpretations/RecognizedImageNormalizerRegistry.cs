using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Applique les normalisations d'images reconnues dans leur ordre déclaré.</summary>
internal sealed class RecognizedImageNormalizerRegistry
{
    /// <summary>Normaliseurs examinés dans leur ordre de priorité.</summary>
    private readonly IReadOnlyList<IRecognizedImageNormalizer> normalizers;

    /// <summary>Crée le registre avec les normaliseurs par défaut.</summary>
    public RecognizedImageNormalizerRegistry() : this([new MacRecognizedImageNormalizer(), new MsxRecognizedImageNormalizer(), new AtariRecognizedImageNormalizer()]) { }

    /// <summary>Crée le registre avec une copie ordonnée des normaliseurs fournis.</summary>
    /// <param name="normalizers">Normaliseurs à copier dans leur ordre d'exécution.</param>
    public RecognizedImageNormalizerRegistry(IEnumerable<IRecognizedImageNormalizer> normalizers) => this.normalizers = Array.AsReadOnly(normalizers.ToArray());

    /// <summary>Retourne la première normalisation applicable, ou l'image initiale.</summary>
    /// <param name="image">Image reconnue à normaliser.</param>
    /// <param name="readerId">Identifiant du lecteur ayant reconnu le volume.</param>
    /// <param name="volume">Volume reconnu.</param>
    /// <returns>Première image normalisée ou image initiale.</returns>
    public SectorImage Normalize(SectorImage image, string readerId, FileSystemVolume volume)
    {
        foreach (var normalizer in normalizers)
            if (normalizer.TryNormalize(image, readerId, volume, out var normalized)) return normalized;
        return image;
    }
}
