using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Visualization;

internal sealed class ExactVisualizationPolicy(string encoderId, params string[] formatIds)
    : SectorImageVisualizationPolicy
{
    public override bool CanHandle(SectorImage image) => formatIds.Contains(image.FormatId,
        StringComparer.OrdinalIgnoreCase);

    public override string EncoderId(SectorImage image) => encoderId;
}
