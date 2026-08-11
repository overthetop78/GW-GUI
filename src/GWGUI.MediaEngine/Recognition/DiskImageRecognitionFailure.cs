namespace GWGUI.MediaEngine.Recognition;

/// <summary>Conserve l'identité d'une politique candidate et l'erreur technique ayant rejeté son contenu.</summary>
/// <param name="PolicyName">Identité du type de politique.</param>
/// <param name="Exception">Erreur technique produite par son Reader.</param>
public sealed record DiskImageRecognitionFailure(string PolicyName, Exception Exception);

/// <summary>Signale que toutes les politiques présélectionnées ont rejeté le contenu.</summary>
public sealed class DiskImageCandidatesRejectedException : AggregateException
{
    /// <summary>Crée l'erreur finale et conserve les rejets dans leur ordre d'exécution.</summary>
    /// <param name="message">Message technique final.</param>
    /// <param name="path">Chemin examiné.</param>
    /// <param name="extension">Extension normalisée.</param>
    /// <param name="requestedFormatId">Identifiant demandé, ou <see langword="null"/>.</param>
    /// <param name="failures">Rejets ordonnés des politiques présélectionnées.</param>
    public DiskImageCandidatesRejectedException(string message, string path, string extension, string? requestedFormatId, IReadOnlyList<DiskImageRecognitionFailure> failures)
        : base(message, failures.Select(failure => failure.Exception))
    {
        Path = path;
        Extension = extension;
        RequestedFormatId = requestedFormatId;
        Failures = failures.ToArray();
    }

    /// <summary>Obtient le chemin examiné.</summary>
    public string Path { get; }
    /// <summary>Obtient l'extension normalisée.</summary>
    public string Extension { get; }
    /// <summary>Obtient l'identifiant demandé, ou <see langword="null"/>.</summary>
    public string? RequestedFormatId { get; }
    /// <summary>Obtient les rejets ordonnés.</summary>
    public IReadOnlyList<DiskImageRecognitionFailure> Failures { get; }
}
