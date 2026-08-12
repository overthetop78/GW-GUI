namespace GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

/// <summary>Décrit un record de catalogue HFS et conserve distinctement ses deux forks.</summary>
internal sealed record MacHfsCatalogRecord
{
    /// <summary>Crée un record de catalogue HFS.</summary>
    public MacHfsCatalogRecord(uint parentId, uint id, string name, bool isDirectory, long size, DateTimeOffset? modified, string type, IEnumerable<byte> dataFork, IEnumerable<byte> resourceFork, bool isValid)
    {
        ParentId = parentId;
        Id = id;
        Name = name;
        IsDirectory = isDirectory;
        Size = size;
        Modified = modified;
        Type = type;
        DataFork = Array.AsReadOnly(dataFork.ToArray());
        ResourceFork = Array.AsReadOnly(resourceFork.ToArray());
        IsValid = isValid;
    }

    /// <summary>Identifiant du dossier parent.</summary>
    public uint ParentId { get; }
    /// <summary>Identifiant du record.</summary>
    public uint Id { get; }
    /// <summary>Nom du record.</summary>
    public string Name { get; }
    /// <summary>Indique si le record décrit un dossier.</summary>
    public bool IsDirectory { get; }
    /// <summary>Taille logique totale des deux forks.</summary>
    public long Size { get; }
    /// <summary>Date de dernière modification.</summary>
    public DateTimeOffset? Modified { get; }
    /// <summary>Type Finder ou description technique.</summary>
    public string Type { get; }
    /// <summary>Contenu du data fork.</summary>
    public IReadOnlyList<byte> DataFork { get; }
    /// <summary>Contenu du resource fork.</summary>
    public IReadOnlyList<byte> ResourceFork { get; }
    /// <summary>Indique si le record et ses forks sont valides.</summary>
    public bool IsValid { get; }
}
