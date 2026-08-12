namespace GWGUI.MediaEngine.FileSystems.Lisa;

/// <summary>Construit les diagnostics propres au système de fichiers Lisa.</summary>
public static class LisaFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant l'absence d'un MDDF.</summary>
    public static InvalidDataException MissingTaggedFileSystem(int pageCount) => new($"The image does not contain a tagged Lisa file system among its {pageCount} pages.");
    /// <summary>Construit l'avertissement signalant l'absence du catalogue.</summary>
    public static string MissingCatalog(ushort version) => $"The Lisa catalog pages are missing for version 0x{version:X4}; file names were recovered from page tags only.";
    /// <summary>Construit l'avertissement signalant une page absente.</summary>
    public static string MissingPage(ushort fileId, int pageNumber) => $"Lisa file 0x{fileId:X4} is missing page {pageNumber}.";
}
