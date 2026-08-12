namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Construit les erreurs et avertissements propres à ProDOS.</summary>
internal static class ProDosFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant un en-tête de volume non reconnu.</summary>
    public static InvalidDataException UnsupportedVolume(int block, byte observed) => new($"L'en-tête ProDOS/SOS du bloc {block} n'est pas reconnu (valeur observée ${observed:X2}).");
    /// <summary>Construit l'avertissement signalant une profondeur excessive.</summary>
    public static string DirectoryDepthExceeded(int depth, int block) => $"Le répertoire ProDOS du bloc {block} dépasse la profondeur maximale au niveau {depth}.";
    /// <summary>Construit l'avertissement signalant un bloc de répertoire absent ou cyclique.</summary>
    public static string InvalidDirectoryBlock(int block, bool cyclic, bool reused) => $"Le bloc de répertoire {block} est invalide (cycle={cyclic}, réutilisé={reused}).";
    /// <summary>Construit l'avertissement signalant un bloc d'index absent.</summary>
    public static string InvalidIndexBlock(string name, ProDosStorageType storageType, int block, bool cyclic) => $"{name} ({storageType}) : le bloc d'index {block} est invalide (cycle={cyclic}).";
    /// <summary>Construit l'avertissement signalant un bloc maître absent.</summary>
    public static string InvalidMasterIndexBlock(string name, ProDosStorageType storageType, int block) => $"{name} ({storageType}) : le bloc d'index maître {block} est invalide.";
    /// <summary>Construit l'avertissement signalant un bloc de données absent.</summary>
    public static string MissingDataBlock(string name, ProDosStorageType storageType, int block) => $"{name} ({storageType}) : le bloc de données {block} est absent, hors image ou de taille incorrecte.";
    /// <summary>Construit l'avertissement signalant un contenu plus court que son EOF.</summary>
    public static string TruncatedContent(string name, ProDosStorageType storageType, long observedLength, long expectedLength) => $"{name} ({storageType}) : {observedLength} octet(s) reconstruits sur {expectedLength} attendus.";
    /// <summary>Construit l'avertissement signalant un bloc bitmap invalide.</summary>
    public static string InvalidBitmapBlock(int block) => $"Le bloc bitmap ProDOS {block} est absent ou de taille incorrecte.";
    /// <summary>Construit l'avertissement signalant un total de blocs incohérent.</summary>
    public static string InvalidTotalBlockCount(int declared, int available) => $"Le volume ProDOS annonce {declared} blocs alors que l'image en contient {available}.";
}
