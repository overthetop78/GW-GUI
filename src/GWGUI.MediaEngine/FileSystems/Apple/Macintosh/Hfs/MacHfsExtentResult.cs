namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

/// <summary>Contient le résultat positionné de la lecture des extents intégrés HFS.</summary>
internal sealed record MacHfsExtentResult
{
    /// <summary>Crée un résultat de lecture d'extents.</summary>
    public MacHfsExtentResult(IEnumerable<byte> content, bool isValid, IEnumerable<int> missingBlocks, long remainingLength)
    {
        Content = Array.AsReadOnly(content.ToArray());
        IsValid = isValid;
        MissingBlocks = Array.AsReadOnly(missingBlocks.ToArray());
        RemainingLength = remainingLength;
    }

    /// <summary>Contenu reconstruit et tronqué à sa longueur logique.</summary>
    public IReadOnlyList<byte> Content { get; }
    /// <summary>Indique si tous les secteurs attendus sont valides et présents.</summary>
    public bool IsValid { get; }
    /// <summary>Blocs logiques absents ou de taille invalide.</summary>
    public IReadOnlyList<int> MissingBlocks { get; }
    /// <summary>Longueur qui nécessite des extents supplémentaires.</summary>
    public long RemainingLength { get; }
}
