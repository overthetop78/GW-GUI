using System.Collections.Frozen;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

/// <summary>Détecte et lit les volumes SpartaDOS 1.x et 2.x.</summary>
public sealed class SpartaDosFileSystemReader : IFileSystemReader
{
    public string Id => Definitions.FileSystemIds.AtariSpartaDos;

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

    public bool CanRead(IMediaSectorImage image) => SpartaDosDiskReader.TryReadHeader(image, out _);

    public FileSystemVolume Read(IMediaSectorImage image)
    {
        if (!SpartaDosDiskReader.TryReadHeader(image, out var header))
            throw SpartaDosFileSystemExceptions.Unsupported(image.FormatId);
        var warnings = new List<string>();
        var entries = SpartaDosDirectoryReader.ReadRoot(image, header, warnings);
        var attributes = new List<string>
        {
            SpartaDosFileSystemLayout.VolumeAttribute,
            header.Version == SpartaDosFileSystemLayout.Version1
                ? SpartaDosFileSystemLayout.Version1Attribute
                : SpartaDosFileSystemLayout.Version2Attribute
        };
        if (header.WriteProtected) attributes.Add(SpartaDosFileSystemLayout.WriteProtectedAttribute);
        var capacity = checked((long)header.TotalSectors * header.SectorSize);
        var freeBytes = checked((long)header.FreeSectors * header.SectorSize);
        return new(
            header.VolumeName,
            Definitions.FileSystemIds.AtariSpartaDos,
            capacity,
            freeBytes,
            null,
            null,
            entries,
            warnings,
            attributes: attributes,
            bootable: header.BootFileMapSector != 0,
            fileSystemDisplayName: Definitions.FileSystemDisplayNames.AtariSpartaDos);
    }
}
