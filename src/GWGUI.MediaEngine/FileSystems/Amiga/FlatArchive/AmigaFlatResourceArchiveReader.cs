using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;

/// <summary>Projette une archive Amiga de ressources concaténées sous forme de volume en lecture seule.</summary>
public sealed class AmigaFlatResourceArchiveReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => FileSystemIds.AmigaFlatResourceArchive;

    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        DiskImageFormatIds.AmigaDos,
        DiskImageFormatIds.AmigaDosHighDensity
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool CanRead(SectorImage image) => AmigaFlatResourceDirectoryReader.TryRead(image, out _);

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!AmigaFlatResourceDirectoryReader.TryRead(image, out var descriptors))
            throw AmigaFlatResourceArchiveExceptions.InvalidDirectory();

        var entries = new List<FileSystemEntry>();
        var warnings = new List<string>();
        long offset = descriptors[0].Length;
        foreach (var descriptor in descriptors.Skip(1))
        {
            var read = AmigaFlatResourceDataReader.Read(image, offset, descriptor.Length);
            if (read.MissingBlocks.Count > 0) warnings.Add(AmigaFlatResourceArchiveWarnings.MissingBlocks(descriptor.Name, read.MissingBlocks));
            if (read.InvalidBlocks.Count > 0) warnings.Add(AmigaFlatResourceArchiveWarnings.InvalidBlocks(descriptor.Name, read.InvalidBlocks));
            entries.Add(new(descriptor.Name, FileSystemEntryKind.File, descriptor.Length, null,
                AmigaFlatResourceArchiveLayout.EntryComment, 0, checked((int)(offset / image.BlockSize)), true, [], read.Bytes));
            offset += descriptor.Length;
        }

        return new(string.Empty, Id, image.Capacity, Math.Max(0, image.Capacity - offset), null, null, entries, warnings);
    }
}
