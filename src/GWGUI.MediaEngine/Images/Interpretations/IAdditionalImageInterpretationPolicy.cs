using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Définit une politique produisant des interprétations supplémentaires d'une image.</summary>
internal interface IAdditionalImageInterpretationPolicy
{
    /// <summary>Crée les interprétations compatibles avec l'image fournie.</summary>
    IEnumerable<SectorImage> Create(SectorImage image);
}
