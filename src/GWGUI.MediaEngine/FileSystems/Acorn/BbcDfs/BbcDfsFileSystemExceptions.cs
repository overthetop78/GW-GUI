namespace GWGUI.MediaEngine.FileSystems.Acorn.BbcDfs;

/// <summary>Construit les erreurs et avertissements propres à BBC DFS.</summary>
internal static class BbcDfsFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant un catalogue invalide.</summary>
    public static InvalidDataException InvalidCatalog(int entryCount, int totalSectors) => new($"The BBC DFS catalogue is invalid: {entryCount} entries and {totalSectors} total sectors were observed.");
    /// <summary>Construit l'avertissement signalant une plage de fichier hors image.</summary>
    public static string FileOutsideImage(string name, int firstSector, int length) => $"{name}: file range from sector {firstSector} for {length} bytes is outside the image.";
}
