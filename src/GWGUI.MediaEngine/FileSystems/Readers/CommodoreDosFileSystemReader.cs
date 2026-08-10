using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class CommodoreDosFileSystemReader : IFileSystemReader
{
    public string Id => "commodore-dos";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "commodore.1541", "commodore.1571", "commodore.1581" };

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId) || image.BlockSize != 256) return false;
        var headerAddress = image.FormatId == "commodore.1581" ? (40, 0) : (18, 0);
        return TryGetSector(image, headerAddress.Item1, headerAddress.Item2, out var header)
            && header.Length == 256
            && header[2] is 0x41 or 0x44
            && HasPlausibleDirectory(image, header, headerAddress.Item1, image.FormatId == "commodore.1581");
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a supported CBM DOS file system.");
        var is1581 = image.FormatId == "commodore.1581";
        var headerTrack = is1581 ? 40 : 18;
        var headerSector = 0;
        if (!TryGetSector(image, headerTrack, headerSector, out var header)) throw new InvalidDataException("The CBM DOS header sector is missing.");

        var nameOffset = is1581 ? 4 : 0x90;
        var name = Petscii.Decode(header.AsSpan(nameOffset, 16));
        var warnings = new List<string>();
        var directoryTrack = header[0];
        var directorySector = header[1];
        if (directoryTrack == 0)
        {
            directoryTrack = (byte)headerTrack;
            directorySector = (byte)(is1581 ? 3 : 1);
        }
        var entries = ReadDirectory(image, directoryTrack, directorySector, warnings);
        var freeBlocks = ReadFreeBlocks(image, is1581);
        return new(name, "CBM DOS", image.Capacity, Math.Max(0, freeBlocks) * 256L, null, null, entries, warnings);
    }

    private static IReadOnlyList<FileSystemEntry> ReadDirectory(SectorImage image, int firstTrack, int firstSector, List<string> warnings)
    {
        var entries = new List<FileSystemEntry>();
        var visited = new HashSet<(int Track, int Sector)>();
        var track = firstTrack; var sector = firstSector;
        while (track != 0)
        {
            if (!visited.Add((track, sector))) { warnings.Add($"Cyclic CBM DOS directory chain at {track}/{sector}."); break; }
            if (!TryGetSector(image, track, sector, out var data)) { warnings.Add($"CBM DOS directory sector {track}/{sector} is missing."); break; }
            for (var slot = 0; slot < 8; slot++)
            {
                var offset = 2 + slot * 32;
                var rawType = data[offset];
                if ((rawType & 0x0f) == 0) continue;
                var name = Petscii.Decode(data.AsSpan(offset + 3, 16));
                if (name.Length == 0) continue;
                var firstDataTrack = data[offset + 1];
                var firstDataSector = data[offset + 2];
                var declaredBlocks = data[offset + 28] | data[offset + 29] << 8;
                IReadOnlyList<byte> content = [];
                try { content = ReadFile(image, firstDataTrack, firstDataSector, warnings, name); }
                catch (InvalidDataException exception) { warnings.Add($"{name}: {exception.Message}"); }
                var type = rawType & 7;
                var comment = TypeName(type) + (((rawType & 0x80) == 0) ? ", open" : string.Empty) + (((rawType & 0x40) != 0) ? ", locked" : string.Empty);
                entries.Add(new(name, FileSystemEntryKind.File, content.Count == 0 ? declaredBlocks * 254L : content.Count,
                    null, comment, rawType, TryToLogicalBlock(image, firstDataTrack, firstDataSector), true, [], content));
            }
            track = data[0]; sector = data[1];
        }
        return entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<byte> ReadFile(SectorImage image, int firstTrack, int firstSector, List<string> warnings, string name)
    {
        if (firstTrack == 0) return [];
        using var stream = new MemoryStream();
        var visited = new HashSet<(int Track, int Sector)>();
        var track = firstTrack; var sector = firstSector;
        while (track != 0)
        {
            if (!visited.Add((track, sector))) throw new InvalidDataException($"Cyclic data chain at {track}/{sector}.");
            if (!TryGetSector(image, track, sector, out var data)) throw new InvalidDataException($"Data sector {track}/{sector} is missing.");
            var nextTrack = data[0]; var nextSector = data[1];
            var used = nextTrack == 0 ? Math.Clamp(nextSector - 1, 0, 254) : 254;
            stream.Write(data, 2, used);
            track = nextTrack; sector = nextSector;
            if (stream.Length > image.Capacity) { warnings.Add($"{name}: file chain exceeds image capacity."); break; }
        }
        return stream.ToArray();
    }

    private static int ReadFreeBlocks(SectorImage image, bool is1581)
    {
        if (!is1581)
        {
            var tracksPerSide = image.Cylinders;
            var total = 0;
            if (TryGetSector(image, 18, 0, out var bam))
                for (var track = 1; track <= Math.Min(35, tracksPerSide); track++) total += bam[4 + (track - 1) * 4];
            if (image.Heads > 1 && TryGetSector(image, 18 + tracksPerSide, 0, out var secondBam))
                for (var track = 1; track <= Math.Min(35, tracksPerSide); track++) total += secondBam[4 + (track - 1) * 4];
            return total;
        }
        var free = 0;
        foreach (var bamSector in new[] { 1, 2 })
        {
            if (!TryGetSector(image, 40, bamSector, out var bam)) continue;
            for (var entry = 0; entry < 40; entry++)
            {
                var offset = 16 + entry * 6;
                if (offset < bam.Length) free += bam[offset];
            }
        }
        return free;
    }

    internal static bool TryGetSector(SectorImage image, int track, int sector, out byte[] data)
    {
        try
        {
            var logical = ToLogicalBlock(image, track, sector);
            if (image.TryGetBlock(logical, out var block) && block.Data.Count == 256) { data = block.Data.ToArray(); return true; }
        }
        catch (ArgumentOutOfRangeException) { }
        data = []; return false;
    }

    internal static int ToLogicalBlock(SectorImage image, int track, int sector)
    {
        if (image.FormatId == "commodore.1581") return CommodoreGeometry.To1581LogicalBlock(track, sector);
        var tracksPerSide = image.Cylinders;
        var side = track > tracksPerSide ? 1 : 0;
        var sideTrack = side == 0 ? track : track - tracksPerSide;
        return CommodoreGeometry.To1541LogicalBlock(sideTrack, sector, tracksPerSide, side);
    }

    private static int TryToLogicalBlock(SectorImage image, int track, int sector)
    {
        try { return ToLogicalBlock(image, track, sector); }
        catch (ArgumentOutOfRangeException) { return -1; }
    }

    private static bool HasPlausibleDirectory(SectorImage image, byte[] header, int headerTrack, bool is1581)
    {
        var track = header[0] == 0 ? headerTrack : header[0];
        var sector = header[0] == 0 ? (is1581 ? 3 : 1) : header[1];
        var visited = new HashSet<(int Track, int Sector)>();
        var valid = 0;
        var invalid = 0;
        while (track != 0 && visited.Count < 64 && visited.Add((track, sector)))
        {
            if (!TryGetSector(image, track, sector, out var data)) return false;
            for (var slot = 0; slot < 8; slot++)
            {
                var offset = 2 + slot * 32;
                var rawType = data[offset];
                if ((rawType & 0x0f) == 0) continue;
                var type = rawType & 7;
                var name = Petscii.Decode(data.AsSpan(offset + 3, 16));
                var dataTrack = data[offset + 1];
                var dataSector = data[offset + 2];
                var plausible = type is >= 1 and <= 5 && name.Length > 0 && !name.Contains('\ufffd')
                    && (dataTrack == 0 || TryToLogicalBlock(image, dataTrack, dataSector) >= 0);
                if (plausible) valid++; else invalid++;
            }
            track = data[0];
            sector = data[1];
        }
        return invalid == 0 && (valid > 0 || visited.Count == 1);
    }

    private static string TypeName(int type) => type switch { 1 => "SEQ", 2 => "PRG", 3 => "USR", 4 => "REL", 5 => "CBM", _ => "DEL" };

    private static class Petscii
    {
        public static string Decode(ReadOnlySpan<byte> bytes)
        {
            var chars = new List<char>(bytes.Length);
            foreach (var raw in bytes)
            {
                if (raw is 0 or 0xa0) break;
                var value = (byte)(raw & 0x7f);
                chars.Add(value switch
                {
                    >= 0x20 and <= 0x5f => (char)value,
                    >= 0x60 and <= 0x7a => (char)(value - 0x20),
                    _ => '�'
                });
            }
            return new string(chars.ToArray()).Trim();
        }
    }
}
