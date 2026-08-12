using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Lit les volumes Commodore DOS contenus dans les images 1541, 1571 et 1581.</summary>
public sealed class CommodoreDosFileSystemReader : IFileSystemReader
{
    /// <summary>Obtient l'identifiant technique central du système de fichiers.</summary>
    public string Id => Definitions.FileSystemIds.CommodoreDos;
    /// <summary>Obtient les formats Commodore DOS pris en charge sous forme non modifiable.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.Commodore1541, DiskImageFormatIds.Commodore1571, DiskImageFormatIds.Commodore1581 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    /// <summary>Indique si l'image contient un en-tête et un répertoire Commodore DOS plausibles.</summary>
    public bool CanRead(SectorImage image) => CatalogFormatIds.Contains(image.FormatId) && CommodoreDosRecognizer.TryRecognize(image, out _);

    /// <summary>Lit le nom, le répertoire, les contenus et l'espace libre du volume reconnu.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        if (!CommodoreDosRecognizer.TryRecognize(image, out var recognition)) throw CommodoreDosExceptions.UnsupportedLayout(image.FormatId);
        var nameBytes = new byte[CommodoreDosLayout.NameLength];
        for (var index = 0; index < nameBytes.Length; index++) nameBytes[index] = recognition.Header[recognition.Layout.VolumeNameOffset + index];
        var name = PetsciiCodec.Decode(nameBytes);
        var warnings = new List<string>();
        var entries = CommodoreDosDirectoryReader.Read(image, recognition.DirectoryTrack, recognition.DirectorySector, warnings);
        var freeSpace = recognition.Layout == CommodoreDosLayout.D81 ? Commodore1581BamReader.Read(image, warnings) : Commodore1541BamReader.Read(image, warnings);
        var freeBytes = freeSpace.FreeBlocks is { } freeBlocks ? (long)freeBlocks * CommodoreDosLayout.SectorSize : 0;
        return new(name, Definitions.FileSystemIds.CommodoreDos, image.Capacity, freeBytes, null, null, entries, warnings);
    }
}
