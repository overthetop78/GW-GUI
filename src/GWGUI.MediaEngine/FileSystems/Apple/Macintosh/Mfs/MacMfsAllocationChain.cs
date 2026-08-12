namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

/// <summary>Décrit le parcours protégé d'une chaîne d'allocation MFS.</summary>
internal sealed record MacMfsAllocationChain
{
    /// <summary>Crée un résultat de parcours de chaîne.</summary>
    public MacMfsAllocationChain(IReadOnlyList<int> clusters, bool isValid, bool hasCycle, bool isOutOfRange, bool isPrematureEnd)
    {
        Clusters = clusters;
        IsValid = isValid;
        HasCycle = hasCycle;
        IsOutOfRange = isOutOfRange;
        IsPrematureEnd = isPrematureEnd;
    }

    /// <summary>Clusters visités dans leur ordre logique.</summary>
    public IReadOnlyList<int> Clusters { get; }
    /// <summary>Indique si la chaîne couvre la longueur demandée sans anomalie.</summary>
    public bool IsValid { get; }
    /// <summary>Indique si un cluster a été visité deux fois.</summary>
    public bool HasCycle { get; }
    /// <summary>Indique si un cluster sort de la carte.</summary>
    public bool IsOutOfRange { get; }
    /// <summary>Indique si la fin de chaîne précède la longueur demandée.</summary>
    public bool IsPrematureEnd { get; }
}
