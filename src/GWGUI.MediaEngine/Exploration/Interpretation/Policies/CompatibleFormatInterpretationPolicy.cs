using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.Exploration.Interpretation.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Interpretation.Policies;

/// <summary>Produit des candidats de formats partageant la taille de bloc de l'image source.</summary>
internal sealed class CompatibleFormatInterpretationPolicy : IAdditionalImageInterpretationPolicy
{
    /// <summary>Énumère les candidats dans l'ordre du catalogue sans reproduire le format source.</summary>
    public IEnumerable<SectorImage> CreateCandidates(SectorImage image)
    {
        foreach (var formatId in CompatibleFormatCatalog.Resolve(image.BlockSize))
            if (!formatId.Equals(image.FormatId, StringComparison.OrdinalIgnoreCase)) yield return image.WithFormatId(formatId);
    }
}
