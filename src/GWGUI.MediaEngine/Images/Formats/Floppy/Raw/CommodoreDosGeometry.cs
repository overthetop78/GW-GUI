using GWGUI.MediaFileSystems.Formats.Commodore;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Images.Formats.Floppy.D64;

using GWGUI.MediaEngine.Images.Formats.Floppy.D71;

using GWGUI.MediaEngine.Images.Formats.Floppy.D81;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

/// <summary>Convertit les coordonnées piste/secteur des volumes Commodore DOS en blocs logiques.</summary>
public static class CommodoreDosGeometry
{
    /// <summary>Tente de convertir une coordonnée sans recourir à une valeur sentinelle.</summary>
    public static bool TryToLogicalBlock(SectorImage image, int track, int sector, out int logicalBlock)
    {
        try
        {
            logicalBlock = ToLogicalBlock(image, track, sector);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            logicalBlock = default;
            return false;
        }
    }

    /// <summary>Convertit une coordonnée valide selon la géométrie 1541, 1571 ou 1581 de l'image.</summary>
    public static int ToLogicalBlock(SectorImage image, int track, int sector)
    {
        if (image.FormatId == DiskImageFormatIds.Commodore1581) return Commodore1581Geometry.ToLogicalBlock(track, sector);
        var tracksPerSide = image.Cylinders;
        var side = track > tracksPerSide ? 1 : 0;
        var sideTrack = side == 0 ? track : track - tracksPerSide;
        return image.Heads == Commodore1571Geometry.SideCount ? Commodore1571Geometry.ToLogicalBlock(sideTrack, sector, tracksPerSide, side) : Commodore1541Geometry.ToSideLogicalBlock(sideTrack, sector, tracksPerSide);
    }
}
