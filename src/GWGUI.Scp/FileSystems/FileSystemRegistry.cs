using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.FileSystems;

public sealed class FileSystemRegistry
{
    public IReadOnlyList<IFileSystemReader> Readers { get; } =
    [
        new Readers.AmigaDosFileSystemReader(),
        new Readers.AtariFat12FileSystemReader(),
        new Readers.AtariDosFileSystemReader()
    ];
    public IReadOnlySet<string> SupportedFormatIds => Readers
        .SelectMany(reader => reader.CatalogFormatIds)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public FileSystemVolume Read(SectorImage image, string? fileSystemId = null)
    {
        if (TryRead(image, fileSystemId, out var volume)) return volume;
        throw new InvalidDataException("No supported file system was detected in the disk image.");
    }

    public bool TryRead(SectorImage image, string? fileSystemId, out FileSystemVolume volume)
    {
        if (fileSystemId is not null)
            fileSystemId = Readers.FirstOrDefault(reader => reader.CatalogFormatIds.Contains(fileSystemId))?.Id ?? fileSystemId;
        var reader = fileSystemId is null
            ? Readers.FirstOrDefault(candidate => candidate.CanRead(image))
            : Readers.FirstOrDefault(candidate => candidate.Id.Equals(fileSystemId, StringComparison.OrdinalIgnoreCase));
        if (reader is null || !reader.CanRead(image)) { volume = null!; return false; }
        try { volume = reader.Read(image); return true; }
        catch (InvalidDataException) { volume = null!; return false; }
    }
}
