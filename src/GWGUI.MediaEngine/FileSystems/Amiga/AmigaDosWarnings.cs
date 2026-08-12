namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Construit les avertissements techniques produits pendant une lecture AmigaDOS.</summary>
public static class AmigaDosWarnings
{
    /// <summary>Signale le checksum invalide d'un bloc racine.</summary>
    public static string InvalidRootChecksum(int block) => $"Root block {block} has an invalid checksum.";
    /// <summary>Signale une chaîne de répertoire cyclique ou invalide.</summary>
    public static string InvalidDirectoryChain(int block) => $"Invalid or cyclic directory chain at block {block}.";
    /// <summary>Signale un bloc d'entrée absent.</summary>
    public static string MissingDirectoryEntry(int block) => $"Directory entry block {block} is missing.";
    /// <summary>Signale un bloc de données absent.</summary>
    public static string MissingFileData(int block) => $"File data block {block} is missing.";
    /// <summary>Signale le type inattendu d'un bloc de données OFS.</summary>
    public static string UnexpectedOfsDataType(int block, int type) => $"OFS data block {block} has unexpected type {type}.";
    /// <summary>Signale le checksum invalide d'un bloc de données OFS.</summary>
    public static string InvalidOfsDataChecksum(int block) => $"OFS data block {block} has an invalid checksum.";
    /// <summary>Signale un bloc d'extension invalide, cyclique ou absent.</summary>
    public static string InvalidExtension(int block) => $"File extension block {block} is invalid, cyclic or missing.";
    /// <summary>Signale le checksum invalide d'un bloc d'extension.</summary>
    public static string InvalidExtensionChecksum(int block) => $"File extension block {block} has an invalid checksum.";
    /// <summary>Signale un contenu plus court que la taille déclarée.</summary>
    public static string TruncatedFile(int expected, int actual) => $"File content is truncated: expected {expected} bytes but read {actual}.";
    /// <summary>Signale un bloc de bitmap absent.</summary>
    public static string MissingBitmap(int block) => $"Bitmap block {block} is missing.";
    /// <summary>Signale le checksum invalide d'un bitmap.</summary>
    public static string InvalidBitmapChecksum(int block) => $"Bitmap block {block} has an invalid checksum.";
    /// <summary>Signale la profondeur maximale d'un répertoire.</summary>
    public static string DirectoryDepthExceeded(int depth) => $"The directory nesting limit was reached at depth {depth}.";
}
