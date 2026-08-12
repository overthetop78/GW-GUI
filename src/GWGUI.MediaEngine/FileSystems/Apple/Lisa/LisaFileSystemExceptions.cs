namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Construit les diagnostics propres au système de fichiers Lisa.</summary>
public static class LisaFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant l'absence d'un MDDF.</summary>
    public static InvalidDataException MissingTaggedFileSystem(int pageCount) => new($"L'image ne contient aucun système Lisa tagué parmi ses {pageCount} pages.");
    /// <summary>Construit l'avertissement signalant l'absence du catalogue.</summary>
    public static string MissingCatalog(ushort version) => $"Les pages du catalogue Lisa {LisaCatalogVersionNames.Get(version)} sont absentes ; les noms de secours proviennent des identifiants des tags.";
    /// <summary>Construit l'avertissement signalant une page absente.</summary>
    public static string MissingPage(ushort fileId, int pageNumber) => $"Le fichier Lisa 0x{fileId:X4} ne contient pas la page {pageNumber}.";
    /// <summary>Crée l'erreur signalant un MDDF tronqué.</summary>
    public static InvalidDataException TruncatedMddf(int observed, int expected) => new($"La page MDDF Lisa contient {observed} octet(s) ; {expected} sont requis.");
    /// <summary>Construit l'avertissement d'un tag absent ou tronqué.</summary>
    public static string InvalidTag(int logicalBlock, int observed, int expected) => $"Le bloc logique Lisa {logicalBlock} possède un tag de {observed} octet(s) ; {expected} sont requis.";
    /// <summary>Construit l'avertissement d'une page dupliquée.</summary>
    public static string DuplicatePage(ushort fileId, int pageNumber) => $"Le fichier Lisa 0x{fileId:X4} contient plusieurs pages {pageNumber}.";
    /// <summary>Construit le nom de secours d'un fichier.</summary>
    public static string FallbackFileName(ushort fileId) => $"Fichier {fileId:X4}";
    /// <summary>Construit la description technique d'un fichier.</summary>
    public static string FileDescription(ushort fileId) => $"Fichier Lisa ${fileId:X4}";
    /// <summary>Construit l'avertissement signalant que l'espace libre ne peut pas être établi.</summary>
    public static string UnknownFreeSpace() => "L'espace libre Lisa ne peut pas être établi sans tag exploitable.";
}
