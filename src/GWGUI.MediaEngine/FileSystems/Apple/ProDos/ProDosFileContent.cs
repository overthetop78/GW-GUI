namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Contient un fichier ProDOS reconstruit et les blocs visités.</summary>
internal sealed record ProDosFileContent
{
    /// <summary>Crée un résultat de reconstruction de fichier.</summary>
    public ProDosFileContent(IReadOnlyList<byte> content, bool isValid, IReadOnlySet<int> dataBlocks, IReadOnlySet<int> indexBlocks)
    {
        Content = content;
        IsValid = isValid;
        DataBlocks = dataBlocks;
        IndexBlocks = indexBlocks;
    }

    /// <summary>Contenu reconstruit jusqu'à l'EOF.</summary>
    public IReadOnlyList<byte> Content { get; }
    /// <summary>Indique si tous les pointeurs et blocs requis sont valides.</summary>
    public bool IsValid { get; }
    /// <summary>Blocs de données visités.</summary>
    public IReadOnlySet<int> DataBlocks { get; }
    /// <summary>Blocs d'index visités.</summary>
    public IReadOnlySet<int> IndexBlocks { get; }
}
