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

    /// <summary>Crée l'erreur signalant une carte d'allocation MFS tronquée.</summary>
    public static InvalidDataException TruncatedAllocationMap(int allocationCount, int observedLength, int expectedLength) => new($"La carte MFS de {allocationCount} allocations contient {observedLength} octet(s) ; {expectedLength} sont requis.");

    /// <summary>Construit l'avertissement signalant une entrée de répertoire MFS invalide.</summary>
    public static string InvalidDirectoryEntry(int block, int offset) => $"L'entrée MFS du bloc {block} à l'offset {offset} est invalide.";

    /// <summary>Construit l'avertissement signalant une chaîne d'allocation MFS invalide.</summary>
    public static string InvalidAllocationChain(string file, string fork, bool cycle, bool outOfRange, bool prematureEnd) => $"{file} : la chaîne du fork {fork} est invalide (cycle={cycle}, hors-carte={outOfRange}, fin-prématurée={prematureEnd}).";

    /// <summary>Construit l'avertissement signalant des métadonnées de fork incohérentes.</summary>
    public static string InconsistentForkMetadata(string file, string fork, uint length, int firstCluster) => $"{file} : le fork {fork} annonce {length} octet(s) depuis le cluster {firstCluster}, ce qui est incohérent.";
}
