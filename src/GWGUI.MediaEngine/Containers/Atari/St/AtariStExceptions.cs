namespace GWGUI.MediaEngine.Containers.Atari.St;

/// <summary>Construit les erreurs de lecture et d'écriture des images Atari ST brutes.</summary>
internal static class AtariStExceptions
{
    /// <summary>Crée l'erreur signalant une longueur vide ou non sectorielle.</summary>
    public static InvalidDataException InvalidLength(int length, int sectorSize) => new($"Atari ST image contains {length} bytes; expected a positive multiple of {sectorSize} bytes.");
    /// <summary>Crée l'erreur signalant qu'aucune géométrie ne correspond à la longueur et au BPB.</summary>
    public static InvalidDataException GeometryNotDetected(int length) => new($"Atari ST geometry could not be determined for an image containing {length} bytes.");
    /// <summary>Crée l'erreur signalant une longueur incompatible avec la géométrie retenue.</summary>
    public static InvalidDataException IncompatibleGeometry(int length, int capacity, int cylinders, int heads, int sectorsPerTrack) => new($"Atari ST image length {length} does not match capacity {capacity} for {cylinders} cylinders, {heads} heads and {sectorsPerTrack} sectors per track.");
    /// <summary>Crée l'erreur signalant une image sectorielle d'une autre famille.</summary>
    public static InvalidDataException UnsupportedSectorImage(string formatId, int sectorSize) => new($"Sector image '{formatId}' is not an Atari ST image with {sectorSize}-byte blocks.");
    /// <summary>Crée l'erreur signalant un bloc logique mal dimensionné.</summary>
    public static InvalidDataException InvalidLogicalSectorSize(int logicalSector, int actual, int expected) => new($"Atari ST logical sector {logicalSector} contains {actual} bytes; expected {expected}.");
}
