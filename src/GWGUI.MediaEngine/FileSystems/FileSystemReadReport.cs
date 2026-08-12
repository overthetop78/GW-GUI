namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Présente séparément les lectures réussies et les corruptions rencontrées.</summary>
public sealed record FileSystemReadReport
{
    /// <summary>Crée un rapport et copie ses collections.</summary>
    public FileSystemReadReport(IEnumerable<FileSystemMatch> matches, IEnumerable<FileSystemReadFailure> failures)
    {
        Matches = Array.AsReadOnly(matches.ToArray());
        Failures = Array.AsReadOnly(failures.ToArray());
    }

    /// <summary>Lectures réussies dans l'ordre des candidats.</summary>
    public IReadOnlyList<FileSystemMatch> Matches { get; }
    /// <summary>Lecteurs candidats ayant rejeté un contenu corrompu.</summary>
    public IReadOnlyList<FileSystemReadFailure> Failures { get; }
    /// <summary>Indique qu'au moins un volume a été lu.</summary>
    public bool HasMatches => Matches.Count != 0;
    /// <summary>Indique qu'au moins un lecteur a reconnu puis rejeté le contenu.</summary>
    public bool HasCorruption => Failures.Count != 0;
    /// <summary>Indique qu'aucun lecteur n'a reconnu l'image.</summary>
    public bool IsUnrecognized => Matches.Count == 0 && Failures.Count == 0;
}
