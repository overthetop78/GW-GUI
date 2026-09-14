using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.WordMagic;

/// <summary>Recognizes the proprietary compressed main-dictionary disk used by Word Magic revision 5.</summary>
public sealed class AtariWordMagicDictionaryFileSystemReader : IFileSystemReader
{
    private const int SectorCount = 720;
    private const int SectorSize = 128;
    private static readonly byte[] FirstSectorSignature =
    [
        0x01, 0x00, 0x51, 0x35, 0x00, 0x47, 0x5f, 0x00,
        0x59, 0xab, 0x00, 0x07, 0xde, 0x00, 0x7d, 0x06
    ];

    public string Id => FileSystemIds.AtariWordMagicDictionary;

    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        DiskImageFormatIds.Atari90,
        DiskImageFormatIds.AtariXfd90
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public bool CanRead(SectorImage image)
    {
        if (!CatalogFormatIds.Contains(image.FormatId)
            || image.BlockCount != SectorCount
            || image.BlockSize != SectorSize
            || !image.TryGetBlock(0, out var first)
            || !image.TryGetBlock(SectorCount - 1, out var last)
            || first.Data.Count != SectorSize
            || last.Data.Count != SectorSize
            || !first.Data.Take(FirstSectorSignature.Length).SequenceEqual(FirstSectorSignature))
            return false;

        return last.Data.Take(124).All(value => value == 0x1a)
            && last.Data.Skip(124).All(value => value == 0);
    }

    public FileSystemVolume Read(SectorImage image)
    {
        if (!CanRead(image))
            throw new InvalidDataException("The sector image is not a Word Magic main dictionary disk.");

        var content = new byte[SectorCount * SectorSize];
        for (var logicalBlock = 0; logicalBlock < SectorCount; logicalBlock++)
        {
            if (!image.TryGetBlock(logicalBlock, out var block) || block.Data.Count != SectorSize)
                throw new InvalidDataException($"Word Magic dictionary sector {logicalBlock + 1} is unavailable.");
            block.Data.ToArray().CopyTo(content, logicalBlock * SectorSize);
        }

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["container"] = FileSystemIds.AtariWordMagicDictionary,
            ["sectorCount"] = SectorCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["sectorSize"] = SectorSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["encoding"] = "Word Magic compressed dictionary"
        };
        var entry = new FileSystemEntry(
            "MAIN.DIC",
            FileSystemEntryKind.File,
            content.Length,
            null,
            string.Empty,
            0,
            1,
            true,
            [],
            content,
            nativeTypeId: "word-magic-main-dictionary",
            occupiedSize: content.Length,
            attributes: ["compressed", "dictionary"],
            dataValid: true,
            syntheticName: true,
            metadata: metadata);
        return new FileSystemVolume(
            FileSystemDisplayNames.AtariWordMagicDictionary,
            FileSystemIds.AtariWordMagicDictionary,
            image.Capacity,
            0,
            null,
            null,
            [entry],
            [],
            freeSpaceKnown: true,
            attributes: ["dictionary", "proprietary", "single-file"],
            bootable: false);
    }
}
