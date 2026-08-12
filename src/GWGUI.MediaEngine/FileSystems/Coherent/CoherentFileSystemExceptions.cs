namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Construit les erreurs et avertissements propres au système COHERENT.</summary>
internal static class CoherentFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant une zone d'inodes invalide.</summary>
    public static InvalidDataException InvalidInodeZone(int end, int fileSystemBlocks) => new($"The COHERENT inode zone ends at block {end}, outside the {fileSystemBlocks}-block file system.");
    /// <summary>Crée l'erreur signalant l'inode nul.</summary>
    public static InvalidDataException NullInode() => new("Invalid COHERENT inode 0.");
    /// <summary>Crée l'erreur signalant un inode hors image.</summary>
    public static InvalidDataException InodeOutsideImage(ushort number, int imageLength) => new($"COHERENT inode {number} is outside the {imageLength}-byte image.");
    /// <summary>Crée l'erreur signalant un fichier trop grand.</summary>
    public static InvalidDataException FileTooLarge(uint size) => new($"The COHERENT file size {size} cannot be represented in memory.");
    /// <summary>Construit l'avertissement signalant un bloc indirect hors image.</summary>
    public static string IndirectBlockOutsideImage(string name, long block, int depth) => $"{name}: indirect COHERENT block {block} at level {depth} is outside the image.";
}
