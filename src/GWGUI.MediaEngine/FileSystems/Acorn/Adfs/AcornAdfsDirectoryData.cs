namespace GWGUI.MediaEngine.FileSystems.Acorn.Adfs;

/// <summary>Contient le nom, le titre et les enfants décodés d'un répertoire ADFS.</summary>
public sealed class AcornAdfsDirectoryData
{
    /// <summary>Crée une copie immuable des données d'un répertoire.</summary>
    public AcornAdfsDirectoryData(string name, string title, IEnumerable<FileSystemEntry> children)
    {
        Name = name;
        Title = title;
        Children = Array.AsReadOnly(children.ToArray());
    }

    /// <summary>Nom du répertoire.</summary>
    public string Name { get; }
    /// <summary>Titre du répertoire.</summary>
    public string Title { get; }
    /// <summary>Entrées enfants.</summary>
    public IReadOnlyList<FileSystemEntry> Children { get; }
}
