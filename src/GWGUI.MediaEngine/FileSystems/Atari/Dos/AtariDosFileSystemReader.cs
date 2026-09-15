using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Lit les catalogues et chaînes de secteurs Atari DOS.</summary>
public sealed class AtariDosFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.AtariDos;
    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        DiskImageFormatIds.Atari90,
        DiskImageFormatIds.Atari130,
        DiskImageFormatIds.Atari140,
        DiskImageFormatIds.Atari180,
        DiskImageFormatIds.AtariAtx,
        DiskImageFormatIds.AtariXfd90,
        DiskImageFormatIds.AtariXfd130,
        DiskImageFormatIds.AtariXfd140,
        DiskImageFormatIds.AtariXfd180
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    /// <inheritdoc />
    public bool CanRead(SectorImage image) => IsSupportedFormat(image.FormatId) && image.BlockCount >= AtariDosFileSystemLayout.LastDirectorySector && AtariDosVtocReader.TrySector(image, AtariDosFileSystemLayout.VtocSector, out var vtoc) && AtariDosVtocReader.LooksValid(vtoc, image.BlockCount) && AtariDosDirectoryReader.ContainsRecordedEntry(image);
    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw AtariDosFileSystemExceptions.UnsupportedDirectory(image.FormatId, image.BlockSize);
        var warnings = new List<string>();
        var entries = AtariDosDirectoryReader.Read(image, warnings);
        var freeSectors = AtariDosVtocReader.ReadFreeSectors(image);
        var isMyDos = AtariDosVtocReader.TrySector(image, AtariDosFileSystemLayout.VtocSector, out var vtoc)
            && vtoc.Length > 0
            && vtoc[0] >= AtariDosFileSystemLayout.MinimumExtendedVtocCode;
        return new(
            string.Empty,
            isMyDos ? Definitions.FileSystemIds.AtariMyDos : Definitions.FileSystemIds.AtariDos,
            image.Capacity,
            freeSectors.HasValue ? Math.Clamp((long)freeSectors.Value * image.BlockSize, 0, image.Capacity) : 0,
            null,
            null,
            entries,
            warnings,
            attributes: isMyDos ? ["mydos", "extended-sector-links"] : ["atari-dos"]);
    }

    private bool IsSupportedFormat(string formatId) =>
        CatalogFormatIds.Contains(formatId)
        || formatId.StartsWith($"{DiskImageFormatIds.AtariPrefix}atr.", StringComparison.OrdinalIgnoreCase);
}
