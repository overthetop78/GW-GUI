using System.Buffers.Binary;
using System.Collections.Frozen;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Blocks;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Vmdk;

/// <summary>Reads autonomous monolithicFlat and uncompressed monolithicSparse VMDK images.</summary>
public sealed class VmdkReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Vmdk }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.HardDisk }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures =
        [VmdkFormat.SparseSignature, VmdkFormat.DescriptorSignature];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => VmdkFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => VmdkFormat.AssociatedExtensions;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        try
        {
            await ReadDocumentAsync(context, false, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
        => ReadDocumentAsync(context, true, cancellationToken);

    private static async Task<MediaImageDocument> ReadDocumentAsync(
        MediaRecognitionContext context,
        bool includeRanges,
        CancellationToken cancellationToken)
    {
        if (context.Length < 4) throw new InvalidDataException("The file is too short to contain a VMDK image.");
        var signature = await context.ReadAsync(0, 4, cancellationToken).ConfigureAwait(false);
        return signature.Span.SequenceEqual(VmdkFormat.SparseSignature.Span)
            ? await ReadSparseAsync(context, includeRanges, cancellationToken).ConfigureAwait(false)
            : await ReadDescriptorAsync(context, includeRanges, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<MediaImageDocument> ReadDescriptorAsync(
        MediaRecognitionContext context,
        bool includeRanges,
        CancellationToken cancellationToken)
    {
        if (context.Length > 4 * 1_024 * 1_024)
            throw new InvalidDataException("A text VMDK descriptor is unexpectedly large.");
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var text = System.Text.Encoding.UTF8.GetString(bytes.Span);
        if (!text.TrimStart('\uFEFF', ' ', '\t', '\r', '\n').StartsWith("# Disk DescriptorFile", StringComparison.Ordinal))
            throw new InvalidDataException("The VMDK descriptor signature is missing.");
        if (ContainsAssignedValue(text, "parentFileNameHint"))
            throw new NotSupportedException("A VMDK parent chain requires a parent resolver that is not registered.");
        var createType = ReadAssignedValue(text, "createType");
        if (!createType.Equals("monolithicFlat", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"VMDK create type '{createType}' is not supported by the descriptor reader.");
        var extent = ParseSingleExtent(text);
        if (!extent.Access.Equals("RW", StringComparison.OrdinalIgnoreCase)
            || !extent.Type.Equals("FLAT", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("Only one writable FLAT extent is supported for monolithicFlat VMDK.");
        var descriptorDirectory = Path.GetDirectoryName(context.Source.PrimaryPath) ?? Directory.GetCurrentDirectory();
        var extentPath = Path.GetFullPath(Path.Combine(descriptorDirectory, extent.FileName));
        if (!File.Exists(extentPath)) throw new InvalidDataException($"The VMDK extent '{extent.FileName}' is missing.");
        var capacity = checked(extent.Sectors * HardDiskFormatConstants.LegacyLogicalSectorSize);
        var sourceOffset = checked(extent.OffsetSectors * HardDiskFormatConstants.LegacyLogicalSectorSize);
        var extentLength = new FileInfo(extentPath).Length;
        if (sourceOffset > extentLength || capacity > extentLength - sourceOffset)
            throw new InvalidDataException("The VMDK FLAT extent is shorter than its declared capacity.");
        var ranges = includeRanges
            ? new[] { new MediaDataRange(0, capacity, MediaDataRangeKind.Stored, new FileRandomAccessData(extentPath), sourceOffset) }
            : new[] { new MediaDataRange(0, capacity, MediaDataRangeKind.Unavailable) };
        var source = new MediaSourceDescriptor(
            context.Source.PrimaryPath,
            context.Source.AssociatedPaths.Concat([extentPath]).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            context.Source.KnownLength,
            context.Source.RequestedFormatId);
        return CreateDocument(source, capacity, ranges, HardDiskImageVariant.Fixed, text);
    }

    private static async Task<MediaImageDocument> ReadSparseAsync(
        MediaRecognitionContext context,
        bool includeRanges,
        CancellationToken cancellationToken)
    {
        if (context.Length < HardDiskFormatConstants.VmdkSparseHeaderSize)
            throw new InvalidDataException("The file is too short to contain a VMDK sparse header.");
        var memory = await context.ReadAsync(0, HardDiskFormatConstants.VmdkSparseHeaderSize, cancellationToken).ConfigureAwait(false);
        var header = memory.Span;
        var version = BinaryPrimitives.ReadUInt32LittleEndian(header[4..8]);
        if (version != HardDiskFormatConstants.VmdkSparseVersion)
            throw new NotSupportedException($"VMDK sparse version {version} is unsupported.");
        var flags = BinaryPrimitives.ReadUInt32LittleEndian(header[8..12]);
        if ((flags & (HardDiskFormatConstants.VmdkSparseCompressedGrainFlag | HardDiskFormatConstants.VmdkSparseMarkerFlag)) != 0)
            throw new NotSupportedException("streamOptimized or compressed VMDK sparse extents are not supported.");
        if (header[72] != 0) throw new InvalidDataException("The VMDK sparse extent is marked unclean.");
        var capacitySectors = BinaryPrimitives.ReadUInt64LittleEndian(header[12..20]);
        var grainSectors = BinaryPrimitives.ReadUInt64LittleEndian(header[20..28]);
        var descriptorOffset = BinaryPrimitives.ReadUInt64LittleEndian(header[28..36]);
        var descriptorSize = BinaryPrimitives.ReadUInt64LittleEndian(header[36..44]);
        var entriesPerTable = BinaryPrimitives.ReadUInt32LittleEndian(header[44..48]);
        var directoryOffset = BinaryPrimitives.ReadUInt64LittleEndian(header[56..64]);
        if (directoryOffset == HardDiskFormatConstants.VmdkStreamOptimizedDirectoryOffset)
            throw new NotSupportedException("streamOptimized VMDK extents are not supported.");
        if (capacitySectors == 0 || capacitySectors > long.MaxValue / HardDiskFormatConstants.LegacyLogicalSectorSize
            || grainSectors == 0 || grainSectors > int.MaxValue / HardDiskFormatConstants.LegacyLogicalSectorSize
            || entriesPerTable == 0 || directoryOffset == 0)
            throw new InvalidDataException("The VMDK sparse geometry is invalid.");
        if (descriptorOffset != 0 && descriptorSize != 0)
        {
            var descriptorLength = checked(descriptorSize * HardDiskFormatConstants.LegacyLogicalSectorSize);
            if (descriptorLength > 4 * 1_024 * 1_024 || descriptorLength > int.MaxValue)
                throw new InvalidDataException("The embedded VMDK descriptor is too large.");
            var descriptor = await context.ReadAsync(
                checked((long)descriptorOffset * HardDiskFormatConstants.LegacyLogicalSectorSize),
                (int)descriptorLength,
                cancellationToken).ConfigureAwait(false);
            var descriptorText = System.Text.Encoding.UTF8.GetString(descriptor.Span).TrimEnd('\0');
            if (ContainsAssignedValue(descriptorText, "parentFileNameHint"))
                throw new NotSupportedException("A VMDK parent chain requires a parent resolver that is not registered.");
            var embeddedType = ReadAssignedValue(descriptorText, "createType");
            if (!embeddedType.Equals("monolithicSparse", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"Embedded VMDK create type '{embeddedType}' is unsupported.");
        }

        var capacity = checked((long)capacitySectors * HardDiskFormatConstants.LegacyLogicalSectorSize);
        var grainSize = checked((long)grainSectors * HardDiskFormatConstants.LegacyLogicalSectorSize);
        var grainCount = checked((capacity + grainSize - 1) / grainSize);
        var directoryEntries = checked((grainCount + entriesPerTable - 1) / entriesPerTable);
        if (directoryEntries > int.MaxValue / sizeof(uint)) throw new InvalidDataException("The VMDK grain directory is too large.");
        var directoryByteOffset = checked((long)directoryOffset * HardDiskFormatConstants.LegacyLogicalSectorSize);
        var directory = await context.ReadAsync(directoryByteOffset, checked((int)directoryEntries * 4), cancellationToken).ConfigureAwait(false);
        var source = includeRanges ? new FileRandomAccessData(context.Source.PrimaryPath) : null;
        var ranges = new List<MediaDataRange>();
        long grainIndex = 0;
        for (var directoryIndex = 0; directoryIndex < directoryEntries; directoryIndex++)
        {
            var tableSector = BinaryPrimitives.ReadUInt32LittleEndian(directory.Span.Slice(directoryIndex * 4, 4));
            if (tableSector == 0) throw new InvalidDataException($"VMDK grain table {directoryIndex} is missing.");
            var table = await context.ReadAsync(
                checked((long)tableSector * HardDiskFormatConstants.LegacyLogicalSectorSize),
                checked((int)entriesPerTable * 4),
                cancellationToken).ConfigureAwait(false);
            for (var tableIndex = 0; tableIndex < entriesPerTable && grainIndex < grainCount; tableIndex++, grainIndex++)
            {
                var grainSector = BinaryPrimitives.ReadUInt32LittleEndian(table.Span.Slice(tableIndex * 4, 4));
                var address = checked(grainIndex * grainSize);
                var length = Math.Min(grainSize, capacity - address);
                if (grainSector == 0)
                {
                    AddRange(ranges, address, length, MediaDataRangeKind.Unallocated);
                    continue;
                }
                if (grainSector == 1 && (flags & HardDiskFormatConstants.VmdkSparseZeroedGrainFlag) != 0)
                {
                    AddRange(ranges, address, length, MediaDataRangeKind.Zero);
                    continue;
                }
                var sourceOffset = checked((long)grainSector * HardDiskFormatConstants.LegacyLogicalSectorSize);
                if (sourceOffset > context.Length - length)
                    throw new InvalidDataException($"VMDK grain {grainIndex} points outside the file.");
                AddRange(ranges, address, length,
                    includeRanges ? MediaDataRangeKind.Stored : MediaDataRangeKind.Unavailable,
                    source,
                    includeRanges ? sourceOffset : 0);
            }
        }
        return CreateDocument(context.Source, capacity, ranges, HardDiskImageVariant.Sparse, null);
    }

    private static MediaImageDocument CreateDocument(
        MediaSourceDescriptor source,
        long capacity,
        IReadOnlyList<MediaDataRange> ranges,
        HardDiskImageVariant variant,
        string? descriptor)
        => new(
            source,
            HardDiskImageFormatIds.Vmdk,
            MediaKind.HardDisk,
            new BlockMediaImageRepresentation(capacity, HardDiskFormatConstants.LegacyLogicalSectorSize, ranges),
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["variant"] = variant.ToString(),
                ["descriptor"] = descriptor ?? string.Empty
            });

    private static VmdkExtent ParseSingleExtent(string descriptor)
    {
        var extentLines = descriptor.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.StartsWith("RW ", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("RDONLY ", StringComparison.OrdinalIgnoreCase)
                || line.StartsWith("NOACCESS ", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (extentLines.Length != 1) throw new NotSupportedException("Only a single monolithic VMDK extent is supported.");
        var line = extentLines[0];
        var firstQuote = line.IndexOf('"');
        var secondQuote = firstQuote < 0 ? -1 : line.IndexOf('"', firstQuote + 1);
        if (firstQuote < 0 || secondQuote < 0) throw new InvalidDataException("The VMDK extent file name is invalid.");
        var prefix = line[..firstQuote].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (prefix.Length != 3 || !long.TryParse(prefix[1], out var sectors) || sectors <= 0)
            throw new InvalidDataException("The VMDK extent capacity is invalid.");
        var suffix = line[(secondQuote + 1)..].Trim();
        long offset = 0;
        if (suffix.Length != 0
            && !long.TryParse(suffix, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out offset))
            throw new InvalidDataException("The VMDK extent offset is invalid.");
        if (offset < 0) throw new InvalidDataException("The VMDK extent offset is invalid.");
        return new VmdkExtent(prefix[0], sectors, prefix[2], line[(firstQuote + 1)..secondQuote], offset);
    }

    private static bool ContainsAssignedValue(string descriptor, string name)
        => descriptor.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(line => line.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase));

    private static string ReadAssignedValue(string descriptor, string name)
    {
        var prefix = name + "=";
        var line = descriptor.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(candidate => candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        if (line is null) throw new InvalidDataException($"The VMDK descriptor has no {name} value.");
        return line[prefix.Length..].Trim().Trim('"');
    }

    private static void AddRange(
        List<MediaDataRange> ranges,
        long address,
        long length,
        MediaDataRangeKind kind,
        FileRandomAccessData? source = null,
        long sourceOffset = 0)
    {
        if (ranges.Count > 0)
        {
            var previous = ranges[^1];
            var sourceContinues = kind != MediaDataRangeKind.Stored
                || ReferenceEquals(previous.Source, source) && previous.SourceOffset + previous.Length == sourceOffset;
            if (previous.Kind == kind && previous.Address + previous.Length == address && sourceContinues)
            {
                ranges[^1] = new MediaDataRange(previous.Address, previous.Length + length, kind, previous.Source, previous.SourceOffset);
                return;
            }
        }
        ranges.Add(new MediaDataRange(address, length, kind, source, sourceOffset));
    }

    private sealed record VmdkExtent(string Access, long Sectors, string Type, string FileName, long OffsetSectors);
}
