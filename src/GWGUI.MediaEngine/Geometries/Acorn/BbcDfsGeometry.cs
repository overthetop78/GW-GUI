using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Geometries.Acorn;

/// <summary>Décrit les quatre géométries BBC DFS SSD et DSD prises en charge.</summary>
/// <param name="FormatId">Identifiant central du format.</param>
/// <param name="Cylinders">Nombre de cylindres.</param>
/// <param name="Heads">Nombre de faces.</param>
public sealed record BbcDfsGeometry(string FormatId, int Cylinders, int Heads)
{
    /// <summary>Nom de la famille utilisé dans les diagnostics techniques.</summary>
    public const string FormatFamilyName = "BBC DFS";
    /// <summary>Taille d'un secteur DFS en octets.</summary>
    public const int SectorSize = 256;
    /// <summary>Nombre de secteurs par piste.</summary>
    public const int SectorsPerTrack = 10;
    /// <summary>Taille d'une piste en octets.</summary>
    public const int TrackSize = SectorSize * SectorsPerTrack;
    /// <summary>Géométrie SSD 40 pistes.</summary>
    public static BbcDfsGeometry Ssd40 { get; } = new(DiskImageFormatIds.AcornDfsSingleSided, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount);
    /// <summary>Géométrie SSD 80 pistes.</summary>
    public static BbcDfsGeometry Ssd80 { get; } = new(DiskImageFormatIds.AcornDfsSingleSided80, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount);
    /// <summary>Géométrie DSD 40 pistes.</summary>
    public static BbcDfsGeometry Dsd40 { get; } = new(DiskImageFormatIds.AcornDfsDoubleSided, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount);
    /// <summary>Géométrie DSD 80 pistes.</summary>
    public static BbcDfsGeometry Dsd80 { get; } = new(DiskImageFormatIds.AcornDfsDoubleSided80, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount);
    /// <summary>Catalogue non modifiable des quatre géométries prises en charge.</summary>
    public static IReadOnlyList<BbcDfsGeometry> Supported { get; } = Array.AsReadOnly([Ssd40, Ssd80, Dsd40, Dsd80]);
    /// <summary>Nombre total de blocs.</summary>
    public int BlockCount => checked(Cylinders * Heads * SectorsPerTrack);
    /// <summary>Capacité exacte en octets.</summary>
    public int Capacity => checked(BlockCount * SectorSize);

    /// <summary>Retourne la géométrie correspondant exactement au type de conteneur et à sa capacité.</summary>
    public static BbcDfsGeometry? Find(int heads, int capacity) => Supported.SingleOrDefault(geometry => geometry.Heads == heads && geometry.Capacity == capacity);

    /// <summary>Retourne la géométrie possédant l'identifiant central demandé.</summary>
    public static BbcDfsGeometry? FindByFormatId(string formatId) => Supported.SingleOrDefault(geometry => geometry.FormatId.Equals(formatId, StringComparison.OrdinalIgnoreCase));

    /// <summary>Détermine la géométrie standard minimale pouvant contenir les cylindres et faces observés.</summary>
    public static BbcDfsGeometry FromObservedGeometry(int cylinders, int heads) => (cylinders > DiskGeometryConstants.FortyTrackCylinderCount, heads > DiskGeometryConstants.SingleSidedHeadCount) switch
    {
        (false, false) => Ssd40,
        (true, false) => Ssd80,
        (false, true) => Dsd40,
        (true, true) => Dsd80
    };
}
