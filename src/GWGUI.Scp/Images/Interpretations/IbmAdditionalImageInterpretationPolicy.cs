using GWGUI.Scp.FileSystems;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images.Interpretations;

internal sealed class IbmAdditionalImageInterpretationPolicy(FileSystemRegistry fileSystems)
    : IAdditionalImageInterpretationPolicy
{
    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        if (image.BlockSize != 512 || image.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) ||
            !image.TryGetBlock(0, out var boot) || boot.Data.Count != 512) yield break;
        var fatMedia = image.TryGetBlock(1, out var fat) && fat.Data.Count > 0 ? fat.Data[0] : (byte)0;
        if (!IbmPcImageReader.TryDetectFluxGeometry(boot.Data.ToArray(), fatMedia, out var geometry)) yield break;
        var formatId = geometry.FormatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase) &&
                       fileSystems.SupportedFormatIds.Contains(geometry.FormatId)
            ? geometry.FormatId : "ibm.scan";
        yield return SectorImageInterpretation.Retag(image, formatId);
    }
}
