namespace GWGUI.MediaEngine.Recognition.Amstrad;

/// <summary>Décrit les champs validés de la spécification disque Amstrad PCW.</summary>
internal sealed record AmstradCpmDiskSpecification(byte Tracks, byte SectorsPerTrack, int SectorSize, byte ReservedTracks, int AllocationSize, byte DirectoryBlocks)
{
    /// <summary>Taille minimale contenant la spécification.</summary>
    public const int MinimumLength = 512;
    /// <summary>Offset du nombre de pistes.</summary>
    public const int TracksOffset = 2;
    /// <summary>Offset du nombre de secteurs par piste.</summary>
    public const int SectorsPerTrackOffset = 3;
    /// <summary>Offset du code de taille sectorielle.</summary>
    public const int SectorSizeCodeOffset = 4;
    /// <summary>Offset du nombre de pistes réservées.</summary>
    public const int ReservedTracksOffset = 5;
    /// <summary>Offset du code de taille d'allocation.</summary>
    public const int AllocationSizeCodeOffset = 6;
    /// <summary>Offset du nombre de blocs de répertoire.</summary>
    public const int DirectoryBlocksOffset = 7;
    /// <summary>Nombre maximal de pistes.</summary>
    public const int MaximumTracks = 96;
    /// <summary>Nombre maximal de secteurs par piste.</summary>
    public const int MaximumSectorsPerTrack = 64;
    /// <summary>Nombre maximal de pistes réservées.</summary>
    public const int MaximumReservedTracks = 8;
    /// <summary>Nombre maximal de blocs de répertoire.</summary>
    public const int MaximumDirectoryBlocks = 16;
    /// <summary>Taille sectorielle minimale.</summary>
    public const int MinimumSectorSize = 128;
    /// <summary>Taille sectorielle maximale.</summary>
    public const int MaximumSectorSize = 4096;
    /// <summary>Taille d'allocation minimale.</summary>
    public const int MinimumAllocationSize = 512;
    /// <summary>Taille d'allocation maximale.</summary>
    public const int MaximumAllocationSize = 16384;
    /// <summary>Masque du code exponentiel de taille.</summary>
    public const byte SizeCodeMask = 7;

    /// <summary>Tente de lire et valider une spécification disque PCW.</summary>
    public static bool TryParse(ReadOnlySpan<byte> bytes, out AmstradCpmDiskSpecification specification)
    {
        specification = null!;
        if (bytes.Length < MinimumLength) return false;
        var tracks = bytes[TracksOffset];
        var sectorsPerTrack = bytes[SectorsPerTrackOffset];
        var sectorSize = MinimumSectorSize << (bytes[SectorSizeCodeOffset] & SizeCodeMask);
        var reservedTracks = bytes[ReservedTracksOffset];
        var allocationSize = MinimumSectorSize << (bytes[AllocationSizeCodeOffset] & SizeCodeMask);
        var directoryBlocks = bytes[DirectoryBlocksOffset];
        if (tracks is 0 or > MaximumTracks || sectorsPerTrack is 0 or > MaximumSectorsPerTrack || sectorSize is < MinimumSectorSize or > MaximumSectorSize || reservedTracks > MaximumReservedTracks || allocationSize is < MinimumAllocationSize or > MaximumAllocationSize || directoryBlocks is 0 or > MaximumDirectoryBlocks) return false;
        specification = new(tracks, sectorsPerTrack, sectorSize, reservedTracks, allocationSize, directoryBlocks);
        return true;
    }
}
