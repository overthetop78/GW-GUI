using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Interpretations;

internal interface IAdditionalImageInterpretationPolicy
{
    IEnumerable<SectorImage> Create(SectorImage image);
}
