using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Geometries.Ibm;

/// <summary>Catalogue immuable des dix géométries IBM PC reconnues par capacité.</summary>
public static class IbmPcGeometryCatalog
{
    /// <summary>Nombre de secteurs d'une piste à huit secteurs.</summary>
    public const int Sectors8 = 8;
    /// <summary>Nombre de secteurs d'une piste à neuf secteurs.</summary>
    public const int Sectors9 = 9;
    /// <summary>Nombre de secteurs d'une piste à dix secteurs.</summary>
    public const int Sectors10 = 10;
    /// <summary>Nombre de secteurs d'une piste à quinze secteurs.</summary>
    public const int Sectors15 = 15;
    /// <summary>Nombre de secteurs d'une piste à dix-huit secteurs.</summary>
    public const int Sectors18 = 18;
    /// <summary>Nombre de secteurs d'une piste à vingt-et-un secteurs.</summary>
    public const int Sectors21 = 21;
    /// <summary>Nombre de secteurs d'une piste à trente-six secteurs.</summary>
    public const int Sectors36 = 36;

    /// <summary>Profils IBM pris en charge, y compris le profil DMF explicitement distinct.</summary>
    public static IReadOnlyList<IbmPcGeometry> All { get; } = Array.AsReadOnly<IbmPcGeometry>(
    [
        new(DiskImageFormatIds.Ibm160, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, Sectors8),
        new(DiskImageFormatIds.Ibm180, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.SingleSidedHeadCount, Sectors9),
        new(DiskImageFormatIds.Ibm320, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Sectors8),
        new(DiskImageFormatIds.Ibm360, DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Sectors9),
        new(DiskImageFormatIds.Ibm720, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Sectors9),
        new(DiskImageFormatIds.Ibm800, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Sectors10),
        new(DiskImageFormatIds.Ibm1200, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Sectors15),
        new(DiskImageFormatIds.Ibm1440, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Sectors18),
        new(DiskImageFormatIds.Ibm1680, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Sectors21),
        new(DiskImageFormatIds.IbmDmf, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Sectors21),
        new(DiskImageFormatIds.Ibm2880, DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, Sectors36)
    ]);

    /// <summary>Catalogue des profils indexés par identifiant technique.</summary>
    public static IReadOnlyDictionary<string, IbmPcGeometry> ByFormatId { get; } = new System.Collections.ObjectModel.ReadOnlyDictionary<string, IbmPcGeometry>(All.ToDictionary(geometry => geometry.FormatId, StringComparer.OrdinalIgnoreCase));

    /// <summary>Catalogue des géométries standards indexées par capacité en octets.</summary>
    public static IReadOnlyDictionary<int, IbmPcGeometry> ByCapacity { get; } = new System.Collections.ObjectModel.ReadOnlyDictionary<int, IbmPcGeometry>(All.Where(geometry => geometry.FormatId != DiskImageFormatIds.IbmDmf).ToDictionary(geometry => geometry.Capacity));

    /// <summary>Recherche une géométrie par capacité exacte.</summary>
    public static bool TryFromCapacity(int capacity, out IbmPcGeometry geometry) => ByCapacity.TryGetValue(capacity, out geometry);
    /// <summary>Recherche le profil explicitement sélectionné par son identifiant.</summary>
    public static bool TryFromFormatId(string formatId, out IbmPcGeometry geometry) => ByFormatId.TryGetValue(formatId, out geometry);
    /// <summary>Recherche une géométrie depuis un descripteur FAT historique.</summary>
    public static bool TryFromMediaDescriptor(byte descriptor, out IbmPcGeometry geometry)
    {
        var capacity = (FatMediaDescriptor)descriptor switch { FatMediaDescriptor.Ibm160 => 160, FatMediaDescriptor.Ibm180 => 180, FatMediaDescriptor.Ibm320 => 320, FatMediaDescriptor.Ibm360 => 360, _ => 0 };
        if (capacity != 0) return TryFromCapacity(capacity * DataSizeConstants.BytesPerKibibyte, out geometry);
        geometry = default;
        return false;
    }
    /// <summary>Retourne l'identifiant d'une géométrie connue ou un identifiant déduit de sa capacité.</summary>
    public static string FormatIdForGeometry(int cylinders, int heads, int sectorsPerTrack, int sectorSize = FatBootSectorLayout.SectorSize)
    {
        var capacity = checked(cylinders * heads * sectorsPerTrack * sectorSize);
        return TryFromCapacity(capacity, out var geometry) ? geometry.FormatId : DiskImageFormatIds.IbmFromCapacity(capacity);
    }
}
