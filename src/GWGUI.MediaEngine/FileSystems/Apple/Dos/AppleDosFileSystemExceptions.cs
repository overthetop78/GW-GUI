namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Construit les erreurs et avertissements propres à Apple DOS.</summary>
internal static class AppleDosFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant un catalogue absent ou invalide.</summary>
    public static InvalidDataException MissingCatalog(int track, int sector) => new($"The Apple DOS catalog referenced by VTOC at T{track} S{sector} is missing or invalid.");
    /// <summary>Construit l'avertissement signalant une liste T/S cyclique.</summary>
    public static string CyclicTrackSectorList(string name, int track, int sector) => $"{name}: T/S list T{track} S{sector} is cyclic.";
    /// <summary>Construit l'avertissement signalant une liste T/S absente.</summary>
    public static string MissingTrackSectorList(string name, int track, int sector) => $"{name}: T/S list T{track} S{sector} is missing.";
    /// <summary>Construit l'avertissement signalant un secteur de données absent.</summary>
    public static string MissingDataSector(string name, int track, int sector) => $"{name}: data sector T{track} S{sector} is missing.";
    /// <summary>Construit l'avertissement signalant un secteur de catalogue absent ou cyclique.</summary>
    public static string InvalidCatalogChain(int track, int sector) => $"Catalog sector T{track} S{sector} is missing or cyclic.";
    /// <summary>Construit l'avertissement signalant une taille de catalogue incohérente.</summary>
    public static string InconsistentCatalogSize(string name) => $"{name}: catalog size is inconsistent.";
}
