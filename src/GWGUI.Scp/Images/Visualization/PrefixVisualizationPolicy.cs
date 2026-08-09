using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Visualization;

internal sealed class PrefixVisualizationPolicy(string encoderId, params string[] prefixes)
    : SectorImageVisualizationPolicy
{
    public override bool CanHandle(SectorImage image) => prefixes.Any(prefix =>
        image.FormatId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    public override string EncoderId(SectorImage image) => encoderId;
}
