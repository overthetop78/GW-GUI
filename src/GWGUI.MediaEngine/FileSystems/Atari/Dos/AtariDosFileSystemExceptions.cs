namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Construit les erreurs et avertissements propres à Atari DOS.</summary>
internal static class AtariDosFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant un répertoire non reconnu.</summary>
    public static InvalidDataException UnsupportedDirectory(string formatId, int sectorSize) => new($"The {formatId} image with {sectorSize}-byte sectors does not contain a supported Atari DOS directory.");
    /// <summary>Construit l'avertissement signalant un secteur de catalogue absent.</summary>
    public static string MissingDirectorySector(int sector) => $"Directory sector {sector} is missing.";
    /// <summary>Construit l'avertissement signalant un secteur de données absent.</summary>
    public static string MissingDataSector(string name, int sector) => $"{name}: Atari DOS data sector {sector} is missing.";
    /// <summary>Construit l'avertissement signalant une chaîne cyclique.</summary>
    public static string CyclicDataChain(string name, int sector) => $"{name}: Atari DOS data chain is cyclic at sector {sector}.";
    /// <summary>Construit l'avertissement signalant un propriétaire incohérent.</summary>
    public static string InconsistentOwner(string name, int sector, int expected, int observed) => $"{name}: sector {sector} belongs to file {observed}, expected {expected}.";
}
