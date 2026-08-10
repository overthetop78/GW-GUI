using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

internal interface IAdditionalImageInterpretationPolicy
{
    IEnumerable<SectorImage> Create(SectorImage image);
}
