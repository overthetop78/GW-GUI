namespace GWGUI.MediaEngine.FileSystems.Apple.Lisa;

/// <summary>Réunit un contenu de fichier Lisa positionné, sa validité et sa première référence logique.</summary>
internal sealed record LisaFileContent
{
    /// <summary>Crée le résultat de reconstruction d'un fichier Lisa.</summary>
    public LisaFileContent(IReadOnlyList<byte> content, bool isValid, int firstLogicalBlock)
    {
        Content = content;
        IsValid = isValid;
        FirstLogicalBlock = firstLogicalBlock;
    }

    /// <summary>Contenu reconstruit, lacunes comprises.</summary>
    public IReadOnlyList<byte> Content { get; }

    /// <summary>Indique si aucune page n'est absente ou dupliquée.</summary>
    public bool IsValid { get; }

    /// <summary>Première référence logique du contenu.</summary>
    public int FirstLogicalBlock { get; }
}
