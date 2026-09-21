using MediaVolumeOrigins = global::GWGUI.MediaFileSystems.Constants.MediaVolumeOrigins;
using PartitionTableIds = global::GWGUI.MediaFileSystems.Constants.PartitionTableIds;
using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using MediaVolumeDetectionResult = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDetectionResult;
using IMediaBlockRepresentation = global::GWGUI.MediaFileSystems.Interfaces.IMediaBlockRepresentation;
using IMediaImageDocument = global::GWGUI.MediaFileSystems.Interfaces.IMediaImageDocument;
using GWGUI.MediaFileSystems.Interfaces.Exploration;
using System.Buffers.Binary;
using System.Globalization;


using GWGUI.MediaFileSystems.Functions;

using System.IO;

namespace GWGUI.MediaFileSystems.Exploration.Partitioning;

/// <summary>Detects GPT partitions and validates the primary and secondary GPT structures.</summary>
public sealed class GptVolumeDetector : IMediaVolumeDetector
{
    private const int MinimumHeaderSize = 92;
    private const int MinimumEntrySize = 128;
    private const long MaximumEntryArraySize = 128L * 1_024 * 1_024;
    private static readonly byte[] Signature = "EFI PART"u8.ToArray();

    public async ValueTask<MediaVolumeDetectionResult?> DetectAsync(
        IMediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!document.IsHardDisk
            || document.Representation is not IMediaBlockRepresentation blocks
            || blocks.LogicalBlockCount < 2)
            return null;

        var primaryBlock = await ReadBlockAsync(blocks, 1, cancellationToken).ConfigureAwait(false);
        if (!primaryBlock.AsSpan(0, Math.Min(Signature.Length, primaryBlock.Length)).SequenceEqual(Signature)) return null;

        var diagnostics = new List<string>();
        GptHeader primary;
        try
        {
            primary = ParseHeader(primaryBlock, blocks, 1);
        }
        catch (InvalidDataException exception)
        {
            return new MediaVolumeDetectionResult([], [exception.Message]);
        }

        var entryBytes = await ReadEntryArrayAsync(blocks, primary, cancellationToken).ConfigureAwait(false);
        if (Crc32Functions.Compute(entryBytes) != primary.EntryArrayCrc32)
            diagnostics.Add("The primary GPT partition entry array CRC is invalid.");

