using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Interpretation.Contracts;

/// <summary>Définit une politique produisant des candidats ordonnés dont la validation reste confiée aux lecteurs de systèmes de fichiers.</summary>
internal interface IAdditionalImageInterpretationPolicy
{
    /// <summary>Crée zéro, un ou plusieurs candidats depuis l'image source.</summary>
    /// <param name="image">Image sectorielle source.</param>
    /// <returns>Énumération différée des candidats dans leur ordre de priorité.</returns>
    IEnumerable<SectorImage> CreateCandidates(SectorImage image);
}
