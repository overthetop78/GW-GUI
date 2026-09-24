using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Raw;

/// <summary>Définit la géométrie de repli des images Apple taguées sans modèle spécialisé.</summary>
public static class AppleTaggedImageGeometry
{
    /// <summary>Nombre de faces.</summary>
    public const int HeadCount = DiskGeometryConstants.SingleSidedHeadCount;
    /// <summary>Index de l'unique face.</summary>
    public const int FirstHead = 0;
    /// <summary>Nombre de secteurs par piste utilisé pour le repli.</summary>
    public const int SectorsPerTrack = 10;
    /// <summary>Nombre minimal de cylindres exposé.</summary>
    public const int MinimumCylinderCount = 1;
}
