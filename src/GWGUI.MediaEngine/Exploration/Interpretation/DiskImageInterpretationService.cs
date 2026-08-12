using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.Interpretations;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Interpretation;

/// <summary>Coordonne la normalisation et les interprétations supplémentaires d'une image reconnue.</summary>
internal sealed class DiskImageInterpretationService
{
    /// <summary>Registre des normalisations appliquées après reconnaissance.</summary>
    private readonly RecognizedImageNormalizerRegistry normalizers;
    /// <summary>Registre des interprétations supplémentaires.</summary>
    private readonly AdditionalImageInterpretationRegistry additionalInterpretations;

    /// <summary>Initialise le service avec les deux registres d'interprétation partagés.</summary>
    /// <param name="normalizers">Registre des normalisations reconnues.</param>
    /// <param name="additionalInterpretations">Registre des interprétations supplémentaires.</param>
    public DiskImageInterpretationService(RecognizedImageNormalizerRegistry normalizers, AdditionalImageInterpretationRegistry additionalInterpretations)
    {
        this.normalizers = normalizers;
        this.additionalInterpretations = additionalInterpretations;
    }

    /// <summary>Normalise une image à partir du lecteur et du volume qui l'ont reconnue.</summary>
    /// <param name="image">Image reconnue à normaliser.</param>
    /// <param name="readerId">Identifiant réel du lecteur ayant reconnu le volume.</param>
    /// <param name="volume">Volume décodé.</param>
    /// <returns>Image normalisée ou image initiale.</returns>
    public SectorImage NormalizeRecognizedImage(SectorImage image, string readerId, FileSystemVolume volume) => normalizers.Normalize(image, readerId, volume);

    /// <summary>Énumère les interprétations supplémentaires compatibles avec l'image.</summary>
    /// <param name="image">Image sectorielle à interpréter.</param>
    /// <returns>Interprétations supplémentaires dans l'ordre du registre.</returns>
    public IEnumerable<SectorImage> AdditionalFileSystemInterpretations(SectorImage image) => additionalInterpretations.Create(image);
}
