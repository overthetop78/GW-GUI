using GWGUI.MediaFileSystems.Formats.Commodore;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.D81;

/// <summary>Décrit la disposition logique exacte d'un conteneur D81.</summary>
public static class D81Layout
{
    /// <summary>Nombre total de blocs logiques.</summary>
    public const int LogicalBlockCount = Commodore1581Geometry.LogicalCylinderCount * Commodore1581Geometry.LogicalBlocksPerTrack;
    /// <summary>Longueur exacte du conteneur en octets.</summary>
    public const int ImageLength = LogicalBlockCount * Commodore1581Geometry.LogicalBlockSize;
}
