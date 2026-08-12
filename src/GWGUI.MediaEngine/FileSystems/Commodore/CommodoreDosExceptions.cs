namespace GWGUI.MediaEngine.FileSystems.Commodore;

/// <summary>Construit les diagnostics propres aux volumes Commodore DOS.</summary>
public static class CommodoreDosExceptions
{
    /// <summary>Crée l'erreur signalant une disposition non prise en charge.</summary>
    /// <param name="formatId">Identifiant de format observé.</param>
    /// <returns>Erreur correspondante.</returns>
    public static InvalidDataException UnsupportedLayout(string formatId) => new($"The image format '{formatId}' does not contain a supported CBM DOS file system.");

    /// <summary>Crée l'erreur signalant l'absence du secteur d'en-tête.</summary>
    /// <param name="track">Piste attendue.</param>
    /// <param name="sector">Secteur attendu.</param>
    /// <returns>Erreur correspondante.</returns>
    public static InvalidDataException MissingHeader(int track, int sector) => new($"The CBM DOS header sector {track}/{sector} is missing.");

    /// <summary>Crée l'erreur signalant une chaîne cyclique.</summary>
    /// <param name="name">Nom de la chaîne.</param>
    /// <param name="track">Piste où le cycle a été détecté.</param>
    /// <param name="sector">Secteur où le cycle a été détecté.</param>
    /// <returns>Erreur correspondante.</returns>
    public static InvalidDataException CyclicChain(string name, int track, int sector) => new($"Cyclic data chain for '{name}' at {track}/{sector}.");

    /// <summary>Crée l'erreur signalant un secteur de données absent.</summary>
    /// <param name="name">Nom du fichier concerné.</param>
    /// <param name="track">Piste attendue.</param>
    /// <param name="sector">Secteur attendu.</param>
    /// <returns>Erreur correspondante.</returns>
    public static InvalidDataException MissingDataSector(string name, int track, int sector) => new($"Data sector {track}/{sector} for '{name}' is missing.");

    /// <summary>Construit l'avertissement signalant un cycle du répertoire.</summary>
    public static string CyclicDirectory(int track, int sector) => $"Cyclic CBM DOS directory chain at {track}/{sector}.";

    /// <summary>Construit l'avertissement signalant un secteur de répertoire absent.</summary>
    public static string MissingDirectorySector(int track, int sector) => $"CBM DOS directory sector {track}/{sector} is missing.";

    /// <summary>Construit l'avertissement signalant une chaîne dépassant la capacité de l'image.</summary>
    public static string ChainExceedsCapacity(string name) => $"{name}: file chain exceeds image capacity.";
}
