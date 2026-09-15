using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.FileSystems.Atari.FixedRecordAnimation;

/// <summary>Reads Atari graphical-animation data disks made of consecutive fixed-size frame records.</summary>
public sealed class AtariFixedRecordAnimationFileSystemReader : IFileSystemReader
{
    private const int SectorSize = 128;
    private const int SectorCount = 1040;
    private const int RecordLength = 1888;
    private const int ShortVolumeRecordCount = 53;
    private const int LongVolumeRecordCount = 54;
    private const int MaximumTrailingDataBytes = 32;

    public string Id => FileSystemIds.AtariFixedRecordAnimation;

    public IReadOnlySet<string> CatalogFormatIds { get; } = new[]
    {
        DiskImageFormatIds.Atari130,
        DiskImageFormatIds.AtariXfd130
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public bool CanRead(SectorImage image) => TryReadLayout(image, out _, out _);

    public FileSystemVolume Read(SectorImage image)
    {
        if (!TryReadLayout(image, out var records, out var volumeRole))
            throw new InvalidDataException("The sector image is not an Atari fixed-record animation data disk.");

        var entries = new List<FileSystemEntry>(records.Count);
        for (var index = 0; index < records.Count; index++)
        {
            var content = records[index];
            var firstByte = index * RecordLength;
            entries.Add(new FileSystemEntry(
                $"FRAME-{index + 1:D4}.BIN",
                FileSystemEntryKind.File,
                content.Length,
                null,
                "Atari fixed-size graphical animation frame",
                0,
                firstByte / SectorSize + 1,
                true,
                [],
                content,
                nativeTypeId: "atari-fixed-record-animation-frame",
                occupiedSize: content.Length,
                attributes: ["image", "animation-frame", "fixed-record"],
                dataValid: true,
                syntheticName: true,
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["container"] = FileSystemIds.AtariFixedRecordAnimation,
                    ["volumeRole"] = volumeRole,
                    ["recordIndex"] = index.ToString(CultureInfo.InvariantCulture),
                    ["recordLength"] = RecordLength.ToString(CultureInfo.InvariantCulture),
                    ["sourceByteOffset"] = firstByte.ToString(CultureInfo.InvariantCulture)
                }));
        }

        return new FileSystemVolume(
            FileSystemDisplayNames.AtariFixedRecordAnimation,
            FileSystemIds.AtariFixedRecordAnimation,
            image.Capacity,
            0,
            null,
            null,
            entries,
            [],
            freeSpaceKnown: false,
            attributes: ["raw-data", "animation", "fixed-records"],
            bootable: false);
    }

    private static bool TryReadLayout(
        SectorImage image,
        out IReadOnlyList<byte[]> records,
        out string volumeRole)
    {
        records = [];
        volumeRole = string.Empty;
        if (!CatalogMatches(image) || image.BlockCount != SectorCount || image.BlockSize != SectorSize)
            return false;

        var data = new byte[SectorCount * SectorSize];
        for (var index = 0; index < SectorCount; index++)
        {
            if (!image.TryGetBlock(index, out var block) || block.Data.Count != SectorSize)
                return false;
            block.Data.ToArray().CopyTo(data, index * SectorSize);
        }

        var zeroCount = data.Count(value => value == 0);
        var packedNibbleCount = data.Count(value => (value & 0x88) == 0);
        if (zeroCount < data.Length / 2 || zeroCount > data.Length * 3 / 4 ||
            packedNibbleCount < data.Length * 19 / 20)
            return false;

        var firstNonZero = Array.FindIndex(data, value => value != 0);
        var recordCount = zeroCount >= data.Length * 13 / 20
            ? LongVolumeRecordCount
            : ShortVolumeRecordCount;
        volumeRole = recordCount == LongVolumeRecordCount
            ? "final-record-volume"
            : firstNonZero >= RecordLength
                ? "leading-blank-record-volume"
                : "continuous-record-volume";

        var loadedLength = recordCount * RecordLength;
        var trailingNonZero = 0;
        var lastTrailingNonZero = -1;
        for (var index = loadedLength; index < data.Length; index++)
        {
            if (data[index] == 0) continue;
            trailingNonZero++;
            lastTrailingNonZero = index - loadedLength;
        }
        if (trailingNonZero > MaximumTrailingDataBytes || lastTrailingNonZero >= MaximumTrailingDataBytes)
            return false;

        var result = new List<byte[]>(recordCount);
        var nonEmptyRecords = 0;
        for (var index = 0; index < recordCount; index++)
        {
            var record = data.AsSpan(index * RecordLength, RecordLength).ToArray();
            if (record.AsSpan().ContainsAnyExcept((byte)0)) nonEmptyRecords++;
            result.Add(record);
        }
        if (nonEmptyRecords < 40) return false;

        records = result;
        return true;
    }

    private static bool CatalogMatches(SectorImage image) =>
        image.FormatId.Equals(DiskImageFormatIds.Atari130, StringComparison.OrdinalIgnoreCase) ||
        image.FormatId.Equals(DiskImageFormatIds.AtariXfd130, StringComparison.OrdinalIgnoreCase);
}
