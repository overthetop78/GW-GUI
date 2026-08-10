namespace GWGUI.MediaEngine.Recognition;

/// <summary>
/// Construit les erreurs produites pendant la reconnaissance d’une image de média.
/// </summary>
public static class DiskImageRecognitionExceptions
{
    /// <summary>Crée l’erreur signalant qu’aucune politique ne prend en charge une extension.</summary>
    /// <param name="extension">Extension normalisée du fichier examiné.</param>
    /// <returns>Exception contenant l’extension rejetée.</returns>
    public static NotSupportedException UnsupportedExtension(string extension) => new($"The image extension '{extension}' is not supported by the explorer yet.");

    /// <summary>Crée l’erreur signalant qu’une politique ne prend pas en charge le format demandé.</summary>
    /// <param name="requestedFormat">Identifiant du format explicitement demandé.</param>
    /// <param name="policyName">Nom de la politique ayant rejeté cet identifiant.</param>
    /// <returns>Exception contenant le format demandé et la politique concernée.</returns>
    public static NotSupportedException UnsupportedRequestedFormat(string requestedFormat, string policyName) => new($"The selected format '{requestedFormat}' is not supported by recognition policy '{policyName}'.");

    /// <summary>Crée l’erreur signalant qu’une politique compatible a rejeté le contenu du fichier.</summary>
    /// <param name="extension">Extension normalisée du fichier examiné.</param>
    /// <param name="policyName">Nom de la politique ayant rejeté le contenu.</param>
    /// <param name="innerException">Erreur technique produite par le lecteur de la politique.</param>
    /// <returns>Exception contenant l’extension, la politique et l’erreur d’origine.</returns>
    public static InvalidDataException PolicyRejectedContent(string extension, string policyName, Exception innerException) => new($"Recognition policy '{policyName}' rejected the content of image extension '{extension}'.", innerException);
}
