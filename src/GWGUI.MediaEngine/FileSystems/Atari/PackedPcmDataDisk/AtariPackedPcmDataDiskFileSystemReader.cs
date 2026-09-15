using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.PackedPcmDataDisk;

/// <summary>Reads Atari disks that store packed 4-bit POKEY samples after three ATASCII information sectors.</summary>
public sealed class AtariPackedPcmDataDiskFileSystemReader : IFileSystemReader
{
    private const int InformationSectorCount = 3;
    private const int InformationSectorSize = 128;
    private const int SampleSectorSize = 256;
    private const int FirstSampleSector = 4;
    private const int LastLoadedSampleSector = 719;
    private const int MinimumSampleSectorCount = 128;

    public string Id => FileSystemIds.AtariPackedPcmDataDisk;

    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        DiskImageFormatIds.Atari180,
        DiskImageFormatIds.AtariXfd180
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public bool CanRead(SectorImage image) => TryReadLayout(image, out _, out _, out _);

    public FileSystemVolume Read(SectorImage image)
    {
        if (!TryReadLayout(image, out var information, out var sample, out var usedSampleSectorCount))
            throw new InvalidDataException("The sector image is not an Atari packed PCM data disk.");

        var informationMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["container"] = FileSystemIds.AtariPackedPcmDataDisk,
            ["encoding"] = "ATASCII",
            ["firstSector"] = "1",
            ["sectorCount"] = InformationSectorCount.ToString(CultureInfo.InvariantCulture)
        };
        var sampleMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["container"] = FileSystemIds.AtariPackedPcmDataDisk,
            ["encoding"] = "packed unsigned 4-bit POKEY volume samples",
            ["nibbleOrder"] = "high-low",
            ["firstSector"] = FirstSampleSector.ToString(CultureInfo.InvariantCulture),
            ["loadedSectorCount"] = (LastLoadedSampleSector - FirstSampleSector + 1).ToString(CultureInfo.InvariantCulture),
            ["usedSectorCount"] = usedSampleSectorCount.ToString(CultureInfo.InvariantCulture),
            ["usedLength"] = ((long)usedSampleSectorCount * SampleSectorSize).ToString(CultureInfo.InvariantCulture)
        };

        var entries = new[]
        {
            new FileSystemEntry(
                "HEADER.ATA",
                FileSystemEntryKind.File,
                information.Length,
                null,
                "ATASCII information sectors",
                0,
                1,
                true,
                [],
                information,
                nativeTypeId: "atari-atascii-information",
                occupiedSize: information.Length,
                attributes: ["information"],
                dataValid: true,
                syntheticName: true,
                metadata: informationMetadata),
            new FileSystemEntry(
                "SAMPLE.SMP",
                FileSystemEntryKind.File,
                sample.Length,
                null,
                "Packed 4-bit POKEY volume samples",
                0,
                FirstSampleSector,
                true,
                [],
                sample,
                nativeTypeId: "atari-pokey-pcm4",
                occupiedSize: sample.Length,
                attributes: ["audio", "packed-4-bit"],
                dataValid: true,
                syntheticName: true,
                metadata: sampleMetadata)
        };

        return new FileSystemVolume(
            FileSystemDisplayNames.AtariPackedPcmDataDisk,
            FileSystemIds.AtariPackedPcmDataDisk,
            image.Capacity,
            0,
            null,
            null,
            entries,
            [],
            freeSpaceKnown: false,
            attributes: ["raw-data", "sampled-audio"],
            bootable: false);
    }

    private static bool TryReadLayout(
        SectorImage image,
        out byte[] information,
        out byte[] sample,
        out int usedSampleSectorCount)
    {
        information = [];
        sample = [];
        usedSampleSectorCount = 0;
        if (!CatalogMatches(image) || image.BlockCount != 720 || image.BlockSize != SampleSectorSize)
            return false;

        var informationBytes = new List<byte>(InformationSectorCount * InformationSectorSize);
        for (var logicalBlock = 0; logicalBlock < InformationSectorCount; logicalBlock++)
        {
            if (!image.TryGetBlock(logicalBlock, out var block) || block.Data.Count != InformationSectorSize)
                return false;
            informationBytes.AddRange(block.Data);
        }
        if (!IsAtasciiInformation(informationBytes)) return false;

        var sampleBytes = new List<byte>((LastLoadedSampleSector - FirstSampleSector + 1) * SampleSectorSize);
        var lastNonZeroSector = 0;
        for (var sector = FirstSampleSector; sector <= LastLoadedSampleSector; sector++)
        {
            if (!image.TryGetBlock(sector - 1, out var block) || block.Data.Count != SampleSectorSize)
                return false;
            sampleBytes.AddRange(block.Data);
            if (block.Data.Any(value => value != 0)) lastNonZeroSector = sector;
        }
        if (!image.TryGetBlock(719, out var finalBlock) || finalBlock.Data.Count != SampleSectorSize ||
            finalBlock.Data.Any(value => value != 0) ||
            lastNonZeroSector < FirstSampleSector + MinimumSampleSectorCount - 1 ||
            lastNonZeroSector >= LastLoadedSampleSector)
            return false;

        usedSampleSectorCount = lastNonZeroSector - FirstSampleSector + 1;
        var usedLength = checked(usedSampleSectorCount * SampleSectorSize);
        if (!HasBalancedPackedNibbles(sampleBytes, usedLength)) return false;

        information = informationBytes.ToArray();
        sample = sampleBytes.ToArray();
        return true;
    }

    private static bool CatalogMatches(SectorImage image) =>
        image.FormatId.Equals(DiskImageFormatIds.Atari180, StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.Equals(DiskImageFormatIds.AtariXfd180, StringComparison.OrdinalIgnoreCase);

    private static bool IsAtasciiInformation(IReadOnlyCollection<byte> bytes)
    {
        var printable = bytes.Count(value =>
        {
            var atascii = value & 0x7f;
            return atascii is >= 0x20 and <= 0x7e;
        });
        return printable >= bytes.Count * 3 / 4;
    }

    private static bool HasBalancedPackedNibbles(IReadOnlyList<byte> bytes, int usedLength)
    {
        var high = new long[16];
        var low = new long[16];
        for (var index = 0; index < usedLength; index++)
        {
            var value = bytes[index];
            high[value >> 4]++;
            low[value & 0x0f]++;
        }

        if (high.Any(count => count == 0) || low.Any(count => count == 0)) return false;
        var difference = high.Zip(low, (left, right) => Math.Abs(left - right)).Sum();
        return difference <= usedLength / 20;
    }
}
