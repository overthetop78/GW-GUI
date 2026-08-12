using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Produit l'interprétation MSX supplémentaire d'une image compatible.</summary>
internal sealed class MsxAdditionalImageInterpretationPolicy : IAdditionalImageInterpretationPolicy
{
    /// <inheritdoc />
    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        if (image.BlockSize == FatBootSectorLayout.SectorSize && SectorImageInterpretation.TryCreateMsx(image, out var interpretation)) yield return interpretation;
    }
}
