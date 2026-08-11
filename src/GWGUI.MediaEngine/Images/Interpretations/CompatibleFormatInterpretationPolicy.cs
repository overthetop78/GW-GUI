using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;


namespace GWGUI.MediaEngine.Images.Interpretations;

internal sealed class CompatibleFormatInterpretationPolicy : IAdditionalImageInterpretationPolicy
{
    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        var formatIds = image.BlockSize switch
        {
            512 => (IReadOnlyList<string>)[DiskImageFormatIds.UcsdIbmMfm, DiskImageFormatIds.Commodore900Coherent,
                DiskImageFormatIds.EpsonQx10_396, DiskImageFormatIds.EpsonQx10_399, DiskImageFormatIds.EpsonQx10Logo],
            256 => [DiskImageFormatIds.AcornDfsSingleSided, DiskImageFormatIds.AcornDfsSingleSided80,
                DiskImageFormatIds.AcornDfsDoubleSided, DiskImageFormatIds.AcornDfsDoubleSided80,
                DiskImageFormatIds.EpsonQx10_320],
            1024 => [DiskImageFormatIds.EpsonQx10_400],
            _ => []
        };
        foreach (var formatId in formatIds)
            if (!formatId.Equals(image.FormatId, StringComparison.OrdinalIgnoreCase))
                yield return SectorImageInterpretation.Retag(image, formatId);
    }
}
