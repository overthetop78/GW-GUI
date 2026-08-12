using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Lit les catalogues et chaînes de secteurs Atari DOS.</summary>
public sealed class AtariDosFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.AtariDos;
    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.Atari90, DiskImageFormatIds.Atari130, DiskImageFormatIds.Atari180 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    /// <inheritdoc />
    public bool CanRead(SectorImage image) => CatalogFormatIds.Contains(image.FormatId) && image.BlockCount >= AtariDosFileSystemLayout.LastDirectorySector && AtariDosVtocReader.TrySector(image, AtariDosFileSystemLayout.VtocSector, out var vtoc) && AtariDosVtocReader.LooksValid(vtoc) && AtariDosVtocReader.TrySector(image, AtariDosFileSystemLayout.FirstDirectorySector, out var directory) && AtariDosDirectoryReader.LooksValid(directory);
    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw AtariDosFileSystemExceptions.UnsupportedDirectory(image.FormatId, image.BlockSize);
        var warnings = new List<string>();
        var entries = AtariDosDirectoryReader.Read(image, warnings);
        var freeSectors = AtariDosVtocReader.ReadFreeSectors(image);
        return new(string.Empty, Definitions.FileSystemIds.AtariDos, image.Capacity, freeSectors.HasValue ? Math.Max(0L, (long)freeSectors.Value * image.BlockSize) : 0, null, null, entries, warnings);
    }
}
