using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Interfaces.Reading.Recognition;

/// <summary>Définit la normalisation d'une image dont le système de fichiers est déjà reconnu.</summary>
internal interface IRecognizedImageNormalizer
{
    /// <summary>Tente de normaliser l'image et laisse l'image source dans <paramref name="normalized"/> en cas de refus.</summary>
    bool TryNormalize(SectorImage image, string readerId, out SectorImage normalized);
}
