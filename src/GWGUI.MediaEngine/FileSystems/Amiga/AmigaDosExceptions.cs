namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Construit les erreurs et avertissements propres au lecteur AmigaDOS.</summary>
internal static class AmigaDosExceptions
{
    /// <summary>Crée l'erreur signalant un boot AmigaDOS non reconnu.</summary>
    public static InvalidDataException UnsupportedBoot() => new("The image does not contain a supported AmigaDOS boot block.");
    /// <summary>Crée l'erreur signalant une variante de boot non prise en charge.</summary>
    /// <param name="variant">Variante observée.</param>
    /// <returns>Erreur de données correspondante.</returns>
    public static InvalidDataException UnsupportedBootVariant(byte variant) => new($"The image contains unsupported AmigaDOS boot variant {variant}.");

    /// <summary>Crée l'erreur signalant un bloc racine invalide.</summary>
    /// <param name="blockNumber">Numéro du bloc racine observé.</param>
    /// <returns>Erreur de données correspondante.</returns>
    public static InvalidDataException InvalidRootBlock(int blockNumber) => new($"The AmigaDOS root block {blockNumber} is invalid.");

    /// <summary>Crée l'erreur signalant un bloc d'extension invalide.</summary>
    /// <param name="blockNumber">Numéro du bloc d'extension.</param>
    /// <returns>Erreur de données correspondante.</returns>
    public static InvalidDataException InvalidExtensionBlock(int blockNumber) => new($"Invalid file extension block {blockNumber}.");

    /// <summary>Crée l'erreur signalant un bloc AmigaDOS absent.</summary>
    /// <param name="description">Description du bloc attendu.</param>
    /// <param name="blockNumber">Numéro du bloc attendu.</param>
    /// <returns>Erreur de données correspondante.</returns>
    public static InvalidDataException MissingBlock(string description, int blockNumber) => new($"The AmigaDOS {description} ({blockNumber}) is missing.");

    /// <summary>Construit l'avertissement signalant le dépassement de profondeur d'un répertoire.</summary>
    /// <param name="depth">Profondeur observée.</param>
    /// <returns>Avertissement correspondant.</returns>
    public static string DirectoryDepthExceeded(int depth) => $"The directory nesting limit was reached at depth {depth}.";
}
