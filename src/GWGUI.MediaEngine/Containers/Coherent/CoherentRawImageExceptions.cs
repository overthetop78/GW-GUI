namespace GWGUI.MediaEngine.Containers.Coherent;

/// <summary>Construit les erreurs de lecture des dumps bruts COHERENT.</summary>
internal static class CoherentRawImageExceptions
{
    /// <summary>Crée l'erreur signalant un contenu sans superbloc COHERENT reconnu.</summary>
    public static InvalidDataException ContentNotCoherent(int length) => new($"Le contenu de {length} octet(s) ne contient pas de superbloc COHERENT reconnu.");
    /// <summary>Crée l'erreur signalant une taille non alignée sur les secteurs.</summary>
    public static InvalidDataException NonSectorAlignedLength(int length, int sectorSize) => new($"Le dump COHERENT contient {length} octet(s), une taille non divisible par les secteurs de {sectorSize} octets.");
    /// <summary>Crée l'erreur signalant un nombre de blocs déclaré invalide.</summary>
    public static InvalidDataException InvalidDeclaredBlockCount(int declaredBlocks, int availableBlocks) => new($"Le système de fichiers COHERENT déclare {declaredBlocks} blocs alors que le dump en contient {availableBlocks}.");
    /// <summary>Crée l'erreur signalant un dump dépassant la géométrie.</summary>
    public static InvalidDataException GeometryCapacityExceeded(int availableBlocks, int geometryCapacity) => new($"Le dump COHERENT contient {availableBlocks} blocs alors que la géométrie Commodore 900 en accepte {geometryCapacity}.");
}
