using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Geometries.Commodore;

/// <summary>Décrit les pistes zonées d'une face Commodore 1541.</summary>
public static class Commodore1541Geometry
{
    /// <summary>Taille d'un secteur en octets.</summary>
    public const int SectorSize = 256;
    /// <summary>Nombre standard de pistes.</summary>
    public const int StandardTrackCount = 35;
    /// <summary>Nombre étendu de pistes.</summary>
    public const int ExtendedTrackCount = DiskGeometryConstants.FortyTrackCylinderCount;
    /// <summary>Nombre maximal de secteurs par piste.</summary>
    public const int MaximumSectorsPerTrack = 21;

    /// <summary>Retourne le nombre de secteurs de la piste 1541 indexée à partir de un.</summary>
    public static int SectorsPerTrack(int track) => track switch { >= 1 and <= 17 => MaximumSectorsPerTrack, <= 24 => 19, <= 30 => 18, <= ExtendedTrackCount => 17, _ => throw new ArgumentOutOfRangeException(nameof(track)) };

    /// <summary>Calcule le nombre de blocs d'une face possédant le nombre de pistes indiqué.</summary>
    public static int BlocksPerSide(int tracks) => Enumerable.Range(1, tracks).Sum(SectorsPerTrack);

    /// <summary>Convertit un bloc logique en piste, secteur et face 1541.</summary>
    public static (int Track, int Sector, int Side) FromLogicalBlock(int block, int tracksPerSide, int sides)
    {
        var blocksPerSide = BlocksPerSide(tracksPerSide);
        if (block < 0 || block >= blocksPerSide * sides) throw new ArgumentOutOfRangeException(nameof(block));
        var side = block / blocksPerSide;
        var remaining = block % blocksPerSide;
        for (var track = 1; track <= tracksPerSide; track++)
        {
            var sectors = SectorsPerTrack(track);
            if (remaining < sectors) return (track, remaining, side);
            remaining -= sectors;
        }
        throw new InvalidOperationException();
    }

    /// <summary>Convertit une piste 1541 indexée à un en cylindre du modèle indexé à zéro.</summary>
    public static int ToCylinder(int track) => track - 1;
}
