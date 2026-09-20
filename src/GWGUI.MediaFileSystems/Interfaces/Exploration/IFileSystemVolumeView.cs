namespace GWGUI.MediaFileSystems.Interfaces.Exploration;

/// <summary>Données d'un volume nécessaires pour évaluer et dédupliquer ses fichiers.</summary>
public interface IFileSystemVolumeView
{
    string Name { get; }
    IReadOnlyList<string> Warnings { get; }
    IReadOnlyList<IFileSystemEntryView> Entries { get; }
}
