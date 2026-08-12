namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

/// <summary>Contient le résultat technique d'un fork MFS.</summary>
internal sealed record MacMfsForkResult
{
    /// <summary>Crée un résultat de lecture de fork.</summary>
    public MacMfsForkResult(IReadOnlyList<byte> content, bool isValid, IReadOnlyList<int> missingBlocks, IReadOnlyList<int> visitedClusters)
    {
        Content = content;
        IsValid = isValid;
        MissingBlocks = missingBlocks;
        VisitedClusters = visitedClusters;
    }

    /// <summary>Contenu positionné du fork.</summary>
    public IReadOnlyList<byte> Content { get; }
    /// <summary>Indique si la chaîne et tous ses secteurs sont valides.</summary>
    public bool IsValid { get; }
    /// <summary>Blocs logiques absents ou de mauvaise taille.</summary>
    public IReadOnlyList<int> MissingBlocks { get; }
    /// <summary>Clusters visités dans leur ordre logique.</summary>
    public IReadOnlyList<int> VisitedClusters { get; }
}
