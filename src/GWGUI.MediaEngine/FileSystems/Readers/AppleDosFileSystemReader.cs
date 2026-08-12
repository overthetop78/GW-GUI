using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.FileSystems.Apple.Dos;


namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit les catalogues et fichiers Apple DOS 3.2 et 3.3.</summary>
public sealed class AppleDosFileSystemReader : IFileSystemReader
{
    /// <summary>Identifiant technique du lecteur Apple DOS.</summary>
    public string Id => Definitions.FileSystemIds.AppleDos;
    /// <summary>Formats Apple DOS pris en charge.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AppleIIDos32, DiskImageFormatIds.AppleIIDos33, DiskImageFormatIds.AppleIIAppleDos140 };

    /// <summary>Indique si l'image contient un VTOC Apple DOS valide.</summary>
    public bool CanRead(SectorImage image)
    {
        var sectors = image.SectorsPerTrack;
        if (image.BlockSize != AppleDosFileSystemLayout.SectorSize || sectors is not (AppleDosFileSystemLayout.Dos32SectorsPerTrack or AppleDosFileSystemLayout.Dos33SectorsPerTrack) || image.BlockCount < AppleDosFileSystemLayout.TrackCount * sectors || !image.TryGetBlock(AppleDosFileSystemLayout.VtocTrack * sectors, out var vtoc)) return false;
        return AppleDosFileSystemLayout.IsValidVtoc(vtoc.Data.ToArray(), AppleDosFileSystemLayout.TrackCount, sectors);
    }

