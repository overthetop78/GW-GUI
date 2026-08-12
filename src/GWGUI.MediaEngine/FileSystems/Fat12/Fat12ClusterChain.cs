namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Réunit le contenu positionné, sa validité et les clusters visités d'une chaîne FAT12.</summary>
internal sealed record Fat12ClusterChain
{
    /// <summary>Crée un résultat en copiant ses collections.</summary>
    public Fat12ClusterChain(IEnumerable<byte> content, bool isValid, IEnumerable<int> visitedClusters)
    {
        Content = Array.AsReadOnly(content.ToArray());
        IsValid = isValid;
        VisitedClusters = Array.AsReadOnly(visitedClusters.ToArray());
    }

    /// <summary>Contenu reconstruit dans ses positions logiques.</summary>
    public IReadOnlyList<byte> Content { get; }
    /// <summary>Indique si la table et tous les secteurs étaient lisibles.</summary>
    public bool IsValid { get; }
    /// <summary>Clusters parcourus dans leur ordre de chaîne.</summary>
    public IReadOnlyList<int> VisitedClusters { get; }
}
