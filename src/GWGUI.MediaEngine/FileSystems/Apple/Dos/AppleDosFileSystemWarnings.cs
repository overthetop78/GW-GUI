namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Construit les avertissements techniques Apple DOS.</summary>
public static class AppleDosFileSystemWarnings
{
    /// <summary>Signale une liste T/S cyclique.</summary>
    public static string CyclicList(string name, int track, int sector) => $"The track/sector list for '{name}' is cyclic at {track}/{sector}.";
    /// <summary>Signale une liste T/S absente.</summary>
    public static string MissingList(string name, int track, int sector) => $"The track/sector list for '{name}' is missing at {track}/{sector}.";
    /// <summary>Signale une coordonnée hors géométrie.</summary>
    public static string InvalidAddress(string name, int track, int sector) => $"The Apple DOS address {track}/{sector} for '{name}' is outside the disk geometry.";
    /// <summary>Signale un secteur de données absent.</summary>
    public static string MissingData(string name, int track, int sector) => $"The data sector {track}/{sector} for '{name}' is missing.";
    /// <summary>Signale un nombre de secteurs incohérent.</summary>
    public static string InconsistentSize(string name, int declared, int observed) => $"File '{name}' declares {declared} sectors but {observed} sectors were traversed.";
}
