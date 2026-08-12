namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Construit les erreurs et avertissements propres au système COHERENT.</summary>
internal static class CoherentExceptions
{
    /// <summary>Crée l'erreur signalant une zone d'inodes invalide.</summary>
    public static InvalidDataException InvalidInodeZone(int end, int fileSystemBlocks) => new($"La zone d'inodes COHERENT se termine au bloc {end}, hors du système de fichiers de {fileSystemBlocks} blocs.");
    /// <summary>Crée l'erreur signalant l'inode nul.</summary>
    public static InvalidDataException NullInode() => new("L'inode COHERENT 0 est invalide.");
    /// <summary>Crée l'erreur signalant un inode hors image.</summary>
    public static InvalidDataException InodeOutsideImage(ushort number, int imageLength) => new($"L'inode COHERENT {number} est hors de la zone de {imageLength} octets.");
    /// <summary>Crée l'erreur signalant un fichier trop grand.</summary>
    public static InvalidDataException FileTooLarge(uint size) => new($"La taille COHERENT de {size} octets ne peut pas être représentée en mémoire.");
    /// <summary>Crée l'erreur signalant l'absence d'un bloc requis du superbloc.</summary>
    public static InvalidDataException MissingSuperblockBlock() => new("Un bloc requis du superbloc COHERENT est absent.");
    /// <summary>Crée l'erreur signalant une longueur canonique insuffisante.</summary>
    public static ArgumentException InsufficientCanonicalLength(int observedLength, int expectedLength, string parameterName) => new($"La valeur canonique COHERENT contient {observedLength} octet(s) ; {expectedLength} sont requis.", parameterName);
}
