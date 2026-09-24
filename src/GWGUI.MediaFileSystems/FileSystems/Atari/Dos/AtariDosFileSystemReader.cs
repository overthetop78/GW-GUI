using System.Collections.Frozen;
using GWGUI.MediaFileSystems.Constants;

using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.Dos;

/// <summary>Lit les catalogues et chaînes de secteurs Atari DOS.</summary>
public sealed class AtariDosFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.AtariDos;
    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        MediaImageFormatIds.Atari90,
        MediaImageFormatIds.Atari130,
        MediaImageFormatIds.Atari140,
        MediaImageFormatIds.Atari180,
        MediaImageFormatIds.AtariAtx,
        MediaImageFormatIds.AtariXfd90,
        MediaImageFormatIds.AtariXfd130,
        MediaImageFormatIds.AtariXfd140,
        MediaImageFormatIds.AtariXfd180
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    /// <inheritdoc />
    public bool CanRead(IMediaSectorImage image) => IsSupportedFormat(image.FormatId)
        && image.BlockCount >= AtariDosFileSystemLayout.LastDirectorySector
        && AtariDosVtocReader.TrySector(image, AtariDosFileSystemLayout.VtocSector, out var vtoc)
        && AtariDosVtocReader.LooksValid(vtoc, image.BlockCount)
        && AtariDosDirectoryReader.TryLocate(image, out _);
    /// <inheritdoc />
    public FileSystemVolume Read(IMediaSectorImage image)
    {
        if (!CanRead(image)) throw AtariDosFileSystemExceptions.UnsupportedDirectory(image.FormatId, image.BlockSize);
        if (!AtariDosDirectoryReader.TryLocate(image, out var directory))
            throw AtariDosFileSystemExceptions.UnsupportedDirectory(image.FormatId, image.BlockSize);
        var warnings = new List<string>();
        var entries = AtariDosDirectoryReader.Read(image, directory, warnings);
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
            attributes: Attributes(isMyDos, directory));
    }

    private static IReadOnlyList<string> Attributes(bool isMyDos, AtariDosDirectoryLocation directory)
    {
        var attributes = isMyDos
            ? new List<string> { "mydos", "extended-sector-links" }
            : ["atari-dos"];
        if (!directory.IsCanonical) attributes.Add("relocated-directory");
        return attributes;
    }

    private bool IsSupportedFormat(string formatId) =>
        CatalogFormatIds.Contains(formatId)
        || formatId.StartsWith($"{MediaImageFormatIds.AtariPrefix}atr.", StringComparison.OrdinalIgnoreCase);
}
