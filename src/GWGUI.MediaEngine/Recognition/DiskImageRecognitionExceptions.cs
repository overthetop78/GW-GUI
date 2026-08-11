namespace GWGUI.MediaEngine.Recognition;

/// <summary>Construit les erreurs techniques produites pendant la reconnaissance d'une image de média.</summary>
internal static class DiskImageRecognitionExceptions
{
    /// <summary>Crée l'erreur signalant qu'aucune politique n'a validé le fichier.</summary>
    /// <param name="path">Chemin du fichier examiné.</param>
    /// <returns>Exception contenant le chemin dont aucun candidat n'a été trouvé.</returns>
    public static NotSupportedException NoCandidateValidated(string path) => new($"No recognition policy validated image '{path}'.");

    /// <summary>Crée l'erreur signalant qu'aucune politique ne prend en charge le format explicitement demandé.</summary>
    /// <param name="requestedFormat">Identifiant du format explicitement demandé.</param>
    /// <returns>Exception contenant l'identifiant inconnu du registre.</returns>
    public static NotSupportedException UnsupportedRequestedFormat(string requestedFormat) => new($"The selected format '{requestedFormat}' is not supported by any recognition policy.");

    /// <summary>Crée l'erreur signalant qu'une politique déterminée ne prend pas en charge le format demandé.</summary>
    /// <param name="requestedFormat">Identifiant du format explicitement demandé.</param>
    /// <param name="policyName">Nom de la politique ayant rejeté cet identifiant.</param>
    /// <returns>Exception contenant le format demandé et la politique concernée.</returns>
    public static NotSupportedException PolicyDoesNotSupportRequestedFormat(string requestedFormat, string policyName) => new($"The selected format '{requestedFormat}' is not supported by recognition policy '{policyName}'.");

    /// <summary>Crée l'erreur signalant qu'une politique candidate a rejeté le contenu du fichier.</summary>
    /// <param name="path">Chemin du fichier examiné.</param>
    /// <param name="policyName">Nom de la politique ayant rejeté le contenu.</param>
    /// <param name="innerException">Erreur technique produite par le lecteur de la politique.</param>
    /// <returns>Exception contenant le chemin, la politique et l'erreur d'origine.</returns>
    public static InvalidDataException PolicyRejectedContent(string path, string policyName, Exception innerException)
    {
        ArgumentNullException.ThrowIfNull(innerException);
        return new($"Recognition policy '{policyName}' rejected image '{path}'.", innerException);
    }

    /// <summary>Crée l'erreur finale conservant tous les rejets produits par les politiques candidates.</summary>
    /// <param name="path">Chemin du fichier examiné.</param>
    /// <param name="requestedFormat">Identifiant demandé, ou <see langword="null"/> en détection automatique.</param>
    /// <param name="rejections">Rejets enveloppés avec l'identité de chaque politique.</param>
    /// <returns>Exception agrégée contenant tous les rejets dans leur ordre d'exécution.</returns>
    public static AggregateException AllCandidatesRejected(string path, string? requestedFormat, IReadOnlyList<Exception> rejections)
    {
        ArgumentNullException.ThrowIfNull(rejections);
        if (rejections.Any(exception => exception is null)) throw new ArgumentException("A rejection cannot be null.", nameof(rejections));
        var requestedFormatText = requestedFormat is null ? string.Empty : $" for requested format '{requestedFormat}'";
        return new($"All recognition candidates rejected image '{path}'{requestedFormatText}.", rejections);
    }
}
