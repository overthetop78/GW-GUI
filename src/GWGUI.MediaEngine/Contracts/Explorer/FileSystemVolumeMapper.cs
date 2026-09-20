using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Contracts.Explorer;

/// <summary>Convertit les volumes entre la lecture interne et les données échangées par MediaEngine.</summary>
public static class FileSystemVolumeMapper
{
    public static FileSystemVolume ConvertVolume(GWGUI.MediaFileSystems.FileSystemVolume volume)
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

    public static GWGUI.MediaFileSystems.FileSystemVolume ToFileSystemsVolume(FileSystemVolume volume)
    {
        ArgumentNullException.ThrowIfNull(volume);
        return new GWGUI.MediaFileSystems.FileSystemVolume(
            volume.Name, volume.FileSystemId, volume.Capacity, volume.FreeBytes,
            volume.Created, volume.Modified, volume.Entries.Select(ToFileSystemsEntry),
            volume.Warnings, volume.FreeSpaceKnown, volume.Attributes, volume.Bootable,
            volume.DiskNumber, volume.DiskCount, volume.DiskNumberOrigin);
    }

    private static GWGUI.MediaFileSystems.FileSystemEntry ToFileSystemsEntry(FileSystemEntry entry) =>
        new(
            entry.Name,
            entry.Kind switch
            {
                FileSystemEntryKind.Directory => GWGUI.MediaFileSystems.FileSystemEntryKind.Directory,
                FileSystemEntryKind.File => GWGUI.MediaFileSystems.FileSystemEntryKind.File,
                FileSystemEntryKind.Link => GWGUI.MediaFileSystems.FileSystemEntryKind.Link,
                _ => GWGUI.MediaFileSystems.FileSystemEntryKind.Unknown
            },
            entry.Size, entry.Modified, entry.Comment, entry.RawAttributes,
            entry.StorageReference, entry.MetadataValid, entry.Children.Select(ToFileSystemsEntry),
            entry.Content, entry.NativeTypeId, entry.OccupiedSize, entry.Created, entry.Accessed,
            entry.Attributes, entry.DataValid, entry.SyntheticName, entry.LinkTarget,
            entry.Diagnostics, entry.Metadata);

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
