using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using System.Text;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Readers;

public sealed class AtariDosFileSystemReader : IFileSystemReader
{
    public string Id => "atari-dos";
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.Atari90, DiskImageFormatIds.Atari130, DiskImageFormatIds.Atari180 };
    public bool CanRead(SectorImage image) => image.FormatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) &&
        !image.FormatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) && image.BlockCount >= 368 &&
        image.TryGetBlock(359, out var vtoc) && LooksLikeVtoc(vtoc.Data) &&
        image.TryGetBlock(360, out var directory) && LooksLikeDirectory(directory.Data);

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The image does not contain a supported Atari DOS directory.");
        var warnings = new List<string>(); var entries = new List<FileSystemEntry>();
        for (var sectorNumber = 361; sectorNumber <= 368; sectorNumber++)
        {
            if (!TrySector(image, sectorNumber, out var sector)) { warnings.Add($"Directory sector {sectorNumber} is missing."); continue; }
            for (var slot = 0; slot < 8; slot++)
            {
                var offset = slot * 16; if (offset + 16 > sector.Length) break; var flags = sector[offset];
                if ((flags & 0x40) == 0 || (flags & 0x80) != 0) continue;
                var sectorCount = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 1));
                var firstSector = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + 3));
                var name = DecodeName(sector.AsSpan(offset + 5, 11));
                var content = ReadFile(image, firstSector, sectorCount, slot + (sectorNumber - 361) * 8, warnings);
                entries.Add(new(name, FileSystemEntryKind.File, content.Count, null, string.Empty, flags, firstSector, true, [], content));
            }
        }
        var freeSectors = ReadFreeSectors(image);
        return new(string.Empty, "Atari DOS", image.Capacity, freeSectors < 0 ? 0 : (long)freeSectors * image.BlockSize, null, null,
            entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    private static IReadOnlyList<byte> ReadFile(SectorImage image, int first, int expectedSectors, int fileNumber, List<string> warnings)
    {
        var result = new List<byte>(); var current = first; var visited = new HashSet<int>(); var count = 0;
        while (current != 0 && count < Math.Max(expectedSectors, 1))
        {
            if (!visited.Add(current) || !TrySector(image, current, out var sector)) { warnings.Add($"{current}: invalid or missing Atari DOS data sector."); break; }
            var link = sector.Length - 3; var storedFile = sector[link] >> 2; var next = (sector[link] & 3) << BitPrimitives.BitsPerByte | sector[link + 1];
            var used = Math.Min(sector[link + 2], link); if (storedFile != fileNumber && storedFile != 0) warnings.Add($"Sector {current} belongs to file {storedFile}, expected {fileNumber}.");
            result.AddRange(sector.Take(used)); current = next; count++;
        }
        return result;
    }

    private static int ReadFreeSectors(SectorImage image)
    {
        if (!TrySector(image, 360, out var vtoc) || vtoc.Length < 5) return -1;
        return BinaryPrimitives.ReadUInt16LittleEndian(vtoc.AsSpan(3));
    }
    private static bool TrySector(SectorImage image, int sectorNumber, out byte[] data) { if (sectorNumber > 0 && image.TryGetBlock(sectorNumber - 1, out var block)) { data = block.Data.ToArray(); return true; } data = []; return false; }
    private static bool LooksLikeVtoc(IReadOnlyList<byte> data)
        => data.Count >= 128 && data[0] == 2;

    private static bool LooksLikeDirectory(IReadOnlyList<byte> data)
    {
        if (data.Count < 128) return false;
        for (var i = 0; i < 8; i++)
        {
            var offset = i * 16;
            var flag = data[offset];
            if (flag == 0) continue;

            var nameIsBlank = true;
            for (var j = 0; j < 11; j++)
            {
                var value = data[offset + 5 + j];
                if (value is not (0 or 0x20)) nameIsBlank = false;
                if (value is not (0 or 0x20) && value is not (>= 0x20 and <= 0x7e)) return false;
            }

            if ((flag & 0x40) != 0 && (flag & 0x80) == 0)
            {
                var sectorCount = data[offset + 1] | data[offset + 2] << BitPrimitives.BitsPerByte;
                var firstSector = data[offset + 3] | data[offset + 4] << BitPrimitives.BitsPerByte;
                if (nameIsBlank || sectorCount == 0 || firstSector == 0) return false;
            }
        }
        return true;
    }
    private static string DecodeName(ReadOnlySpan<byte> raw) { var name = System.Text.Encoding.ASCII.GetString(raw[..8]).Trim(); var ext = System.Text.Encoding.ASCII.GetString(raw[8..]).Trim(); return ext.Length == 0 ? name : name + "." + ext; }
}
