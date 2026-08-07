namespace GWGUI.Scp.FileSystems;

public enum FileSystemEntryKind { Directory, File, Link, Unknown }

public sealed record FileSystemEntry(
    string Name,
    FileSystemEntryKind Kind,
    long Size,
    DateTimeOffset? Modified,
    string Comment,
    uint Protection,
    int HeaderBlock,
    bool MetadataValid,
    IReadOnlyList<FileSystemEntry> Children,
    IReadOnlyList<byte>? Content = null);

public sealed record FileSystemVolume(
    string Name,
    string FileSystem,
    long Capacity,
    long FreeBytes,
    DateTimeOffset? Created,
    DateTimeOffset? Modified,
    IReadOnlyList<FileSystemEntry> Entries,
    IReadOnlyList<string> Warnings);

public interface IFileSystemReader
{
    string Id { get; }
    bool CanRead(SectorImages.SectorImage image);
    FileSystemVolume Read(SectorImages.SectorImage image);
}
