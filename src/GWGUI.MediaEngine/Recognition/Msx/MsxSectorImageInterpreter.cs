using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Msx;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Msx;

/// <summary>Valide le boot MSX-DOS et réidentifie une image selon le catalogue de géométries MSX.</summary>
internal sealed class MsxSectorImageInterpreter
{
    /// <summary>Tente de créer une interprétation MSX sans modifier le contenu sectoriel.</summary>
    public bool TryInterpret(SectorImage image, out SectorImage interpretation)
    {
        interpretation = image;
        if (image.FormatId.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase) || image.BlockSize != FatBootSectorLayout.SectorSize || !image.TryGetBlock(FatBootSectorLayout.BootLogicalBlock, out var boot) || boot.Data.Count != FatBootSectorLayout.SectorSize || !MsxBootSectorProbe.LooksLikeMsx(boot.Data.ToArray())) return false;
        var geometry = MsxDiskGeometryCatalog.Find(checked(image.BlockCount * image.BlockSize), boot.Data[FatBootSectorLayout.MediaDescriptorOffset]);
        if (geometry is null) return false;
        interpretation = image.WithFormatId(geometry.FormatId);
        return true;
    }
}
