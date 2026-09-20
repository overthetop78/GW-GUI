using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems;

/// <summary>Adapts a file-system reader to the engine's existing sector-reader catalog.</summary>
public sealed class MediaFileSystemsReaderAdapter : IFileSystemReader
{
    private readonly GWGUI.MediaFileSystems.IFileSystemReader reader;

    public MediaFileSystemsReaderAdapter(GWGUI.MediaFileSystems.IFileSystemReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        this.reader = reader;
    }

    public string Id => reader.Id;

    public IReadOnlySet<string> CatalogFormatIds => reader.CatalogFormatIds;

    public bool CanRead(SectorImage image) => reader.CanRead(image);

    public FileSystemVolume Read(SectorImage image) => ConvertVolume(reader.Read(image));

    internal static FileSystemVolume ConvertVolume(GWGUI.MediaFileSystems.FileSystemVolume volume)
    {
        ArgumentNullException.ThrowIfNull(volume);
        return new FileSystemVolume(
            volume.Name,
            volume.FileSystemId,
            volume.Capacity,
            volume.FreeBytes,
            volume.Created,
            volume.Modified,
            volume.Entries.Select(ConvertEntry),
            volume.Warnings,
            volume.FreeSpaceKnown,
            volume.Attributes,
            volume.Bootable,
            volume.DiskNumber,
            volume.DiskCount,
            volume.DiskNumberOrigin);
    }

    private static FileSystemEntry ConvertEntry(GWGUI.MediaFileSystems.FileSystemEntry entry) =>
        new(
            entry.Name,
            entry.Kind switch
            {
                GWGUI.MediaFileSystems.FileSystemEntryKind.Directory => FileSystemEntryKind.Directory,
                GWGUI.MediaFileSystems.FileSystemEntryKind.File => FileSystemEntryKind.File,
                GWGUI.MediaFileSystems.FileSystemEntryKind.Link => FileSystemEntryKind.Link,
                _ => FileSystemEntryKind.Unknown
            },
            entry.Size,
            entry.Modified,
            entry.Comment,
            entry.RawAttributes,
            entry.StorageReference,
            entry.MetadataValid,
            entry.Children.Select(ConvertEntry),
            entry.Content,
            entry.NativeTypeId,
            entry.OccupiedSize,
            entry.Created,
            entry.Accessed,
            entry.Attributes,
            entry.DataValid,
            entry.SyntheticName,
            entry.LinkTarget,
            entry.Diagnostics,
            entry.Metadata);
}
