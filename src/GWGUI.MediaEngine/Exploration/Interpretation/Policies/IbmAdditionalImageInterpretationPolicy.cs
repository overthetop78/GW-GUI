using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Interpretation.Contracts;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Interpretation.Policies;

/// <summary>Produit un candidat IBM depuis le BPB ou le descripteur de média FAT d'une image non IBM.</summary>
internal sealed class IbmAdditionalImageInterpretationPolicy : IAdditionalImageInterpretationPolicy
{
    private readonly IReadOnlySet<string> supportedFormatIds;

    /// <summary>Copie les identifiants de formats pris en charge sans tenir compte de la casse.</summary>
    public IbmAdditionalImageInterpretationPolicy(IEnumerable<string> supportedFormatIds) => this.supportedFormatIds = supportedFormatIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Crée un candidat précis lorsqu'il est pris en charge, sinon un candidat d'analyse IBM.</summary>
    public IEnumerable<SectorImage> CreateCandidates(SectorImage image)
    {
        if (image.BlockSize != FatBootSectorLayout.SectorSize || image.FormatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) || !image.TryGetBlock(FatBootSectorLayout.FirstSectorNumber, out var boot) || boot.Data.Count != FatBootSectorLayout.SectorSize) yield break;
        var fatMedia = image.TryGetBlock(FatBootSectorLayout.FirstFatSectorNumber - FatBootSectorLayout.FirstSectorNumber, out var fat) && fat.Data.Count > FatBootSectorLayout.FatMediaDescriptorDataOffset ? fat.Data[FatBootSectorLayout.FatMediaDescriptorDataOffset] : FatBootSectorLayout.UnknownMediaDescriptor;
        if (!IbmBootGeometryDetector.TryDetect(boot.Data.ToArray(), fatMedia, out var geometry)) yield break;
        var formatId = geometry.FormatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) && supportedFormatIds.Contains(geometry.FormatId) ? geometry.FormatId : DiskImageFormatIds.IbmScan;
        yield return image.WithFormatId(formatId);
    }
}
