using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Migration;

public static class MigrationPlanner
{
    public static MigrationPlan Create(FileSystemVolume source, string targetFileSystemId)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFileSystemId);
        return new(source.FileSystemId, targetFileSystemId, source.Name, source.Entries.Select(entry => CreateEntry(entry, string.Empty)));
    }

    private static MigrationEntry CreateEntry(FileSystemEntry source, string parentPath)
    {
        var path = string.IsNullOrEmpty(parentPath) ? source.Name : $"{parentPath}/{source.Name}";
        return new(path, source.Name, source.Kind, source.Content, source.Modified, source.Comment, source.RawAttributes, source.MetadataValid, source.Children.Select(child => CreateEntry(child, path)));
    }
}
