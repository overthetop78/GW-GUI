namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Construit les diagnostics propres aux volumes RT-11.</summary>
public static class Rt11FileSystemExceptions
{
    /// <summary>Crée l'erreur signalant un home block invalide.</summary>
    public static InvalidDataException InvalidHomeBlock(string signature, int directoryBlock) => new($"The RT-11 home block is invalid (signature '{signature}', directory block {directoryBlock}).");
    /// <summary>Construit l'avertissement signalant une paire de blocs absente.</summary>
    public static string MissingBlockPair(int firstBlock) => $"RT-11 directory block pair starting at {firstBlock} is missing.";
    /// <summary>Construit l'avertissement signalant un contenu tronqué.</summary>
    public static string TruncatedContent(int startBlock, int blockCount) => $"RT-11 content starting at block {startBlock} is incomplete for {blockCount} blocks.";
    /// <summary>Construit l'avertissement signalant une taille d'entrée invalide.</summary>
    public static string InvalidEntrySize(int segment, int size) => $"RT-11 directory segment {segment} has invalid entry size {size}.";
    /// <summary>Construit l'avertissement signalant un nom vide.</summary>
    public static string EmptyName(int block) => $"RT-11 entry at block {block} has an empty name.";
}
