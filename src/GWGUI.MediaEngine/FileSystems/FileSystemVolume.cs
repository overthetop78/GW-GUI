namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Décrit un volume et les entrées décodées de son système de fichiers.</summary>
public sealed record FileSystemVolume
{
    /// <summary>Crée un volume et copie ses collections.</summary>
    public FileSystemVolume(string name, string fileSystemId, long capacity, long freeBytes, DateTimeOffset? created, DateTimeOffset? modified, IEnumerable<FileSystemEntry> entries, IEnumerable<string> warnings, bool freeSpaceKnown = true)
    {
        Name = name;
        FileSystemId = fileSystemId;
        Capacity = capacity;
        FreeBytes = freeBytes;
        Created = created;
        Modified = modified;
        Entries = Array.AsReadOnly(entries.ToArray());
        Warnings = Array.AsReadOnly(warnings.ToArray());
        FreeSpaceKnown = freeSpaceKnown;
    }

    /// <summary>Nom du volume, éventuellement vide.</summary>
    public string Name { get; }
    /// <summary>Identifiant technique central du système de fichiers.</summary>
    public string FileSystemId { get; }
    /// <summary>Capacité du volume en octets.</summary>
    public long Capacity { get; }
    /// <summary>Espace libre en octets.</summary>
    public long FreeBytes { get; }
    /// <summary>Indique si l'espace libre a pu être calculé depuis les structures du volume.</summary>
    public bool FreeSpaceKnown { get; }
    /// <summary>Date de création, ou <see langword="null"/> lorsqu'elle est absente.</summary>
    public DateTimeOffset? Created { get; }
    /// <summary>Date de dernière modification, ou <see langword="null"/> lorsqu'elle est absente.</summary>
    public DateTimeOffset? Modified { get; }
    /// <summary>Copie non modifiable des entrées racines.</summary>
    public IReadOnlyList<FileSystemEntry> Entries { get; }
    /// <summary>Copie non modifiable des avertissements techniques.</summary>
    public IReadOnlyList<string> Warnings { get; }
}
