using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Interpretations;

internal sealed class MsxAdditionalImageInterpretationPolicy : IAdditionalImageInterpretationPolicy
{
    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        if (image.BlockSize == 512 && SectorImageInterpretation.TryCreateMsx(image, out var interpretation))
            yield return interpretation;
    }
}
