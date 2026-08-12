using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Définit la normalisation d'une image dont le système de fichiers est déjà reconnu.</summary>
internal interface IRecognizedImageNormalizer
{
    /// <summary>Tente de produire l'identité et la géométrie finales de l'image reconnue.</summary>
    bool TryNormalize(SectorImage image, string readerId, FileSystemVolume volume, out SectorImage normalized);
}
