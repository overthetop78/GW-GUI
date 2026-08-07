using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.FileSystems;

public sealed class FileSystemRegistry
{
    public IReadOnlyList<IFileSystemReader> Readers { get; } = [new Readers.AmigaDosFileSystemReader()];

    public FileSystemVolume Read(SectorImage image, string? fileSystemId = null)
    {
        if (fileSystemId is not null && fileSystemId.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase)) fileSystemId = "amigados";
        var reader = fileSystemId is null
            ? Readers.FirstOrDefault(candidate => candidate.CanRead(image))
            : Readers.FirstOrDefault(candidate => candidate.Id.Equals(fileSystemId, StringComparison.OrdinalIgnoreCase));
        if (reader is null) throw new InvalidDataException("No supported file system was detected in the disk image.");
        return reader.Read(image);
    }
}
