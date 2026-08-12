namespace GWGUI.MediaEngine.Geometries.Commodore;

/// <summary>Compose exactement deux faces 1541, la première précédant la seconde dans l'ordre logique D71.</summary>
public static class Commodore1571Geometry
{
    /// <summary>Nombre exact de faces.</summary>
    public const int SideCount = 2;

    /// <summary>Convertit une adresse des deux faces en bloc logique D71.</summary>
    public static int ToLogicalBlock(int track, int sector, int tracksPerSide, int side)
    {
        if (side is < 0 or >= SideCount) throw CommodoreGeometryExceptions.InvalidSide(side, SideCount);
        return side * Commodore1541Geometry.BlocksPerSide(tracksPerSide) + Commodore1541Geometry.ToSideLogicalBlock(track, sector, tracksPerSide);
    }

    /// <summary>Convertit un bloc logique D71 en adresse de piste, secteur et face.</summary>
    public static Commodore1541Address FromLogicalBlock(int block, int tracksPerSide) => Commodore1541Geometry.FromLogicalBlock(block, tracksPerSide, SideCount);
}
