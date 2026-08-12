namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Décrit une entrée décodée dans un système de fichiers.</summary>
public sealed record FileSystemEntry
{
    /// <summary>Crée une entrée et copie ses collections.</summary>
    public FileSystemEntry(string name, FileSystemEntryKind kind, long size, DateTimeOffset? modified, string comment, uint rawAttributes, int storageReference, bool metadataValid, IEnumerable<FileSystemEntry> children, IEnumerable<byte>? content = null)
    {
        Name = name;
        Kind = kind;
        Size = size;
        Modified = modified;
        Comment = comment;
        RawAttributes = rawAttributes;
        StorageReference = storageReference;
        MetadataValid = metadataValid;
        Children = Array.AsReadOnly(children.ToArray());
        Content = content is null ? null : Array.AsReadOnly(content.ToArray());
    }

    /// <summary>Nom décodé de l'entrée.</summary>
    public string Name { get; }
    /// <summary>Nature commune de l'entrée.</summary>
    public FileSystemEntryKind Kind { get; }
    /// <summary>Taille logique en octets.</summary>
    public long Size { get; }
    /// <summary>Date de dernière modification, ou <see langword="null"/> lorsqu'elle est absente.</summary>
    public DateTimeOffset? Modified { get; }
    /// <summary>Description technique décodée ou construite par le lecteur, jamais un texte d'interface localisé.</summary>
    public string Comment { get; }
    /// <summary>Attributs bruts dont la signification dépend du format.</summary>
    public uint RawAttributes { get; }
    /// <summary>Référence de stockage dont la signification dépend du format.</summary>
    public int StorageReference { get; }
    /// <summary>Indique si les métadonnées de l'entrée sont valides, indépendamment de la présence de son contenu.</summary>
    public bool MetadataValid { get; }
    /// <summary>Copie non modifiable des entrées enfants.</summary>
    public IReadOnlyList<FileSystemEntry> Children { get; }
    /// <summary>Copie non modifiable du contenu, collection vide pour un fichier vide, ou <see langword="null"/> lorsque le contenu est absent.</summary>
    public IReadOnlyList<byte>? Content { get; }
}
