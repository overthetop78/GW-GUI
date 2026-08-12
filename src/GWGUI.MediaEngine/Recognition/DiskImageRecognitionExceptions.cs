namespace GWGUI.MediaEngine.Recognition;

/// <summary>Construit les erreurs techniques produites pendant la reconnaissance d'une image de média.</summary>
internal static class DiskImageRecognitionExceptions
{
    /// <summary>Crée l'erreur signalant qu'aucune politique n'a validé le fichier.</summary>
    /// <param name="context">Contexte contenant le chemin, l'extension et le format demandé.</param>
    /// <returns>Exception contenant le chemin dont aucun candidat n'a été trouvé.</returns>
    public static DiskImageNotRecognizedException NoCandidateValidated(DiskImageRecognitionContext context) => new($"No recognition policy validated image '{context.Path}' with extension '{context.Extension}' and requested format '{context.RequestedFormatId ?? "<automatic>"}'.", context.Path);

    /// <summary>Crée l'erreur signalant qu'aucune politique ne prend en charge le format explicitement demandé.</summary>
    /// <param name="requestedFormat">Identifiant du format explicitement demandé.</param>
    /// <returns>Exception contenant l'identifiant inconnu du registre.</returns>
    public static NotSupportedException UnsupportedRequestedFormat(string requestedFormat) => new($"The selected format '{requestedFormat}' is not supported by any recognition policy.");

    /// <summary>Crée l'erreur signalant qu'une politique déterminée ne prend pas en charge le format demandé.</summary>
    /// <param name="requestedFormat">Identifiant du format explicitement demandé.</param>
    /// <param name="policyName">Nom de la politique ayant rejeté cet identifiant.</param>
    /// <returns>Exception contenant le format demandé et la politique concernée.</returns>
    public static NotSupportedException PolicyDoesNotSupportRequestedFormat(string requestedFormat, string policyName) => new($"The selected format '{requestedFormat}' is not supported by recognition policy '{policyName}'.");

    /// <summary>Crée l'erreur finale conservant tous les rejets produits par les politiques candidates.</summary>
    /// <param name="context">Contexte contenant le chemin, l'extension et le format demandé.</param>
    /// <param name="failures">Rejets techniques associés à l'identité de chaque politique.</param>
    /// <returns>Exception agrégée contenant tous les rejets dans leur ordre d'exécution.</returns>
    public static DiskImageCandidatesRejectedException AllCandidatesRejected(DiskImageRecognitionContext context, IReadOnlyList<DiskImageRecognitionFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(failures);
        if (failures.Any(failure => failure is null)) throw new ArgumentException("A recognition failure cannot be null.", nameof(failures));
        var requestedFormatText = context.RequestedFormatId is null ? "<automatic>" : context.RequestedFormatId;
        return new($"All recognition candidates rejected image '{context.Path}' with extension '{context.Extension}' and requested format '{requestedFormatText}'.", context.Path, context.Extension, context.RequestedFormatId, failures);
    }
}
