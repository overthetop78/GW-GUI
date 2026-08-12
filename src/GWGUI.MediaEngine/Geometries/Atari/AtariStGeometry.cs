using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Geometries.Atari;

/// <summary>Décrit et valide une géométrie sectorielle Atari ST.</summary>
public readonly record struct AtariStGeometry
{
    /// <summary>Taille fixe d'un secteur en octets.</summary>
    public const int SectorSize = 512;

    /// <summary>Crée une géométrie aux dimensions strictement positives.</summary>
    public AtariStGeometry(int cylinders, int heads, int sectorsPerTrack)
    {
        if (cylinders <= 0) throw new ArgumentOutOfRangeException(nameof(cylinders), cylinders, "Atari ST cylinders must be positive.");
        if (heads <= 0) throw new ArgumentOutOfRangeException(nameof(heads), heads, "Atari ST heads must be positive.");
        if (sectorsPerTrack <= 0) throw new ArgumentOutOfRangeException(nameof(sectorsPerTrack), sectorsPerTrack, "Atari ST sectors per track must be positive.");
        Cylinders = cylinders;
        Heads = heads;
        SectorsPerTrack = sectorsPerTrack;
        Capacity = checked(cylinders * heads * sectorsPerTrack * SectorSize);
    }

    /// <summary>Nombre de cylindres.</summary>
    public int Cylinders { get; }
    /// <summary>Nombre de faces.</summary>
    public int Heads { get; }
    /// <summary>Nombre de secteurs par piste.</summary>
    public int SectorsPerTrack { get; }
    /// <summary>Capacité exacte en octets.</summary>
    public int Capacity { get; }
    /// <summary>Identifiant Atari ST central calculé depuis la capacité.</summary>
    public string FormatId => DiskImageFormatIds.AtariStFromCapacity(Capacity);

    /// <summary>Résout la géométrie exacte d'un format Atari ST catalogué.</summary>
    public static bool TryFromFormatId(string formatId, out AtariStGeometry geometry)
    {
        geometry = formatId.ToLowerInvariant() switch
        {
            DiskImageFormatIds.AtariSt180 => new(40, 1, 9),
            DiskImageFormatIds.AtariSt360 => new(40, 2, 9),
            DiskImageFormatIds.AtariSt400 => new(80, 1, 10),
            DiskImageFormatIds.AtariSt440 => new(80, 1, 11),
            DiskImageFormatIds.AtariSt720 => new(80, 2, 9),
            DiskImageFormatIds.AtariSt800 => new(80, 2, 10),
            DiskImageFormatIds.AtariSt810 => new(90, 2, 9),
            DiskImageFormatIds.AtariSt880 => new(80, 2, 11),
            DiskImageFormatIds.AtariSt1440 => new(80, 2, 18),
            _ => default
        };
        return geometry.Capacity > 0;
    }
}
