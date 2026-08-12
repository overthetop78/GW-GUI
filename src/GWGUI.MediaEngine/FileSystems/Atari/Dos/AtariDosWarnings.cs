namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Construit les avertissements techniques Atari DOS.</summary>
public static class AtariDosWarnings
{
    /// <summary>Signale un secteur de données tronqué.</summary>
    public static string TruncatedSector(string name, int sector) => $"Data sector {sector} for '{name}' is too short to contain its link.";
    /// <summary>Signale un compteur de secteurs incohérent.</summary>
    public static string InconsistentCount(string name, int expected, int observed, int next) => $"File '{name}' declares {expected} sectors, traversed {observed}, and ends with link {next}.";
    /// <summary>Signale une longueur utile invalide.</summary>
    public static string InvalidUsedLength(string name, int sector, int used, int available) => $"Data sector {sector} for '{name}' declares {used} bytes but only {available} are available.";
}
