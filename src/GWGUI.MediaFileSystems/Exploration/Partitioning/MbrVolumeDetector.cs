using MediaVolumeOrigins = global::GWGUI.MediaFileSystems.Constants.MediaVolumeOrigins;
using PartitionTableIds = global::GWGUI.MediaFileSystems.Constants.PartitionTableIds;
using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using MediaVolumeDetectionResult = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDetectionResult;
using IMediaBlockRepresentation = global::GWGUI.MediaFileSystems.Interfaces.IMediaBlockRepresentation;
using IMediaImageDocument = global::GWGUI.MediaFileSystems.Interfaces.IMediaImageDocument;
using GWGUI.MediaFileSystems.Interfaces.Exploration;
using System.Buffers.Binary;
using System.Globalization;



using System.IO;

namespace GWGUI.MediaFileSystems.Exploration.Partitioning;

/// <summary>Detects primary MBR partitions and logical partitions linked through EBR records.</summary>
public sealed class MbrVolumeDetector : IMediaVolumeDetector
{
    private const int PartitionTableOffset = 446;
    private const int PartitionEntrySize = 16;
    private const int PartitionEntryCount = 4;
    private const int BootRecordSize = 512;
    private const byte ProtectiveGptType = 0xEE;
    private const int MaximumEbrCount = 4_096;

    public async ValueTask<MediaVolumeDetectionResult?> DetectAsync(
        IMediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!document.IsHardDisk
            || document.Representation is not IMediaBlockRepresentation blocks
            || blocks.LogicalBlockSize < BootRecordSize
            || blocks.Capacity < blocks.LogicalBlockSize)
            return null;

        var bootRecord = await ReadBootRecordAsync(blocks, 0, cancellationToken).ConfigureAwait(false);
        if (!HasSignature(bootRecord)) return null;
        var entries = ReadEntries(bootRecord);
        if (entries.Any(entry => entry.Type == ProtectiveGptType)) return null;
        if (entries.All(entry => entry.Type == 0 && entry.SectorCount == 0)) return null;

        var diagnostics = new List<string>();
        var partitions = new List<PartitionRange>();
        var partitionNumber = 0;
        foreach (var entry in entries)
        {
            if (entry.Type == 0 && entry.SectorCount == 0) continue;
            if (entry.Type == 0 || entry.SectorCount == 0)
            {
                diagnostics.Add("An MBR partition entry has an empty type or length.");
                continue;
            }
            if (IsExtended(entry.Type))
            {
                await ReadExtendedChainAsync(
                    blocks,
                    entry,
                    partitions,
                    diagnostics,
                    () => ++partitionNumber,
                    cancellationToken).ConfigureAwait(false);
                continue;
            }
            partitionNumber++;
            TryAddPartition(blocks, entry.FirstLba, entry.SectorCount, entry.Type, partitionNumber, partitions, diagnostics);
        }

