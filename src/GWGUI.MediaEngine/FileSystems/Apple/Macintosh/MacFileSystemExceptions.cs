namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh;

/// <summary>Construit les erreurs et avertissements communs aux systèmes de fichiers Macintosh.</summary>
internal static class MacFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant qu'une signature de volume ne correspond pas au système attendu.</summary>
    public static InvalidDataException InvalidVolume(string system, ushort signature) => new($"L'image ne contient pas de volume {system} ; signature observée : 0x{signature:X4}.");

    /// <summary>Construit l'avertissement signalant un bloc absent dans un fork de fichier.</summary>
    public static string MissingBlock(string file, string fork, int block) => $"{file} : le bloc {block} du fork {fork} est absent.";

    /// <summary>Construit l'avertissement signalant que le contenu obtenu d'un fork est incomplet.</summary>
    public static string IncompleteData(string file, string fork, long observedLength, long expectedLength) => $"{file} : le fork {fork} contient {observedLength} octet(s) sur les {expectedLength} attendus.";

    /// <summary>Construit l'avertissement signalant un cycle dans l'arborescence des dossiers.</summary>
    public static string DirectoryCycle(uint directoryId) => $"Le dossier Macintosh {directoryId} forme un cycle dans le catalogue.";

    /// <summary>Crée l'erreur signalant une taille d'allocation nulle ou non alignée sur les secteurs.</summary>
    public static InvalidDataException InvalidAllocationSize(string system, uint allocationSize, int sectorSize) => new($"La taille d'allocation {allocationSize} du volume {system} n'est pas un multiple non nul de {sectorSize} octets.");

    /// <summary>Crée l'erreur signalant un catalogue ou un nœud structurellement tronqué.</summary>
    public static InvalidDataException TruncatedCatalog(string system, int observedLength, int expectedLength) => new($"Le catalogue {system} contient {observedLength} octet(s) ; {expectedLength} sont requis.");

    /// <summary>Construit l'avertissement signalant qu'aucun record de catalogue n'est lisible.</summary>
    public static string NoReadableCatalogRecord(string system) => $"Le catalogue {system} ne contient aucun record lisible.";

    /// <summary>Construit l'avertissement signalant un nombre de blocs libres supérieur au nombre total.</summary>
    public static string InvalidFreeAllocationCount(string system, int freeCount, int totalCount) => $"Le volume {system} annonce {freeCount} blocs libres pour {totalCount} blocs d'allocation.";
}