    /// <summary>Lit le catalogue et reconstruit les fichiers Apple DOS.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image))
        {
            var vtocBlock = AppleDosFileSystemLayout.VtocTrack * image.SectorsPerTrack;
            var observedTrack = -1;
            var observedSector = -1;
            if (image.TryGetBlock(vtocBlock, out var candidate) && candidate.Data.Count > AppleDosFileSystemLayout.VtocCatalogSectorOffset)
            {
                observedTrack = candidate.Data[AppleDosFileSystemLayout.VtocCatalogTrackOffset];
                observedSector = candidate.Data[AppleDosFileSystemLayout.VtocCatalogSectorOffset];
            }
            throw AppleDosFileSystemExceptions.MissingCatalog(observedTrack, observedSector);
        }
        var sectors = image.SectorsPerTrack;
        var vtoc = image.GetBlock(AppleDosFileSystemLayout.VtocTrack * sectors).Span;
        var tracks = vtoc[AppleDosFileSystemLayout.VtocTrackCountOffset];
        var warnings = new List<string>(); var entries = new List<FileSystemEntry>(); var visitedCatalog = new HashSet<int>();
        var track = vtoc[AppleDosFileSystemLayout.VtocCatalogTrackOffset];
        var sector = vtoc[AppleDosFileSystemLayout.VtocCatalogSectorOffset];
        while (track != 0)
        {
            var logical = track * sectors + sector;
            if (!visitedCatalog.Add(logical) || !image.TryGetBlock(logical, out var catalog)) { warnings.Add(AppleDosFileSystemExceptions.InvalidCatalogChain(track, sector)); break; }
            var bytes = catalog.Data.ToArray();
            for (var entryIndex = 0; entryIndex < AppleDosFileSystemLayout.CatalogEntriesPerSector; entryIndex++)
            {
                // Apple DOS stops scanning the current catalog sector at the first
                // unused entry. Bytes beyond it are not catalog entries and may
                // contain stale data from an earlier disk/file layout.
                var offset = AppleDosFileSystemLayout.CatalogFirstEntryOffset + entryIndex * AppleDosFileSystemLayout.CatalogEntrySize;
                var tsTrack = bytes[offset + AppleDosFileSystemLayout.EntryTrackOffset];
                if (tsTrack == 0) break;
                if (tsTrack == AppleDosFileSystemLayout.DeletedEntryMarker) continue;
                var tsSector = bytes[offset + AppleDosFileSystemLayout.EntrySectorOffset];
                var type = (AppleDosFileType)(bytes[offset + AppleDosFileSystemLayout.EntryTypeOffset] & AppleDosFileSystemLayout.ValueMask);
                var name = DecodeName(bytes.AsSpan(offset + AppleDosFileSystemLayout.EntryNameOffset, AppleDosFileSystemLayout.EntryNameLength));
                var declaredSectors = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset + AppleDosFileSystemLayout.EntrySectorCountOffset));
                var content = ReadFile(image, sectors, tsTrack, tsSector, warnings, name);
                entries.Add(new(name, FileSystemEntryKind.File, content.Count, null, AppleDosFileTypeNames.Get(type), (byte)type, logical, true, [], content));
                if (declaredSectors > 0 && content.Count > declaredSectors * (long)AppleDosFileSystemLayout.SectorSize) warnings.Add(AppleDosFileSystemExceptions.InconsistentCatalogSize(name));
            }
            track = bytes[AppleDosFileSystemLayout.VtocCatalogTrackOffset];
            sector = bytes[AppleDosFileSystemLayout.VtocCatalogSectorOffset];
        }
        var free = CountFree(vtoc, tracks, sectors);
        return new($"DOS-{vtoc[AppleDosFileSystemLayout.VtocVolumeNumberOffset]:D3}", sectors == AppleDosFileSystemLayout.Dos32SectorsPerTrack ? Definitions.FileSystemIds.AppleDos : Definitions.FileSystemIds.AppleDos, image.Capacity, (long)free * AppleDosFileSystemLayout.SectorSize, null, null,
            entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    /// <summary>Reconstruit un fichier depuis sa chaîne de secteurs de listes T/S.</summary>
    private static IReadOnlyList<byte> ReadFile(SectorImage image, int sectorsPerTrack, int track, int sector, List<string> warnings, string name)
    {
        using var output = new MemoryStream(); var visited = new HashSet<int>();
        while (track != 0)
        {
            var logical = track * sectorsPerTrack + sector;
            if (!visited.Add(logical)) { warnings.Add(AppleDosFileSystemExceptions.CyclicTrackSectorList(name, track, sector)); break; }
            if (!image.TryGetBlock(logical, out var list)) { warnings.Add(AppleDosFileSystemExceptions.MissingTrackSectorList(name, track, sector)); break; }
            var data = list.Data.ToArray();
            for (var pairIndex = 0; pairIndex < AppleDosFileSystemLayout.TrackSectorPairCount; pairIndex++)
            {
                var offset = AppleDosFileSystemLayout.TrackSectorPairsOffset + pairIndex * AppleDosFileSystemLayout.TrackSectorPairSize;
                var dataTrack = data[offset]; var dataSector = data[offset + 1]; if (dataTrack == 0) continue;
                var dataLogical = dataTrack * sectorsPerTrack + dataSector;
                if (!image.TryGetBlock(dataLogical, out var block)) { warnings.Add(AppleDosFileSystemExceptions.MissingDataSector(name, dataTrack, dataSector)); continue; }
                output.Write(block.Data.ToArray());
            }
            track = data[1]; sector = data[2];
        }
        return output.ToArray();
    }

    /// <summary>Compte les secteurs libres décrits par le bitmap du VTOC.</summary>
    private static int CountFree(ReadOnlySpan<byte> vtoc, int tracks, int sectors)
    {
        var free = 0;
        for (var track = 0; track < tracks && AppleDosFileSystemLayout.VtocFreeBitmapOffset + track * AppleDosFileSystemLayout.VtocTrackBitmapSize + AppleDosFileSystemLayout.VtocTrackBitmapSize - 1 < vtoc.Length; track++)
        {
            var bits = BinaryPrimitives.ReadUInt32BigEndian(vtoc.Slice(AppleDosFileSystemLayout.VtocFreeBitmapOffset + track * AppleDosFileSystemLayout.VtocTrackBitmapSize, AppleDosFileSystemLayout.VtocTrackBitmapSize));
            for (var sector = 0; sector < sectors; sector++) if ((bits & (1u << sector)) != 0) free++;
        }
        return free;
    }

    /// <summary>Décode un nom Apple DOS en retirant son bit fort.</summary>
    private static string DecodeName(ReadOnlySpan<byte> raw) => System.Text.Encoding.ASCII.GetString(raw.ToArray().Select(value => (byte)(value & AppleDosFileSystemLayout.ValueMask)).ToArray()).TrimEnd(' ', '\0');
}
