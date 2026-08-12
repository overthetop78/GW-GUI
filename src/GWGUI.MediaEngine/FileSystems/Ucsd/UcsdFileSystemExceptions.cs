using GWGUI.MediaEngine.FileSystems.Definitions;

namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Construit les diagnostics propres aux volumes UCSD.</summary>
public static class UcsdFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant un système non reconnu.</summary>
    public static InvalidDataException UnrecognizedSystem(int block, IEnumerable<int> missingBlocks) => new($"Le répertoire UCSD au bloc {block} n'est pas reconnu ; blocs invalides : {string.Join(", ", missingBlocks)}.");
    /// <summary>Construit l'avertissement signalant des blocs absents ou tronqués.</summary>
    public static string MissingBlocks(string owner, IEnumerable<int> blocks) => $"{owner} : blocs UCSD absents ou tronqués : {string.Join(", ", blocks)}.";
    /// <summary>Construit l'avertissement signalant des blocs de répertoire absents ou tronqués.</summary>
    public static string MissingDirectoryBlocks(IEnumerable<int> blocks) => MissingBlocks(FileSystemDisplayNames.Ucsd, blocks);
    /// <summary>Construit l'avertissement signalant un nom invalide.</summary>
    public static string InvalidName(int entryIndex, string name) => $"L'entrée UCSD {entryIndex} possède un nom invalide '{name}'.";
    /// <summary>Construit l'avertissement signalant une plage invalide.</summary>
    public static string InvalidRange(int entryIndex, string name, int firstBlock, int lastBlock, int totalBlocks) => $"L'entrée UCSD {entryIndex} ('{name}') utilise la plage invalide [{firstBlock}, {lastBlock}) sur {totalBlocks} blocs.";
    /// <summary>Construit l'avertissement signalant un chevauchement de blocs.</summary>
    public static string Overlap(int entryIndex, string name, IEnumerable<int> blocks) => $"L'entrée UCSD {entryIndex} ('{name}') chevauche les blocs {string.Join(", ", blocks)}.";
    /// <summary>Construit l'avertissement signalant une différence de nombre d'entrées.</summary>
    public static string DeclaredFileCountMismatch(int declared, int valid) => $"Le répertoire UCSD annonce {declared} fichier(s), mais {valid} entrée(s) valide(s) ont été trouvées.";
    /// <summary>Construit l'avertissement signalant un nombre d'octets final invalide.</summary>
    public static string InvalidLastBlockByteCount(string name, int observed) => $"{name} : le dernier bloc UCSD annonce {observed} octets.";
}
