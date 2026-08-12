namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Construit les erreurs et avertissements propres à ProDOS.</summary>
internal static class ProDosFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant un en-tête de volume non reconnu.</summary>
    public static InvalidDataException UnsupportedVolume(int block, byte observed) => new($"The ProDOS/SOS volume header in block {block} is not recognized (observed ${observed:X2}).");
    /// <summary>Construit l'avertissement signalant une profondeur excessive.</summary>
    public static string DirectoryDepthExceeded(int depth, int block) => $"The ProDOS directory at block {block} exceeds the nesting limit at depth {depth}.";
    /// <summary>Construit l'avertissement signalant un bloc de répertoire absent ou cyclique.</summary>
    public static string InvalidDirectoryBlock(int block) => $"Directory block {block} is missing or cyclic.";
    /// <summary>Construit l'avertissement signalant un bloc d'index absent.</summary>
    public static string MissingIndexBlock(string name, int block) => $"{name}: index block {block} is missing.";
    /// <summary>Construit l'avertissement signalant un bloc maître absent.</summary>
    public static string MissingMasterIndexBlock(string name, int block) => $"{name}: master index block {block} is missing.";
    /// <summary>Construit l'avertissement signalant un bloc de données absent.</summary>
    public static string MissingDataBlock(string name, int block) => $"{name}: data block {block} is missing.";
}
