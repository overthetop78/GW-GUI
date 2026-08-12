namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Réunit le contenu positionné, sa validité et les diagnostics d'une lecture CBM DOS.</summary>
internal sealed record CommodoreDosFileReadResult
{
    /// <summary>Crée un résultat en copiant toutes ses collections.</summary>
    public CommodoreDosFileReadResult(IEnumerable<byte> content, bool isValid, IEnumerable<(int Track, int Sector)> visitedSectors, IEnumerable<string> warnings, int? firstLogicalBlock)
    {
        Content = Array.AsReadOnly(content.ToArray());
        IsValid = isValid;
        VisitedSectors = Array.AsReadOnly(visitedSectors.ToArray());
        Warnings = Array.AsReadOnly(warnings.ToArray());
        FirstLogicalBlock = firstLogicalBlock;
    }

    /// <summary>Contenu reconstruit.</summary>
    public IReadOnlyList<byte> Content { get; }
    /// <summary>Indique si toute la chaîne était lisible et cohérente.</summary>
    public bool IsValid { get; }
    /// <summary>Coordonnées parcourues dans leur ordre logique.</summary>
    public IReadOnlyList<(int Track, int Sector)> VisitedSectors { get; }
    /// <summary>Avertissements produits pendant cette lecture.</summary>
    public IReadOnlyList<string> Warnings { get; }
    /// <summary>Premier bloc logique, ou aucune valeur lorsque la coordonnée initiale est absente ou invalide.</summary>
    public int? FirstLogicalBlock { get; }
}
