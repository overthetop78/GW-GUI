using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.Images.Visualization;

internal sealed class SectorImageVisualizationPolicyRegistry
{
    private readonly IReadOnlyList<ISectorImageVisualizationPolicy> _policies =
    [
        new AppleVisualizationPolicy(),
        new CommodoreVisualizationPolicy(),
        new DecRx02VisualizationPolicy(),
        new AtariVisualizationPolicy(),
        new PrefixVisualizationPolicy(FluxCodecIds.AmigaMfm, DiskImageFormatIds.AmigaPrefix),
        new PrefixVisualizationPolicy(FluxCodecIds.IsoFm, DiskImageFormatIds.AcornDfsPrefix),
        new PrefixVisualizationPolicy(FluxCodecIds.IsoMfm, DiskImageFormatIds.AcornAdfsPrefix),
        new PrefixVisualizationPolicy(FluxCodecIds.IsoMfm, DiskImageFormatIds.IbmPrefix, DiskImageFormatIds.AmstradPrefix,
            DiskImageFormatIds.MsxPrefix, DiskImageFormatIds.UcsdPrefix, DiskImageFormatIds.EpsonQx10Prefix),
        new ExactVisualizationPolicy(FluxCodecIds.IsoMfm, DiskImageFormatIds.Imd, DiskImageFormatIds.Td0)
    ];

    public ISectorImageVisualizationPolicy? Resolve(SectorImage image) =>
        _policies.FirstOrDefault(policy => policy.CanHandle(image));
}
