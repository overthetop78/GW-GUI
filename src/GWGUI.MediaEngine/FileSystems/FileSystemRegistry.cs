using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems;

public sealed class FileSystemRegistry
{
    public sealed record Match(string ReaderId, FileSystemVolume Volume);

    public IReadOnlyList<IFileSystemReader> Readers { get; } =
    [
        new Readers.AmigaDosFileSystemReader(),
        new Acorn.Adfs.AcornAdfsFileSystemReader(),
        new Readers.BbcDfsFileSystemReader(),
        new Readers.CoherentFileSystemReader(),
        new Readers.Rt11FileSystemReader(),
        new Readers.UcsdFileSystemReader(),
        new Readers.AppleInformXzipFileSystemReader(),
        new Readers.AppleDosFileSystemReader(),
        new Readers.ProDosFileSystemReader(),
        new Readers.MacMfsFileSystemReader(),
        new Readers.MacHfsFileSystemReader(),
        new Readers.LisaFileSystemReader(),
        new Cpm.AmstradCpmFileSystemReader(),
        new Cpm.CpmFileSystemReader(),
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

    public IReadOnlyList<Match> ReadAll(SectorImage image)
    {
        var matches = new List<Match>();
        foreach (var reader in Readers)
        {
            if (!reader.CanRead(image)) continue;
            try { matches.Add(new(reader.Id, reader.Read(image))); }
            catch (InvalidDataException) { }
        }
        return matches;
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
