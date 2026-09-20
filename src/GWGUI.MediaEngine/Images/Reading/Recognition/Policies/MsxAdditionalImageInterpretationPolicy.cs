using GWGUI.MediaEngine.Interfaces.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Recognition.Msx;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Reading.Recognition.Policies;

/// <summary>Produit le candidat MSX validé par l'interpréteur commun.</summary>
internal sealed class MsxAdditionalImageInterpretationPolicy(MsxSectorImageInterpreter interpreter) : IAdditionalImageInterpretationPolicy
{
    /// <summary>Retourne le candidat uniquement lorsque le boot et la géométrie MSX sont valides.</summary>
    public IEnumerable<SectorImage> CreateCandidates(SectorImage image)
    {
        if (interpreter.TryInterpret(image, out var interpretation)) yield return interpretation;
    }
}
