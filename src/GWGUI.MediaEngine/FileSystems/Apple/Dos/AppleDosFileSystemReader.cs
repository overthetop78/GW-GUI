using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Lit les catalogues et fichiers Apple DOS 3.2 et 3.3.</summary>
public sealed class AppleDosFileSystemReader : IFileSystemReader
{
    /// <inheritdoc />
    public string Id => Definitions.FileSystemIds.AppleDos;
    /// <inheritdoc />
    public IReadOnlySet<string> CatalogFormatIds { get; } = new[] { DiskImageFormatIds.AppleIIDos32, DiskImageFormatIds.AppleIIDos33, DiskImageFormatIds.AppleIIAppleDos140 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    /// <inheritdoc />
    public bool CanRead(SectorImage image) => AppleDosVtocReader.TryRead(image, out _);

    /// <inheritdoc />
    public FileSystemVolume Read(SectorImage image)
    {
        if (!AppleDosVtocReader.TryRead(image, out var vtoc) || vtoc is null) throw AppleDosFileSystemExceptions.MissingCatalog(-1, -1);
        var warnings = new List<string>();
        var entries = new List<FileSystemEntry>();
        var visitedCatalog = new HashSet<int>();
        var track = vtoc.CatalogTrack;
        var sector = vtoc.CatalogSector;
        while (track != 0)
        {
            if (!AppleDosFileSystemLayout.IsValidAddress(track, sector, vtoc.Tracks, vtoc.SectorsPerTrack))
            {
                warnings.Add(AppleDosFileSystemWarnings.InvalidAddress("catalog", track, sector));
                break;
            }
            var logical = track * vtoc.SectorsPerTrack + sector;
            if (!visitedCatalog.Add(logical))
            {
                warnings.Add(AppleDosFileSystemExceptions.InvalidCatalogChain(track, sector));
                break;
            }
            if (!image.TryGetBlock(logical, out var catalog) || catalog.Data.Count != AppleDosFileSystemLayout.SectorSize)
            {
                warnings.Add(AppleDosFileSystemExceptions.InvalidCatalogChain(track, sector));
                break;
            }
            var bytes = catalog.Data.ToArray();
            ReadCatalogSector(image, bytes, entries, warnings);
            track = bytes[AppleDosFileSystemLayout.NextTrackOffset];
            sector = bytes[AppleDosFileSystemLayout.NextSectorOffset];
        }
        return new(VolumeName(vtoc.VolumeNumber), Definitions.FileSystemIds.AppleDos, image.Capacity, (long)vtoc.FreeSectorCount * AppleDosFileSystemLayout.SectorSize, null, null, entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase), warnings);
    }

    /// <summary>Lit les entrées valides d'un secteur jusqu'au premier marqueur inutilisé.</summary>
    private static void ReadCatalogSector(SectorImage image, byte[] bytes, ICollection<FileSystemEntry> entries, ICollection<string> warnings)
    {
        for (var entryIndex = 0; entryIndex < AppleDosFileSystemLayout.CatalogEntriesPerSector; entryIndex++)
        {
            var offset = AppleDosFileSystemLayout.CatalogFirstEntryOffset + entryIndex * AppleDosFileSystemLayout.CatalogEntrySize;
            var tsTrack = bytes[offset + AppleDosFileSystemLayout.EntryTrackOffset];
            if (tsTrack == AppleDosFileSystemLayout.UnusedEntryMarker) break;
            if (tsTrack == AppleDosFileSystemLayout.DeletedEntryMarker) continue;
            var tsSector = bytes[offset + AppleDosFileSystemLayout.EntrySectorOffset];
            var rawType = bytes[offset + AppleDosFileSystemLayout.EntryTypeOffset];
            var type = (AppleDosFileType)(rawType & AppleDosFileSystemLayout.ValueMask);
            var name = AppleDosNameCodec.Decode(bytes.AsSpan(offset + AppleDosFileSystemLayout.EntryNameOffset, AppleDosFileSystemLayout.EntryNameLength));
            var declaredSectors = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset + AppleDosFileSystemLayout.EntrySectorCountOffset));
            var file = AppleDosTrackSectorListReader.Read(image, tsTrack, tsSector, warnings, name);
            var sizeValid = declaredSectors == file.TraversedSectorCount;
            if (!sizeValid) warnings.Add(AppleDosFileSystemWarnings.InconsistentSize(name, declaredSectors, file.TraversedSectorCount));
            var content = file.Content;
            uint attributes = rawType;
            if (type == AppleDosFileType.Binary && AppleDosBinaryFileCodec.TryDecode(file.Content, out var decoded, out var loadAddress))
            {
                content = decoded;
                attributes |= (uint)loadAddress << AppleDosFileSystemLayout.BinaryLoadAddressAttributeShift;
            }
            entries.Add(new(name, FileSystemEntryKind.File, content.Count, null, AppleDosFileTypeNames.Get(type), attributes, file.StorageReference, file.IsValid && sizeValid, [], content));
        }
    }

    /// <summary>Construit le nom technique du volume depuis son numéro VTOC.</summary>
    public static string VolumeName(byte number) => $"{AppleDosFileSystemLayout.VolumeNamePrefix}{number:D3}";
}
