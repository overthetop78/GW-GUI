namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>
/// Construit les exceptions produites pendant la validation d’un conteneur SCP.
/// </summary>
public static class ScpExceptions
{
    /// <summary>Crée l’erreur signalant qu’un média SCP étendu ne représente pas une disquette.</summary>
    /// <returns>Exception décrivant le média étendu non pris en charge.</returns>
    public static NotSupportedException ExtendedMedia() =>
        new("Extended SCP media are not floppy images.");

    /// <summary>Crée l’erreur signalant l’absence de la signature SCP.</summary>
    /// <returns>Exception décrivant la signature de fichier absente.</returns>
    public static InvalidDataException MissingFileSignature() =>
        new("The file does not contain an SCP signature.");

    /// <summary>Crée l’erreur signalant un nombre de révolutions invalide.</summary>
    /// <param name="observedCount">Nombre de révolutions lu dans l’en-tête.</param>
    /// <returns>Exception contenant la valeur observée.</returns>
    public static InvalidDataException InvalidRevolutionCount(byte observedCount) =>
        new($"The SCP revolution count {observedCount} is invalid.");

    /// <summary>Crée l’erreur signalant une plage de pistes invalide.</summary>
    /// <param name="startTrack">Première piste observée.</param>
    /// <param name="endTrack">Dernière piste observée.</param>
    /// <returns>Exception contenant les bornes observées.</returns>
    public static InvalidDataException InvalidTrackRange(byte startTrack, byte endTrack) =>
        new($"The SCP track range {startTrack}..{endTrack} is invalid.");

    /// <summary>Crée l’erreur signalant une largeur de cellule de bit non prise en charge.</summary>
    /// <param name="observedWidth">Largeur observée dans l’en-tête.</param>
    /// <returns>Exception contenant la largeur observée.</returns>
    public static NotSupportedException UnsupportedBitCellWidth(byte observedWidth) =>
        new($"Unsupported SCP bit-cell width: {observedWidth}.");

    /// <summary>Crée l’erreur signalant un sélecteur de tête invalide.</summary>
    /// <param name="observedSelector">Sélecteur observé dans l’en-tête.</param>
    /// <returns>Exception contenant le sélecteur observé.</returns>
    public static InvalidDataException InvalidHeadSelector(byte observedSelector) =>
        new($"The SCP head selector {observedSelector} is invalid.");

    /// <summary>Crée l’erreur signalant l’absence de signature TRK pour une piste.</summary>
    /// <param name="expectedTrack">Numéro de piste attendu par la table.</param>
    /// <param name="observedTrack">Numéro présent dans le descripteur lu.</param>
    /// <returns>Exception contenant les numéros attendu et observé.</returns>
    public static InvalidDataException MissingTrackSignature(int expectedTrack, byte observedTrack) =>
        new($"Track {expectedTrack} has no TRK signature; the descriptor contains track number {observedTrack}.");

    /// <summary>Crée l’erreur signalant une incohérence entre la table et le descripteur de piste.</summary>
    /// <param name="expectedTrack">Numéro de piste attendu par la table.</param>
    /// <param name="observedTrack">Numéro présent dans le descripteur.</param>
    /// <returns>Exception contenant les numéros attendu et observé.</returns>
    public static InvalidDataException TrackNumberMismatch(int expectedTrack, byte observedTrack) =>
        new($"Track table entry {expectedTrack} points to track {observedTrack}.");

    /// <summary>Crée l’erreur signalant une section absente, incomplète ou hors limites.</summary>
    /// <param name="section">Nom technique de la section.</param>
    /// <param name="offset">Position demandée, en octets.</param>
    /// <param name="requiredLength">Longueur demandée, en octets.</param>
    /// <returns>Exception contenant la section, sa position et sa longueur requise.</returns>
    public static InvalidDataException IncompleteSection(string section, int offset, int requiredLength) =>
        new($"Incomplete or invalid {section} at offset {offset}; {requiredLength} bytes are required.");
}
