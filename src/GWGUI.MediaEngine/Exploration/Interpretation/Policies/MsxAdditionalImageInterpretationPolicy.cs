using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.Recognition.Msx;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Interpretation.Policies;

/// <summary>Produit le candidat MSX validé par l'interpréteur commun.</summary>
internal sealed class MsxAdditionalImageInterpretationPolicy(MsxSectorImageInterpreter interpreter) : IAdditionalImageInterpretationPolicy
{
    /// <summary>Retourne le candidat uniquement lorsque le boot et la géométrie MSX sont valides.</summary>
    public IEnumerable<SectorImage> CreateCandidates(SectorImage image)
    {
        if (interpreter.TryInterpret(image, out var interpretation)) yield return interpretation;
    }
}
