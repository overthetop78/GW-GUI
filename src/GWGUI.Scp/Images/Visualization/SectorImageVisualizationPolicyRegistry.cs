using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Visualization;

internal sealed class SectorImageVisualizationPolicyRegistry
{
    private readonly IReadOnlyList<ISectorImageVisualizationPolicy> _policies =
    [
        new AppleVisualizationPolicy(),
        new CommodoreVisualizationPolicy(),
        new DecRx02VisualizationPolicy(),
        new AtariVisualizationPolicy(),
        new PrefixVisualizationPolicy("amiga.mfm", "amiga."),
        new PrefixVisualizationPolicy("iso.fm", "acorn.dfs."),
        new PrefixVisualizationPolicy("iso.mfm", "acorn.adfs."),
        new PrefixVisualizationPolicy("iso.mfm", "ibm.", "amstrad.", "msx.", "ucsd.", "epson."),
        new ExactVisualizationPolicy("iso.mfm", "imd", "td0")
    ];

    public ISectorImageVisualizationPolicy? Resolve(SectorImage image) =>
        _policies.FirstOrDefault(policy => policy.CanHandle(image));
}
