using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.FileSystems.AppleDos;
using GWGUI.MediaEngine.Geometries.Apple;


namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class AppleDosFileSystemReader : IFileSystemReader
{
    public string Id => "apple-dos";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.AppleIIDos32, DiskImageFormatIds.AppleIIDos33, DiskImageFormatIds.AppleIIAppleDos140 };

    public bool CanRead(SectorImage image)
    {
        var sectors = image.SectorsPerTrack;
        if (image.BlockSize != AppleIIGeometry.SectorSize || sectors is not (13 or AppleIIGeometry.SectorsPerTrack) || image.BlockCount < AppleIIGeometry.TrackCount * sectors || !image.TryGetBlock(AppleDosVtoc.Track * sectors, out var vtoc)) return false;
        return AppleDosVtoc.IsValid(vtoc.Data.ToArray(), AppleIIGeometry.TrackCount, sectors, AppleIIGeometry.SectorSize);
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain an Apple DOS catalog.");
        var sectors = image.SectorsPerTrack; var vtoc = image.GetBlock(17 * sectors).Span; var tracks = vtoc[0x34];
        var warnings = new List<string>(); var entries = new List<FileSystemEntry>(); var visitedCatalog = new HashSet<int>();
        var track = vtoc[1]; var sector = vtoc[2];
        while (track != 0)
        {
            var logical = track * sectors + sector;
            if (!visitedCatalog.Add(logical) || !image.TryGetBlock(logical, out var catalog)) { warnings.Add($"Catalog sector T{track} S{sector} is missing or cyclic."); break; }
            var bytes = catalog.Data.ToArray();
            for (var offset = 0x0b; offset + 35 <= bytes.Length; offset += 35)
            {
                // Apple DOS stops scanning the current catalog sector at the first
                // unused entry. Bytes beyond it are not catalog entries and may
                // contain stale data from an earlier disk/file layout.
                var tsTrack = bytes[offset];
                if (tsTrack == 0) break;
                if (tsTrack == 0xff) continue;
                var tsSector = bytes[offset + 1]; var type = bytes[offset + 2];
                var name = DecodeName(bytes.AsSpan(offset + 3, 30));
                var declaredSectors = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset + 33));
                var content = ReadFile(image, sectors, tsTrack, tsSector, warnings, name);
                entries.Add(new(name, FileSystemEntryKind.File, content.Count, null, TypeName(type), type, logical, true, [], content));
                if (declaredSectors > 0 && content.Count > declaredSectors * 256L) warnings.Add($"{name}: catalog size is inconsistent.");
            }
            track = bytes[1]; sector = bytes[2];
        }
        var free = CountFree(vtoc, tracks, sectors);
        return new($"DOS-{vtoc[6]:D3}", sectors == 13 ? "Apple DOS 3.2" : "Apple DOS 3.3", image.Capacity, (long)free * 256, null, null,
            entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    private static IReadOnlyList<byte> ReadFile(SectorImage image, int sectorsPerTrack, int track, int sector, List<string> warnings, string name)
    {
        using var output = new MemoryStream(); var visited = new HashSet<int>();
        while (track != 0)
        {
            var logical = track * sectorsPerTrack + sector;
            if (!visited.Add(logical) || !image.TryGetBlock(logical, out var list)) { warnings.Add($"{name}: T/S list T{track} S{sector} is missing or cyclic."); break; }
            var data = list.Data.ToArray();
            for (var offset = 0x0c; offset + 1 < data.Length; offset += 2)
            {
                var dataTrack = data[offset]; var dataSector = data[offset + 1]; if (dataTrack == 0) continue;
                var dataLogical = dataTrack * sectorsPerTrack + dataSector;
                if (!image.TryGetBlock(dataLogical, out var block)) { warnings.Add($"{name}: data sector T{dataTrack} S{dataSector} is missing."); continue; }
                output.Write(block.Data.ToArray());
            }
            track = data[1]; sector = data[2];
        }
        return output.ToArray();
    }

    private static int CountFree(ReadOnlySpan<byte> vtoc, int tracks, int sectors)
    {
        var free = 0;
        for (var track = 0; track < tracks && 0x38 + track * 4 + 3 < vtoc.Length; track++)
        {
            var bits = BinaryPrimitives.ReadUInt32BigEndian(vtoc.Slice(0x38 + track * 4, 4));
            for (var sector = 0; sector < sectors; sector++) if ((bits & (1u << sector)) != 0) free++;
        }
        return free;
    }

    private static string DecodeName(ReadOnlySpan<byte> raw) => System.Text.Encoding.ASCII.GetString(raw.ToArray().Select(value => (byte)(value & 0x7f)).ToArray()).TrimEnd(' ', '\0');
    private static string TypeName(byte type) => (type & 0x7f) switch { 0 => "Text", 1 => "Integer BASIC", 2 => "Applesoft BASIC", 4 => "Binary", 8 => "S", 16 => "Relocatable", 32 => "A", 64 => "B", _ => "File" };
}
