using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.FileSystems;

public sealed class FileSystemRegistry
{
    public IReadOnlyList<IFileSystemReader> Readers { get; } =
    [
        new Readers.AmigaDosFileSystemReader(),
        new Readers.AmstradCpmFileSystemReader(),
        new Readers.CpmFileSystemReader(),
        new Readers.CommodoreDosFileSystemReader(),
        new Readers.Fat12FileSystemReader(),
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
        IFileSystemReader? reader;
        if (fileSystemId is null) reader = Readers.FirstOrDefault(candidate => candidate.CanRead(image));
        else
        {
            reader = Readers.FirstOrDefault(candidate => candidate.Id.Equals(fileSystemId, StringComparison.OrdinalIgnoreCase));
            if (reader is null)
                reader = Readers.FirstOrDefault(candidate => candidate.CatalogFormatIds.Contains(fileSystemId) && candidate.CanRead(image));
        }
        if (reader is null || !reader.CanRead(image)) { volume = null!; return false; }
        try { volume = reader.Read(image); return true; }
        catch (InvalidDataException) { volume = null!; return false; }
    }
}
