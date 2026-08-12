namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Construit les avertissements techniques produits par le lecteur Commodore DOS.</summary>
internal static class CommodoreDosWarnings
{
    /// <summary>Signale un cycle de répertoire.</summary>
    public static string DirectoryCycle(int track, int sector) => $"La chaîne de répertoire CBM DOS est cyclique en {track}/{sector}.";
    /// <summary>Signale un secteur de répertoire absent.</summary>
    public static string DirectorySectorMissing(int track, int sector) => $"Le secteur de répertoire CBM DOS {track}/{sector} est absent.";
    /// <summary>Signale un secteur de répertoire tronqué.</summary>
    public static string DirectorySectorTruncated(int track, int sector, int length) => $"Le secteur de répertoire CBM DOS {track}/{sector} ne contient que {length} octets.";
    /// <summary>Signale une coordonnée de répertoire invalide.</summary>
    public static string DirectoryCoordinateInvalid(int track, int sector) => $"La coordonnée de répertoire CBM DOS {track}/{sector} est invalide.";
    /// <summary>Signale un problème de lecture de fichier.</summary>
    public static string FileReadFailure(string name, string reason) => $"{name}: {reason}";
    /// <summary>Signale que la chaîne dépasse la capacité annoncée.</summary>
    public static string CapacityExceeded(string name) => $"{name}: la chaîne CBM DOS dépasse la capacité de l'image.";
    /// <summary>Signale un BAM requis absent.</summary>
    public static string BamMissing(int track, int sector) => $"Le BAM CBM DOS requis en {track}/{sector} est absent.";
    /// <summary>Signale un BAM tronqué.</summary>
    public static string BamTruncated(int track, int sector, int length) => $"Le BAM CBM DOS {track}/{sector} est tronqué à {length} octets.";
}
