using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Geometries.Commodore;

/// <summary>Décrit les pistes zonées d'une face Commodore 1541.</summary>
public static class Commodore1541Geometry
{
    /// <summary>Première piste valide.</summary>
    public const int FirstTrack = 1;
    /// <summary>Taille d'un secteur en octets.</summary>
    public const int SectorSize = 256;
    /// <summary>Nombre standard de pistes.</summary>
    public const int StandardTrackCount = 35;
    /// <summary>Nombre étendu de pistes.</summary>
    public const int ExtendedTrackCount = DiskGeometryConstants.FortyTrackCylinderCount;
    /// <summary>Nombre maximal de secteurs par piste.</summary>
    public const int MaximumSectorsPerTrack = 21;
    /// <summary>Dernière piste de la première zone.</summary>
    public const int Zone1EndTrack = 17;
    /// <summary>Dernière piste de la deuxième zone.</summary>
    public const int Zone2EndTrack = 24;
    /// <summary>Dernière piste de la troisième zone.</summary>
    public const int Zone3EndTrack = 30;
    /// <summary>Nombre de secteurs dans la deuxième zone.</summary>
    public const int Zone2SectorCount = 19;
    /// <summary>Nombre de secteurs dans la troisième zone.</summary>
    public const int Zone3SectorCount = 18;
    /// <summary>Nombre de secteurs dans la quatrième zone.</summary>
    public const int Zone4SectorCount = 17;
    /// <summary>Nombres de pistes acceptés par les conteneurs 1541.</summary>
    public static IReadOnlyList<int> SupportedTrackCounts { get; } = Array.AsReadOnly(new[] { StandardTrackCount, ExtendedTrackCount });
    /// <summary>Préfixe du premier bloc de chaque piste, indexé par numéro de piste.</summary>
    private static IReadOnlyList<int> TrackBlockOffsets { get; } = CreateTrackBlockOffsets();

    /// <summary>Retourne le nombre de secteurs de la piste 1541 indexée à partir de un.</summary>
    public static int SectorsPerTrack(int track)
    {
        if (track is < FirstTrack or > ExtendedTrackCount) throw CommodoreGeometryExceptions.InvalidTrack(track, FirstTrack, ExtendedTrackCount);
        return track switch { <= Zone1EndTrack => MaximumSectorsPerTrack, <= Zone2EndTrack => Zone2SectorCount, <= Zone3EndTrack => Zone3SectorCount, _ => Zone4SectorCount };
    }

    /// <summary>Calcule le nombre de blocs d'une face possédant le nombre de pistes indiqué.</summary>
    public static int BlocksPerSide(int tracks)
    {
        ValidateTrackCount(tracks);
        return TrackBlockOffsets[tracks + 1];
    }

    /// <summary>Convertit une piste et un secteur d'une face en bloc logique de cette face.</summary>
    public static int ToSideLogicalBlock(int track, int sector, int tracks)
    {
        ValidateTrackCount(tracks);
        if (track is < FirstTrack || track > tracks) throw CommodoreGeometryExceptions.InvalidTrack(track, FirstTrack, tracks);
        var sectorCount = SectorsPerTrack(track);
        if (sector is < 0 || sector >= sectorCount) throw CommodoreGeometryExceptions.InvalidSector(sector, 0, sectorCount - 1);
        return TrackBlockOffsets[track] + sector;
    }

    /// <summary>Convertit un bloc logique en piste, secteur et face 1541.</summary>
    public static Commodore1541Address FromLogicalBlock(int block, int tracksPerSide, int sides)
    {
        var blocksPerSide = BlocksPerSide(tracksPerSide);
        if (sides <= 0) throw CommodoreGeometryExceptions.InvalidSide(sides, sides);
        if (block < 0 || block >= blocksPerSide * sides) throw CommodoreGeometryExceptions.InvalidLogicalBlock(block, blocksPerSide * sides);
        var side = block / blocksPerSide;
        var remaining = block % blocksPerSide;
        for (var track = 1; track <= tracksPerSide; track++)
        {
            var sectors = SectorsPerTrack(track);
            if (remaining < sectors) return new(track, remaining, side);
            remaining -= sectors;
        }
        throw new InvalidOperationException("La conversion inverse 1541 a épuisé les pistes après validation du bloc logique.");
    }

    /// <summary>Convertit une piste 1541 indexée à un en cylindre du modèle indexé à zéro.</summary>
    public static int ToCylinder(int track) => track - 1;

    /// <summary>Valide le nombre de pistes pris en charge.</summary>
    private static void ValidateTrackCount(int tracks)
    {
        if (!SupportedTrackCounts.Contains(tracks)) throw CommodoreGeometryExceptions.InvalidTrackCount(tracks, SupportedTrackCounts);
    }

    /// <summary>Calcule une fois le préfixe de blocs de chaque piste.</summary>
    private static IReadOnlyList<int> CreateTrackBlockOffsets()
    {
        var offsets = new int[ExtendedTrackCount + 2];
        for (var track = FirstTrack; track <= ExtendedTrackCount; track++) offsets[track + 1] = offsets[track] + SectorsPerTrack(track);
        return Array.AsReadOnly(offsets);
    }
}
