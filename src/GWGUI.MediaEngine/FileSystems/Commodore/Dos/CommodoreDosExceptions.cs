namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Construit les diagnostics propres aux volumes Commodore DOS.</summary>
public static class CommodoreDosExceptions
{
    /// <summary>Crée l'erreur signalant une disposition non prise en charge.</summary>
    /// <param name="formatId">Identifiant de format observé.</param>
    /// <returns>Erreur correspondante.</returns>
    public static InvalidDataException UnsupportedLayout(string formatId) => new($"Le format d'image '{formatId}' ne contient pas de système de fichiers CBM DOS pris en charge.");

    /// <summary>Crée l'erreur signalant l'absence du secteur d'en-tête.</summary>
    /// <param name="track">Piste attendue.</param>
    /// <param name="sector">Secteur attendu.</param>
    /// <returns>Erreur correspondante.</returns>
    public static InvalidDataException MissingHeader(int track, int sector) => new($"Le secteur d'en-tête CBM DOS {track}/{sector} est absent.");

    /// <summary>Crée l'erreur signalant une coordonnée piste/secteur invalide.</summary>
    public static InvalidDataException InvalidCoordinate(int track, int sector) => new($"La coordonnée CBM DOS {track}/{sector} est invalide.");
    /// <summary>Crée l'erreur signalant un compteur du dernier secteur invalide.</summary>
    public static InvalidDataException InvalidLastSectorCount(string name, int value) => new($"Le compteur final CBM DOS {value} de '{name}' est invalide.");
}
