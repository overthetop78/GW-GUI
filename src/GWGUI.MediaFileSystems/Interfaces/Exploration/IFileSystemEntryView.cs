namespace GWGUI.MediaFileSystems.Interfaces.Exploration;

/// <summary>Données d'une entrée nécessaires pour comparer les volumes reconnus.</summary>
public interface IFileSystemEntryView
{
    string Name { get; }
    string KindName { get; }
    long Size { get; }
    bool MetadataValid { get; }
    bool SyntheticName { get; }
    IReadOnlyList<IFileSystemEntryView> Children { get; }
}
