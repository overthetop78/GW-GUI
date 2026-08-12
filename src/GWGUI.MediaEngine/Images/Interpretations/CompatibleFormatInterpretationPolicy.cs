using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Acorn;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Produit les identités de formats partageant la même taille de bloc.</summary>
internal sealed class CompatibleFormatInterpretationPolicy : IAdditionalImageInterpretationPolicy
{
    /// <inheritdoc />
    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        var formatIds = image.BlockSize switch
        {
            FatBootSectorLayout.SectorSize => (IReadOnlyList<string>)[DiskImageFormatIds.UcsdIbmMfm, DiskImageFormatIds.Commodore900Coherent,
                DiskImageFormatIds.EpsonQx10_396, DiskImageFormatIds.EpsonQx10_399, DiskImageFormatIds.EpsonQx10Logo],
            BbcDfsGeometry.SectorSize => [DiskImageFormatIds.AcornDfsSingleSided, DiskImageFormatIds.AcornDfsSingleSided80,
                DiskImageFormatIds.AcornDfsDoubleSided, DiskImageFormatIds.AcornDfsDoubleSided80,
                DiskImageFormatIds.EpsonQx10_320],
            AcornAdfGeometry.BlockSize => [DiskImageFormatIds.EpsonQx10_400],
            _ => []
        };
        foreach (var formatId in formatIds)
        {
            if (!formatId.Equals(image.FormatId, StringComparison.OrdinalIgnoreCase)) yield return SectorImageInterpretation.Retag(image, formatId);
        }
    }
}
