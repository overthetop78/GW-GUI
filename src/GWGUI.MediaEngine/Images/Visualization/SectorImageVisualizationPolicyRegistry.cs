using GWGUI.MediaEngine.SectorImages;

using GWGUI.MediaEngine.Recognition.Definitions;

namespace GWGUI.MediaEngine.Images.Visualization;

internal sealed class SectorImageVisualizationPolicyRegistry
{
    private readonly IReadOnlyList<ISectorImageVisualizationPolicy> _policies =
    [
        new AppleVisualizationPolicy(),
        new CommodoreVisualizationPolicy(),
        new DecRx02VisualizationPolicy(),
        new AtariVisualizationPolicy(),
        new PrefixVisualizationPolicy("amiga.mfm", DiskImageFormatIds.AmigaPrefix),
        new PrefixVisualizationPolicy("iso.fm", DiskImageFormatIds.AcornDfsPrefix),
        new PrefixVisualizationPolicy("iso.mfm", DiskImageFormatIds.AcornAdfsPrefix),
        new PrefixVisualizationPolicy("iso.mfm", DiskImageFormatIds.IbmPrefix, DiskImageFormatIds.AmstradPrefix,
            DiskImageFormatIds.MsxPrefix, DiskImageFormatIds.UcsdPrefix, DiskImageFormatIds.EpsonQx10Prefix),
        new ExactVisualizationPolicy("iso.mfm", DiskImageFormatIds.Imd, DiskImageFormatIds.Td0)
    ];

    public ISectorImageVisualizationPolicy? Resolve(SectorImage image) =>
        _policies.FirstOrDefault(policy => policy.CanHandle(image));
}
