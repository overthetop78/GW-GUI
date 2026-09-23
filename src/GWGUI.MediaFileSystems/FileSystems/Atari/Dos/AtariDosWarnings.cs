namespace GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

/// <summary>Construit les avertissements techniques Atari DOS.</summary>
public static class AtariDosWarnings
{
    /// <summary>Signale une entrée de catalogue qui n'a pas été finalisée.</summary>
    public static string OpenForOutputEntryIgnored(string name, int sector, int slot) => $"Directory sector {sector}, entry {slot + 1} ('{name}') is still open for output and was ignored.";
    /// <summary>Signale un secteur de données tronqué.</summary>
    public static string TruncatedSector(string name, int sector) => $"Data sector {sector} for '{name}' is too short to contain its link.";
    /// <summary>Signale une fin de chaîne rencontrée avant l'étendue déclarée.</summary>
    public static string PrematureEnd(string name, int expected, int observed) => $"File '{name}' declares {expected} sectors, but its chain ends after {observed}.";
    /// <summary>Signale une entrée vide qui référence pourtant des données.</summary>
    public static string UnexpectedDataForEmptyFile(string name, int firstSector) => $"Empty file '{name}' unexpectedly references sector {firstSector}.";
    /// <summary>Signale qu'une entrée délimite une vue avant la fin de la chaîne sous-jacente.</summary>
    public static string ContinuationAfterDeclaredExtent(string name, int declaredSectors, int nextSector) => $"File '{name}' is limited to {declaredSectors} directory sectors while the underlying chain continues at sector {nextSector}.";
    /// <summary>Signale une vue dont les secteurs sont volontairement partagés avec une autre entrée.</summary>
    public static string SharedSectorView(string name, string ownerName, int firstSector, int sectorCount) => $"File '{name}' shares {sectorCount} sectors from sector {firstSector} with '{ownerName}'.";
    /// <summary>Signale une longueur utile invalide.</summary>
    public static string InvalidUsedLength(string name, int sector, int used, int available) => $"Data sector {sector} for '{name}' declares {used} bytes but only {available} are available.";
}
