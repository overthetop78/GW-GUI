using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Recognition.Ibm;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images.Interpretations;

/// <summary>Produit une interprétation IBM PC lorsqu'un secteur d'amorçage DOS valide est détecté.</summary>
/// <param name="fileSystems">Registre des systèmes de fichiers permettant de confirmer l'identifiant IBM.</param>
internal sealed class IbmAdditionalImageInterpretationPolicy(FileSystemRegistry fileSystems)
    : IAdditionalImageInterpretationPolicy
{
    /// <inheritdoc />
    public IEnumerable<SectorImage> Create(SectorImage image)
    {
        if (image.BlockSize != FatBootSectorLayout.SectorSize || image.FormatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) || !image.TryGetBlock(0, out var boot) || boot.Data.Count != FatBootSectorLayout.SectorSize) yield break;
        var fatMedia = image.TryGetBlock(FatBootSectorLayout.FirstFatSectorNumber - FatBootSectorLayout.FirstSectorNumber, out var fat) && fat.Data.Count > FatBootSectorLayout.FatMediaDescriptorDataOffset ? fat.Data[FatBootSectorLayout.FatMediaDescriptorDataOffset] : FatBootSectorLayout.UnknownMediaDescriptor;
        if (!IbmDosDiskProbe.TryIdentify(boot.Data.ToArray(), fatMedia, false, out var geometry)) yield break;
        var formatId = geometry.FormatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) && fileSystems.SupportedFormatIds.Contains(geometry.FormatId) ? geometry.FormatId : DiskImageFormatIds.IbmScan;
        yield return SectorImageInterpretation.Retag(image, formatId);
    }
}