        AddOverlapDiagnostics(partitions, diagnostics);
        var volumes = partitions
            .OrderBy(partition => partition.Start)
            .Select(partition => new MediaVolumeDescriptor(
                partition.Start,
                partition.Length,
                MediaVolumeOrigins.Partition,
                PartitionTableIds.Mbr,
                partition.Number,
                partitionType: $"0x{partition.Type:X2}"))
            .ToArray();
        return new MediaVolumeDetectionResult(volumes, diagnostics);
    }

    private static async Task ReadExtendedChainAsync(
        IMediaBlockRepresentation blocks,
        MbrEntry container,
        List<PartitionRange> partitions,
        List<string> diagnostics,
        Func<int> nextPartitionNumber,
        CancellationToken cancellationToken)
    {
        var extendedBase = (long)container.FirstLba;
        var containerEnd = checked(extendedBase + container.SectorCount);
        var currentEbr = extendedBase;
        var visited = new HashSet<long>();
        for (var index = 0; currentEbr != 0 && index < MaximumEbrCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!visited.Add(currentEbr))
            {
                diagnostics.Add("The EBR chain contains a loop.");
                return;
            }
            if (currentEbr < extendedBase || currentEbr >= containerEnd)
            {
                diagnostics.Add("An EBR record lies outside its extended partition.");
                return;
            }

            byte[] record;
            try
            {
                record = await ReadBootRecordAsync(blocks, currentEbr, cancellationToken).ConfigureAwait(false);
            }
            catch (InvalidDataException)
            {
                diagnostics.Add("An EBR record cannot be read from the image.");
                return;
            }
            if (!HasSignature(record))
            {
                diagnostics.Add("An EBR record has no valid boot signature.");
                return;
            }

            var entries = ReadEntries(record);
            var logical = entries[0];
            if (logical.Type != 0 && logical.SectorCount != 0)
            {
                var firstLba = checked(currentEbr + logical.FirstLba);
                var number = nextPartitionNumber();
                if (firstLba < extendedBase || firstLba + logical.SectorCount > containerEnd)
                    diagnostics.Add($"Logical MBR partition {number} lies outside its extended partition.");
                else
                    TryAddPartition(blocks, firstLba, logical.SectorCount, logical.Type, number, partitions, diagnostics);
            }
            else if (logical.Type != 0 || logical.SectorCount != 0)
            {
                diagnostics.Add("An EBR logical partition entry has an empty type or length.");
            }

            var link = entries[1];
            if (link.Type == 0 && link.SectorCount == 0) return;
            if (!IsExtended(link.Type) || link.SectorCount == 0)
            {
                diagnostics.Add("An EBR link entry is not a valid extended partition link.");
                return;
            }
            currentEbr = checked(extendedBase + link.FirstLba);
            if (entries.Skip(2).Any(entry => entry.Type != 0 || entry.SectorCount != 0))
                diagnostics.Add("An EBR record contains unsupported additional partition entries.");
        }
        if (currentEbr != 0) diagnostics.Add("The EBR chain exceeds the supported entry limit.");
    }

    private static bool TryAddPartition(
        IMediaBlockRepresentation blocks,
        long firstLba,
        long sectorCount,
        byte type,
        int number,
        List<PartitionRange> partitions,
        List<string> diagnostics)
    {
        if (firstLba <= 0 || sectorCount <= 0
            || firstLba > long.MaxValue / blocks.LogicalBlockSize
            || sectorCount > long.MaxValue / blocks.LogicalBlockSize)
        {
            diagnostics.Add($"MBR partition {number} has invalid LBA bounds.");
            return false;
        }
        var start = checked(firstLba * blocks.LogicalBlockSize);
        var length = checked(sectorCount * blocks.LogicalBlockSize);
        if (start > blocks.Capacity - length)
        {
            diagnostics.Add($"MBR partition {number} exceeds the media capacity.");
            return false;
        }
        partitions.Add(new PartitionRange(start, length, type, number));
        return true;
    }

    private static void AddOverlapDiagnostics(List<PartitionRange> partitions, List<string> diagnostics)
    {
        var ordered = partitions.OrderBy(partition => partition.Start).ToArray();
        for (var index = 1; index < ordered.Length; index++)
        {
            if (ordered[index].Start < ordered[index - 1].Start + ordered[index - 1].Length)
                diagnostics.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "MBR partitions {0} and {1} overlap.",
                    ordered[index - 1].Number,
                    ordered[index].Number));
        }
    }

    private static async Task<byte[]> ReadBootRecordAsync(
        IMediaBlockRepresentation blocks,
        long lba,
        CancellationToken cancellationToken)
    {
        if (lba < 0 || lba > blocks.LogicalBlockCount - 1)
            throw new InvalidDataException("The requested boot record is outside the media.");
        var buffer = new byte[BootRecordSize];
        await blocks.ReadExactlyAsync(
            checked(lba * blocks.LogicalBlockSize),
            buffer,
            cancellationToken).ConfigureAwait(false);
        return buffer;
    }

    private static bool HasSignature(ReadOnlySpan<byte> record)
        => record[510] == 0x55 && record[511] == 0xAA;

    private static MbrEntry[] ReadEntries(ReadOnlySpan<byte> record)
    {
        var entries = new MbrEntry[PartitionEntryCount];
        for (var index = 0; index < entries.Length; index++)
        {
            var entry = record.Slice(PartitionTableOffset + index * PartitionEntrySize, PartitionEntrySize);
            entries[index] = new MbrEntry(
                entry[4],
                BinaryPrimitives.ReadUInt32LittleEndian(entry[8..12]),
                BinaryPrimitives.ReadUInt32LittleEndian(entry[12..16]));
        }
        return entries;
    }

    private static bool IsExtended(byte type) => type is 0x05 or 0x0F or 0x85;

    private readonly record struct MbrEntry(byte Type, uint FirstLba, uint SectorCount);

    private sealed record PartitionRange(long Start, long Length, byte Type, int Number);
}