        await ValidateSecondaryHeaderAsync(blocks, primary, diagnostics, cancellationToken).ConfigureAwait(false);
        var partitions = ParsePartitions(blocks, primary, entryBytes, diagnostics);
        AddOverlapDiagnostics(partitions, diagnostics);
        var volumes = partitions
            .OrderBy(partition => partition.Start)
            .Select(partition => new MediaVolumeDescriptor(
                partition.Start,
                partition.Length,
                MediaVolumeOrigins.Partition,
                PartitionTableIds.Gpt,
                partition.Number,
                partitionType: partition.TypeId,
                partitionId: partition.PartitionId,
                name: partition.Name))
            .ToArray();
        return new MediaVolumeDetectionResult(volumes, diagnostics);
    }

    private static GptHeader ParseHeader(byte[] block, IMediaBlockRepresentation blocks, long expectedLba)
    {
        if (block.Length < MinimumHeaderSize || !block.AsSpan(0, Signature.Length).SequenceEqual(Signature))
            throw new InvalidDataException($"The GPT header at LBA {expectedLba} has no valid signature.");
        var revision = BinaryPrimitives.ReadUInt32LittleEndian(block.AsSpan(8, 4));
        var headerSize = BinaryPrimitives.ReadUInt32LittleEndian(block.AsSpan(12, 4));
        if (revision < 0x00010000 || headerSize < MinimumHeaderSize || headerSize > block.Length)
            throw new InvalidDataException($"The GPT header at LBA {expectedLba} has invalid dimensions.");
        var expectedCrc = BinaryPrimitives.ReadUInt32LittleEndian(block.AsSpan(16, 4));
        var headerBytes = block.AsSpan(0, checked((int)headerSize)).ToArray();
        headerBytes.AsSpan(16, 4).Clear();
        if (Crc32Functions.Compute(headerBytes) != expectedCrc)
            throw new InvalidDataException($"The GPT header CRC at LBA {expectedLba} is invalid.");

        var currentLba = BinaryPrimitives.ReadUInt64LittleEndian(block.AsSpan(24, 8));
        var backupLba = BinaryPrimitives.ReadUInt64LittleEndian(block.AsSpan(32, 8));
        var firstUsableLba = BinaryPrimitives.ReadUInt64LittleEndian(block.AsSpan(40, 8));
        var lastUsableLba = BinaryPrimitives.ReadUInt64LittleEndian(block.AsSpan(48, 8));
        var entryArrayLba = BinaryPrimitives.ReadUInt64LittleEndian(block.AsSpan(72, 8));
        var entryCount = BinaryPrimitives.ReadUInt32LittleEndian(block.AsSpan(80, 4));
        var entrySize = BinaryPrimitives.ReadUInt32LittleEndian(block.AsSpan(84, 4));
        var entryArrayCrc = BinaryPrimitives.ReadUInt32LittleEndian(block.AsSpan(88, 4));
        if (currentLba != (ulong)expectedLba || backupLba >= (ulong)blocks.LogicalBlockCount
            || firstUsableLba > lastUsableLba || lastUsableLba >= (ulong)blocks.LogicalBlockCount
            || entryArrayLba >= (ulong)blocks.LogicalBlockCount || entryCount == 0
            || entrySize < MinimumEntrySize || entrySize % 8 != 0)
            throw new InvalidDataException($"The GPT header at LBA {expectedLba} contains invalid bounds.");
        var entryArrayLength = checked((long)entryCount * entrySize);
        if (entryArrayLength > MaximumEntryArraySize
            || entryArrayLength > int.MaxValue
            || entryArrayLba > (ulong)((blocks.Capacity - entryArrayLength) / blocks.LogicalBlockSize))
            throw new InvalidDataException($"The GPT partition entry array at LBA {entryArrayLba} exceeds the media.");

        return new GptHeader(
            revision,
            currentLba,
            backupLba,
            firstUsableLba,
            lastUsableLba,
            new Guid(block.AsSpan(56, 16)),
            entryArrayLba,
            entryCount,
            entrySize,
            entryArrayCrc);
    }

    private static async Task ValidateSecondaryHeaderAsync(
        IMediaBlockRepresentation blocks,
        GptHeader primary,
        List<string> diagnostics,
        CancellationToken cancellationToken)
    {
        try
        {
            var secondaryBlock = await ReadBlockAsync(blocks, checked((long)primary.BackupLba), cancellationToken).ConfigureAwait(false);
            var secondary = ParseHeader(secondaryBlock, blocks, checked((long)primary.BackupLba));
            if (secondary.BackupLba != primary.CurrentLba
                || secondary.FirstUsableLba != primary.FirstUsableLba
                || secondary.LastUsableLba != primary.LastUsableLba
                || secondary.DiskId != primary.DiskId
                || secondary.EntryCount != primary.EntryCount
                || secondary.EntrySize != primary.EntrySize
                || secondary.EntryArrayCrc32 != primary.EntryArrayCrc32)
                diagnostics.Add("The secondary GPT header does not match the primary header.");
            var secondaryEntries = await ReadEntryArrayAsync(blocks, secondary, cancellationToken).ConfigureAwait(false);
            if (Crc32Functions.Compute(secondaryEntries) != secondary.EntryArrayCrc32)
                diagnostics.Add("The secondary GPT partition entry array CRC is invalid.");
        }
        catch (InvalidDataException exception)
        {
            diagnostics.Add(exception.Message);
        }
    }

    private static List<GptPartition> ParsePartitions(
        IMediaBlockRepresentation blocks,
        GptHeader header,
        byte[] entries,
        List<string> diagnostics)
    {
        var partitions = new List<GptPartition>();
        for (var index = 0; index < header.EntryCount; index++)
        {
            var entryOffset = checked((int)((long)index * header.EntrySize));
            var entry = entries.AsSpan(entryOffset, checked((int)header.EntrySize));
            var typeId = new Guid(entry[..16]);
            if (typeId == Guid.Empty) continue;
            var partitionId = new Guid(entry[16..32]);
            var firstLba = BinaryPrimitives.ReadUInt64LittleEndian(entry[32..40]);
            var lastLba = BinaryPrimitives.ReadUInt64LittleEndian(entry[40..48]);
            var number = checked((int)index + 1);
            if (partitionId == Guid.Empty || firstLba > lastLba
                || firstLba < header.FirstUsableLba || lastLba > header.LastUsableLba
                || lastLba >= (ulong)blocks.LogicalBlockCount)
            {
                diagnostics.Add($"GPT partition {number} has invalid identifiers or LBA bounds.");
                continue;
            }
            var start = checked((long)firstLba * blocks.LogicalBlockSize);
            var length = checked(((long)(lastLba - firstLba) + 1) * blocks.LogicalBlockSize);
            var nameLength = Math.Min(72, entry.Length - 56);
            var name = nameLength <= 0
                ? null
                : System.Text.Encoding.Unicode.GetString(entry.Slice(56, nameLength)).TrimEnd('\0');
            partitions.Add(new GptPartition(
                start,
                length,
                number,
                typeId.ToString("D"),
                partitionId.ToString("D"),
                string.IsNullOrWhiteSpace(name) ? null : name));
        }
        return partitions;
    }

    private static void AddOverlapDiagnostics(List<GptPartition> partitions, List<string> diagnostics)
    {
        var ordered = partitions.OrderBy(partition => partition.Start).ToArray();
        for (var index = 1; index < ordered.Length; index++)
        {
            if (ordered[index].Start < ordered[index - 1].Start + ordered[index - 1].Length)
                diagnostics.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "GPT partitions {0} and {1} overlap.",
                    ordered[index - 1].Number,
                    ordered[index].Number));
        }
    }

    private static async Task<byte[]> ReadEntryArrayAsync(
        IMediaBlockRepresentation blocks,
        GptHeader header,
        CancellationToken cancellationToken)
    {
        var length = checked((int)((long)header.EntryCount * header.EntrySize));
        var data = new byte[length];
        await blocks.ReadExactlyAsync(
            checked((long)header.EntryArrayLba * blocks.LogicalBlockSize),
            data,
            cancellationToken).ConfigureAwait(false);
        return data;
    }

    private static async Task<byte[]> ReadBlockAsync(
        IMediaBlockRepresentation blocks,
        long lba,
        CancellationToken cancellationToken)
    {
        if (lba < 0 || lba >= blocks.LogicalBlockCount)
            throw new InvalidDataException("A GPT structure lies outside the media.");
        var data = new byte[blocks.LogicalBlockSize];
        await blocks.ReadExactlyAsync(
            checked(lba * blocks.LogicalBlockSize),
            data,
            cancellationToken).ConfigureAwait(false);
        return data;
    }

    private sealed record GptHeader(
        uint Revision,
        ulong CurrentLba,
        ulong BackupLba,
        ulong FirstUsableLba,
        ulong LastUsableLba,
        Guid DiskId,
        ulong EntryArrayLba,
        uint EntryCount,
        uint EntrySize,
        uint EntryArrayCrc32);

    private sealed record GptPartition(
        long Start,
        long Length,
        int Number,
        string TypeId,
        string PartitionId,
        string? Name);
}
