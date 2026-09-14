using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.PrintShop;

/// <summary>Reads Atari Print Shop graphics-library disks.</summary>
public sealed class AtariPrintShopFileSystemReader : IFileSystemReader
{
    private const int HeaderSector = 361;
    private const int FirstDirectorySector = 362;
    private const int LastDirectorySector = 393;
    private const int DirectoryEntryLength = 32;
    private const int DataLengthPerSector = 126;
    private const int DefaultIconLength = 572;
    private static readonly byte[] Signature = System.Text.Encoding.ASCII.GetBytes("PRINT SHOP:CLK!");

    public string Id => FileSystemIds.AtariPrintShop;

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

    public bool CanRead(SectorImage image) =>
        CatalogFormatIds.Contains(image.FormatId)
        && image.BlockSize == 128
        && image.BlockCount >= LastDirectorySector
        && TrySector(image, HeaderSector, out var header)
        && header.AsSpan(0, Signature.Length).SequenceEqual(Signature)
        && EnumerateDirectoryRecords(image).Any(record => record.IsActive && record.IsValid(image.BlockCount));

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image)) throw new InvalidDataException("The sector image is not an Atari Print Shop graphics disk.");

        var entries = new List<FileSystemEntry>();
        var occupiedSectors = new HashSet<int>(Enumerable.Range(360, LastDirectorySector - 359));
        foreach (var record in EnumerateDirectoryRecords(image).Where(record => record.IsActive && record.IsValid(image.BlockCount)))
        {
            var content = ReadFile(image, record, occupiedSectors, out var dataValid, out var occupiedSectorCount);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["container"] = FileSystemIds.AtariPrintShop,
                ["startSector"] = record.StartSector.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["typeId"] = ((char)record.TypeId).ToString(),
                ["declaredLength"] = record.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            entries.Add(new FileSystemEntry(
                record.Name,
                FileSystemEntryKind.File,
                content.Length,
                null,
                "Atari Print Shop icon",
                record.TypeId,
                record.StartSector,
                true,
                [],
                content,
                nativeTypeId: "atari-print-shop-icon",
                occupiedSize: (long)occupiedSectorCount * image.BlockSize,
                attributes: ["graphics"],
                dataValid: dataValid,
                diagnostics: dataValid ? [] : [$"The catalog declares {record.Length} bytes, but the sector chain contains {content.Length} bytes."],
                metadata: metadata));
        }

        var freeSectors = Math.Max(0, image.BlockCount - occupiedSectors.Count);
        return new FileSystemVolume(
            FileSystemDisplayNames.AtariPrintShop,
            FileSystemIds.AtariPrintShop,
            image.Capacity,
            (long)freeSectors * image.BlockSize,
            null,
            null,
            entries,
            [],
            attributes: ["graphics-library"],
            bootable: false);
    }

    private static IEnumerable<DirectoryRecord> EnumerateDirectoryRecords(SectorImage image)
    {
        for (var sector = FirstDirectorySector; sector <= LastDirectorySector; sector++)
        {
            if (!TrySector(image, sector, out var bytes)) yield break;
            for (var offset = 0; offset + DirectoryEntryLength <= bytes.Length; offset += DirectoryEntryLength)
            {
                var name = System.Text.Encoding.ASCII.GetString(bytes, offset, 16).TrimEnd(' ', '\0');
                var startSector = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset + 16, 2));
                var typeId = bytes[offset + 19];
                var length = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset + 20, 2));
                yield return new DirectoryRecord(name, startSector, typeId, length);
            }
        }
    }

    private static byte[] ReadFile(
        SectorImage image,
        DirectoryRecord record,
        ISet<int> occupiedSectors,
        out bool dataValid,
        out int occupiedSectorCount)
    {
        var content = new byte[record.Length];
        var written = 0;
        var sector = record.StartSector;
        var visited = new HashSet<int>();
        dataValid = true;
        while (written < content.Length)
        {
            if (sector <= 0 || sector > image.BlockCount || !visited.Add(sector) || !TrySector(image, sector, out var bytes))
            {
                dataValid = false;
                break;
            }

            occupiedSectors.Add(sector);
            var count = Math.Min(DataLengthPerSector, content.Length - written);
            bytes.AsSpan(0, count).CopyTo(content.AsSpan(written));
            written += count;
            var nextSector = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(DataLengthPerSector, 2));
            if (written < content.Length && nextSector == 0)
            {
                dataValid = false;
                break;
            }
            sector = nextSector;
        }
        occupiedSectorCount = visited.Count;
        if (written != content.Length) Array.Resize(ref content, written);
        return content;
    }

    private static bool TrySector(SectorImage image, int sectorNumber, out byte[] bytes)
    {
        bytes = [];
        if (sectorNumber <= 0 || !image.TryGetBlock(sectorNumber - 1, out var block) || block.Data.Count < 128) return false;
        bytes = block.Data.ToArray();
        return true;
    }

    private readonly record struct DirectoryRecord(string Name, int StartSector, byte TypeId, int Length)
    {
        public bool IsActive => TypeId is (byte)'X' or (byte)'x';
        public bool IsValid(int blockCount) =>
            !string.IsNullOrWhiteSpace(Name)
            && StartSector > 0 && StartSector <= blockCount
            && Length is > 0 and <= DefaultIconLength;
    }
}
