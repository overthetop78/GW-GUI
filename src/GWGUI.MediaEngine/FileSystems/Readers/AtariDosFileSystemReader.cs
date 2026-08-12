using GWGUI.MediaEngine.Definitions;
using System.Buffers.Binary;
using GWGUI.MediaEngine.FileSystems.Atari.Dos;
using GWGUI.MediaEngine.SectorImages;


using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Readers;

/// <summary>Lit les catalogues et chaînes de secteurs Atari DOS.</summary>
public sealed class AtariDosFileSystemReader : IFileSystemReader
{
    /// <summary>Identifiant technique du lecteur.</summary>
    public string Id => Definitions.FileSystemIds.AtariDos;
    /// <summary>Formats Atari DOS pris en charge.</summary>
    public IReadOnlySet<string> CatalogFormatIds { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { DiskImageFormatIds.Atari90, DiskImageFormatIds.Atari130, DiskImageFormatIds.Atari180 };
    public bool CanRead(SectorImage image) => image.FormatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase) &&
        !image.FormatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase) && image.BlockCount >= 368 &&
        image.TryGetBlock(AtariDosFileSystemLayout.VtocSector - 1, out var vtoc) && LooksLikeVtoc(vtoc.Data) && image.TryGetBlock(AtariDosFileSystemLayout.FirstDirectorySector - 1, out var directory) && LooksLikeDirectory(directory.Data);

    /// <summary>Lit le volume Atari DOS.</summary>
    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw AtariDosFileSystemExceptions.UnsupportedDirectory(image.FormatId, image.BlockSize);
        var warnings = new List<string>(); var entries = new List<FileSystemEntry>();
        for (var sectorNumber = AtariDosFileSystemLayout.FirstDirectorySector; sectorNumber <= AtariDosFileSystemLayout.LastDirectorySector; sectorNumber++)
        {
            if (!TrySector(image, sectorNumber, out var sector)) { warnings.Add(AtariDosFileSystemExceptions.MissingDirectorySector(sectorNumber)); continue; }
            for (var slot = 0; slot < AtariDosFileSystemLayout.DirectoryEntriesPerSector; slot++)
            {
                var offset = slot * AtariDosFileSystemLayout.DirectoryEntrySize; if (offset + AtariDosFileSystemLayout.DirectoryEntrySize > sector.Length) break; var flags = sector[offset + AtariDosFileSystemLayout.FlagsOffset];
                if ((flags & AtariDosFileSystemLayout.InUseFlag) == 0 || (flags & AtariDosFileSystemLayout.DeletedFlag) != 0) continue;
                var sectorCount = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.SectorCountOffset));
                var firstSector = BinaryPrimitives.ReadUInt16LittleEndian(sector.AsSpan(offset + AtariDosFileSystemLayout.FirstSectorOffset));
                var name = DecodeName(sector.AsSpan(offset + AtariDosFileSystemLayout.NameOffset, AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength));
                var content = ReadFile(image, firstSector, sectorCount, slot + (sectorNumber - AtariDosFileSystemLayout.FirstDirectorySector) * AtariDosFileSystemLayout.DirectoryEntriesPerSector, warnings, name);
                entries.Add(new(name, FileSystemEntryKind.File, content.Count, null, string.Empty, flags, firstSector, true, [], content));
            }
        }
        var freeSectors = ReadFreeSectors(image);
        return new(string.Empty, Definitions.FileSystemDisplayNames.AtariDos, image.Capacity, freeSectors < 0 ? 0 : (long)freeSectors * image.BlockSize, null, null,
            entries.OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray(), warnings);
    }

    /// <summary>Reconstruit une chaîne de secteurs Atari DOS.</summary>
    private static IReadOnlyList<byte> ReadFile(SectorImage image, int first, int expectedSectors, int fileNumber, List<string> warnings, string name)
    {
        var result = new List<byte>(); var current = first; var visited = new HashSet<int>(); var count = 0;
        while (current != 0 && count < Math.Max(expectedSectors, 1))
        {
            if (!visited.Add(current)) { warnings.Add(AtariDosFileSystemExceptions.CyclicDataChain(name, current)); break; }
            if (!TrySector(image, current, out var sector)) { warnings.Add(AtariDosFileSystemExceptions.MissingDataSector(name, current)); break; }
            var link = sector.Length - AtariDosFileSystemLayout.LinkByteCount; var storedFile = sector[link] >> AtariDosFileSystemLayout.FileOwnerShift; var next = (sector[link] & AtariDosFileSystemLayout.NextSectorHighMask) << BitPrimitives.BitsPerByte | sector[link + 1];
            var used = Math.Min(sector[link + 2], link); if (storedFile != fileNumber && storedFile != 0) warnings.Add(AtariDosFileSystemExceptions.InconsistentOwner(name, current, fileNumber, storedFile));
            result.AddRange(sector.Take(used)); current = next; count++;
        }
        return result;
    }

    private static int ReadFreeSectors(SectorImage image)
    {
        if (!TrySector(image, AtariDosFileSystemLayout.VtocSector, out var vtoc) || vtoc.Length < AtariDosFileSystemLayout.FreeSectorCountOffset + sizeof(ushort)) return -1;
        return BinaryPrimitives.ReadUInt16LittleEndian(vtoc.AsSpan(AtariDosFileSystemLayout.FreeSectorCountOffset));
    }
    private static bool TrySector(SectorImage image, int sectorNumber, out byte[] data) { if (sectorNumber > 0 && image.TryGetBlock(sectorNumber - 1, out var block)) { data = block.Data.ToArray(); return true; } data = []; return false; }
    private static bool LooksLikeVtoc(IReadOnlyList<byte> data)
        => data.Count >= AtariDosFileSystemLayout.MinimumSectorSize && data[0] == AtariDosFileSystemLayout.VtocMarker;

    private static bool LooksLikeDirectory(IReadOnlyList<byte> data)
    {
        if (data.Count < AtariDosFileSystemLayout.MinimumSectorSize) return false;
        for (var i = 0; i < AtariDosFileSystemLayout.DirectoryEntriesPerSector; i++)
        {
            var offset = i * AtariDosFileSystemLayout.DirectoryEntrySize;
            var flag = data[offset];
            if (flag == 0) continue;

            var nameIsBlank = true;
            for (var j = 0; j < AtariDosFileSystemLayout.NameLength + AtariDosFileSystemLayout.ExtensionLength; j++)
            {
                var value = data[offset + AtariDosFileSystemLayout.NameOffset + j];
                if (value is not (0 or 0x20)) nameIsBlank = false;
                if (value is not (0 or 0x20) && value is not (>= 0x20 and <= 0x7e)) return false;
            }

            if ((flag & AtariDosFileSystemLayout.InUseFlag) != 0 && (flag & AtariDosFileSystemLayout.DeletedFlag) == 0)
            {
                var sectorCount = data[offset + 1] | data[offset + 2] << BitPrimitives.BitsPerByte;
                var firstSector = data[offset + 3] | data[offset + 4] << BitPrimitives.BitsPerByte;
                if (nameIsBlank || sectorCount == 0 || firstSector == 0) return false;
            }
        }
        return true;
    }
    private static string DecodeName(ReadOnlySpan<byte> raw) { var name = System.Text.Encoding.ASCII.GetString(raw[..AtariDosFileSystemLayout.NameLength]).Trim(); var ext = System.Text.Encoding.ASCII.GetString(raw[AtariDosFileSystemLayout.NameLength..]).Trim(); return ext.Length == 0 ? name : name + "." + ext; }
}
