namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;

/// <summary>Contient une lecture positionnelle de blocs MFS.</summary>
internal sealed record MacMfsBlockReadResult
{
    /// <summary>Crée un résultat de lecture positionnelle.</summary>
    public MacMfsBlockReadResult(IReadOnlyList<byte> bytes, IReadOnlyList<bool> presentBlocks, bool isValid)
    {
        Bytes = bytes;
        PresentBlocks = presentBlocks;
        IsValid = isValid;
    }

    /// <summary>Octets lus, avec des zéros réservés aux blocs absents.</summary>
    public IReadOnlyList<byte> Bytes { get; }
    /// <summary>Présence de chaque bloc demandé.</summary>
    public IReadOnlyList<bool> PresentBlocks { get; }
    /// <summary>Indique si chaque bloc était présent avec la bonne taille.</summary>
    public bool IsValid { get; }
}
