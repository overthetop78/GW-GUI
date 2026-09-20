using GWGUI.MediaEngine.Interfaces.Reading.Recognition;
using GWGUI.MediaFileSystems.FileSystems.Fat12;
using GWGUI.MediaEngine.Images.Reading.Recognition.Ibm;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Reading.Recognition.Policies;

/// <summary>Produit un candidat IBM depuis le BPB ou le descripteur de média FAT d'une image non IBM.</summary>
internal sealed class IbmAdditionalImageInterpretationPolicy : IAdditionalImageInterpretationPolicy
{
    private readonly IReadOnlySet<string> supportedFormatIds;

    /// <summary>Copie les identifiants de formats pris en charge sans tenir compte de la casse.</summary>
    public IbmAdditionalImageInterpretationPolicy(IEnumerable<string> supportedFormatIds) => this.supportedFormatIds = supportedFormatIds.ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Crée un candidat précis lorsqu'il est pris en charge, sinon un candidat d'analyse IBM.</summary>
    public IEnumerable<SectorImage> CreateCandidates(SectorImage image)
    {
        if (image.BlockSize != FatBootSectorLayout.SectorSize || image.FormatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) || !image.TryGetBlock(FatBootSectorLayout.BootLogicalBlock, out var boot) || boot.Data.Count != FatBootSectorLayout.SectorSize) yield break;
        var fatMedia = image.TryGetBlock(FatBootSectorLayout.FirstFatLogicalBlock, out var fat) && fat.Data.Count > FatBootSectorLayout.FatMediaDescriptorDataOffset ? fat.Data[FatBootSectorLayout.FatMediaDescriptorDataOffset] : FatBootSectorLayout.UnknownMediaDescriptor;
        if (!IbmDosDiskProbe.TryIdentify(boot.Data.ToArray(), fatMedia, false, out var geometry)) yield break;
        var physicalFormatId = DiskImageFormatIds.IbmFromCapacity(image.Capacity);
        var formatId = geometry.FormatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) && supportedFormatIds.Contains(geometry.FormatId)
            ? geometry.FormatId
            : supportedFormatIds.Contains(physicalFormatId)
                ? physicalFormatId
                : DiskImageFormatIds.IbmScan;
        yield return image.WithFormatId(formatId);
    }
}
