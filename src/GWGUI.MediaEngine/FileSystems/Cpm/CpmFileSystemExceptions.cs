namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Construit les erreurs propres aux lecteurs CP/M.</summary>
internal static class CpmFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant l'absence de disposition pour un format.</summary>
    public static InvalidDataException MissingLayout(string formatId) => new($"The CP/M disk layout is not supported for format {formatId}.");
    /// <summary>Crée l'erreur signalant un répertoire non reconnu.</summary>
    public static InvalidDataException UnsupportedDirectory(string formatId) => new($"The image does not contain a supported CP/M directory for format {formatId}.");
    /// <summary>Construit l'avertissement signalant une allocation hors limites.</summary>
    public static string AllocationOutsideImage(string name, int block, int offset, int imageLength) => $"{name}: CP/M allocation block {block} at offset {offset} is outside the {imageLength}-byte image.";
    /// <summary>Construit l'avertissement signalant une allocation traversant un bloc logique absent.</summary>
    public static string MissingLogicalBlock(string name, int allocation) => $"{name}: CP/M allocation block {allocation} crosses a missing logical block.";
    /// <summary>Construit l'avertissement signalant une allocation traversant un bloc logique tronqué.</summary>
    public static string TruncatedLogicalBlock(string name, int allocation) => $"{name}: CP/M allocation block {allocation} crosses a truncated logical block.";
    /// <summary>Construit l'avertissement signalant une allocation dupliquée.</summary>
    public static string DuplicateAllocation(string name, int allocation) => $"{name}: CP/M allocation block {allocation} is referenced more than once.";
}
