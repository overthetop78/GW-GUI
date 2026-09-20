using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Reading.Blocks;
using GWGUI.MediaEngine.Representations.Blocks;
using GWGUI.MediaEngine.Writing;

namespace GWGUI.MediaEngine.Formats.HardDisk.Vmdk;

/// <summary>Writes autonomous monolithicFlat and uncompressed monolithicSparse VMDK images.</summary>
public sealed class VmdkWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Vmdk }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public VmdkWriter(IAtomicImageFileWriter? files = null)
    {
        this.files = files ?? new AtomicImageFileWriter();
    }

    public string Id => MediaImageWriterIds.HardDiskVmdk;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => VmdkFormat.Extensions;
    public bool ProducesMultipleFiles => true;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && VmdkFormat.Extensions.Contains(targetExtension)
            && document.MediaKind == MediaKind.HardDisk
            && document.Representation is BlockMediaImageRepresentation blocks
            && blocks.Capacity > 0
            && blocks.LogicalBlockSize == HardDiskFormatConstants.LegacyLogicalSectorSize
            && HasCompleteReadableCoverage(blocks);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension)
            || document.Representation is not BlockMediaImageRepresentation blocks)
            throw new InvalidDataException("The media document cannot be written as a supported VMDK image.");

        if (SelectVariant(document, blocks) == HardDiskImageVariant.Sparse)
        {
            await files.WriteAsync(
                outputPath,
                (output, token) => WriteSparseAsync(blocks, Path.GetFileName(outputPath), output, token),
                cancellationToken).ConfigureAwait(false);
            return [outputPath];
        }

        var flatPath = Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? Directory.GetCurrentDirectory(),
            Path.GetFileNameWithoutExtension(outputPath) + "-flat" + DiskImageFileExtensions.Vmdk);
        var descriptor = CreateDescriptor(blocks, "monolithicFlat", "FLAT", Path.GetFileName(flatPath), " 0");
        await AtomicMediaFileSetWriter.WriteAsync(
            new Dictionary<string, Func<Stream, CancellationToken, Task>>(StringComparer.OrdinalIgnoreCase)
            {
                [flatPath] = (output, token) => WriteLogicalDataAsync(blocks, output, token),
                [outputPath] = (output, token) => output.WriteAsync(descriptor, token).AsTask()
            },
            cancellationToken).ConfigureAwait(false);
        return [outputPath, flatPath];
    }

    private static async Task WriteSparseAsync(
        BlockMediaImageRepresentation blocks,
        string fileName,
        Stream output,
        CancellationToken cancellationToken)
    {
        var capacitySectors = blocks.Capacity / HardDiskFormatConstants.LegacyLogicalSectorSize;
        var grainSectors = HardDiskFormatConstants.VmdkDefaultGrainSectors;
        var grainSize = grainSectors * HardDiskFormatConstants.LegacyLogicalSectorSize;
        var grainCount = checked((blocks.Capacity + grainSize - 1) / grainSize);
        var entriesPerTable = HardDiskFormatConstants.VmdkDefaultEntriesPerGrainTable;
        var tableCount = checked((grainCount + entriesPerTable - 1) / entriesPerTable);
        if (tableCount > int.MaxValue / sizeof(uint)) throw new InvalidDataException("The VMDK grain directory is too large.");
        const int descriptorSectors = 20;
        const long descriptorOffset = 1;
        var directoryOffset = descriptorOffset + descriptorSectors;
        var directorySectors = Align(checked((int)tableCount * sizeof(uint)), HardDiskFormatConstants.LegacyLogicalSectorSize)
            / HardDiskFormatConstants.LegacyLogicalSectorSize;
        var tableSectors = Align(entriesPerTable * sizeof(uint), HardDiskFormatConstants.LegacyLogicalSectorSize)
            / HardDiskFormatConstants.LegacyLogicalSectorSize;
        var tablesOffset = directoryOffset + directorySectors;
        var overheadSectors = tablesOffset + tableCount * tableSectors;

        var header = new byte[HardDiskFormatConstants.VmdkSparseHeaderSize];
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0, 4), HardDiskFormatConstants.VmdkSparseMagic);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4, 4), HardDiskFormatConstants.VmdkSparseVersion);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(8, 4),
            (uint)(HardDiskFormatConstants.VmdkSparseValidNewLineFlag | HardDiskFormatConstants.VmdkSparseZeroedGrainFlag));
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(12, 8), (ulong)capacitySectors);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(20, 8), (ulong)grainSectors);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(28, 8), descriptorOffset);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(36, 8), descriptorSectors);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(44, 4), (uint)entriesPerTable);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(56, 8), (ulong)directoryOffset);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(64, 8), (ulong)overheadSectors);
        header[73] = (byte)'\n';
        header[74] = (byte)' ';
        header[75] = (byte)'\r';
        header[76] = (byte)'\n';
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);

        var descriptor = CreateDescriptor(blocks, "monolithicSparse", "SPARSE", fileName, string.Empty);
        var descriptorArea = new byte[descriptorSectors * HardDiskFormatConstants.LegacyLogicalSectorSize];
        if (descriptor.Length > descriptorArea.Length) throw new InvalidDataException("The generated VMDK descriptor is too large.");
        descriptor.CopyTo(descriptorArea, 0);
        await output.WriteAsync(descriptorArea, cancellationToken).ConfigureAwait(false);

        var directory = new byte[directorySectors * HardDiskFormatConstants.LegacyLogicalSectorSize];
        for (var index = 0; index < tableCount; index++)
        {
            var tableSector = checked(tablesOffset + index * tableSectors);
            if (tableSector > uint.MaxValue) throw new InvalidDataException("The VMDK grain table offset exceeds the format.");
            BinaryPrimitives.WriteUInt32LittleEndian(directory.AsSpan(index * 4, 4), (uint)tableSector);
        }
        await output.WriteAsync(directory, cancellationToken).ConfigureAwait(false);
        var tablesByteOffset = output.Position;
        var tables = new byte[checked((int)tableCount * tableSectors * HardDiskFormatConstants.LegacyLogicalSectorSize)];
        await output.WriteAsync(tables, cancellationToken).ConfigureAwait(false);

        var grain = new byte[grainSize];
        for (long grainIndex = 0; grainIndex < grainCount; grainIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            grain.AsSpan().Clear();
            var address = checked(grainIndex * grainSize);
            var logicalLength = (int)Math.Min(grainSize, blocks.Capacity - address);
            await BlockMediaDataReader.ReadExactlyAsync(
                blocks,
                address,
                grain.AsMemory(0, logicalLength),
                cancellationToken).ConfigureAwait(false);
            var tableIndex = grainIndex / entriesPerTable;
            var entryIndex = grainIndex % entriesPerTable;
            var entryOffset = checked((int)((tableIndex * tableSectors * HardDiskFormatConstants.LegacyLogicalSectorSize) + entryIndex * 4));
            if (grain.AsSpan(0, logicalLength).IndexOfAnyExcept((byte)0) < 0)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(tables.AsSpan(entryOffset, 4), 1);
                continue;
            }
            if (output.Position % HardDiskFormatConstants.LegacyLogicalSectorSize != 0)
                throw new InvalidDataException("The VMDK grain is not sector aligned.");
            var grainSector = output.Position / HardDiskFormatConstants.LegacyLogicalSectorSize;
            if (grainSector > uint.MaxValue) throw new InvalidDataException("The VMDK grain offset exceeds the format.");
            BinaryPrimitives.WriteUInt32LittleEndian(tables.AsSpan(entryOffset, 4), (uint)grainSector);
            await output.WriteAsync(grain, cancellationToken).ConfigureAwait(false);
        }

        var end = output.Position;
        output.Position = tablesByteOffset;
        await output.WriteAsync(tables, cancellationToken).ConfigureAwait(false);
        output.Position = end;
    }

    private static byte[] CreateDescriptor(
        BlockMediaImageRepresentation blocks,
        string createType,
        string extentType,
        string extentFileName,
        string extentSuffix)
    {
        var capacitySectors = blocks.Capacity / HardDiskFormatConstants.LegacyLogicalSectorSize;
        var contentId = (uint)Random.Shared.NextInt64(1, uint.MaxValue);
        var geometry = blocks.Geometry;
        var cylinders = geometry?.Cylinders ?? Math.Max(1, capacitySectors / (16 * 63));
        var heads = geometry?.Heads ?? 16;
        var sectors = geometry?.SectorsPerTrack ?? 63;
        var text = $"# Disk DescriptorFile\nversion=1\nencoding=\"UTF-8\"\nCID={contentId:x8}\nparentCID=ffffffff\ncreateType=\"{createType}\"\n\n# Extent description\nRW {capacitySectors.ToString(CultureInfo.InvariantCulture)} {extentType} \"{extentFileName}\"{extentSuffix}\n\n# The Disk Data Base\n#DDB\nddb.virtualHWVersion = \"4\"\nddb.geometry.cylinders = \"{cylinders.ToString(CultureInfo.InvariantCulture)}\"\nddb.geometry.heads = \"{heads.ToString(CultureInfo.InvariantCulture)}\"\nddb.geometry.sectors = \"{sectors.ToString(CultureInfo.InvariantCulture)}\"\nddb.adapterType = \"lsilogic\"\n";
        return System.Text.Encoding.UTF8.GetBytes(text);
    }

    private static async Task WriteLogicalDataAsync(
        BlockMediaImageRepresentation blocks,
        Stream output,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * DataSizeConstants.BytesPerKibibyte];
        long address = 0;
        while (address < blocks.Capacity)
        {
            var count = (int)Math.Min(buffer.Length, blocks.Capacity - address);
            await BlockMediaDataReader.ReadExactlyAsync(blocks, address, buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            address += count;
        }
    }

    private static HardDiskImageVariant SelectVariant(MediaImageDocument document, BlockMediaImageRepresentation blocks)
    {
        if (document.FormatId.Equals(HardDiskImageFormatIds.Vmdk, StringComparison.OrdinalIgnoreCase)
            && document.Metadata.TryGetValue("variant", out var value)
            && Enum.TryParse<HardDiskImageVariant>(value, true, out var variant)
            && variant is HardDiskImageVariant.Fixed or HardDiskImageVariant.Sparse)
            return variant;
        return blocks.Ranges.Any(range => range.Kind is MediaDataRangeKind.Zero or MediaDataRangeKind.Unallocated)
            ? HardDiskImageVariant.Sparse
            : HardDiskImageVariant.Fixed;
    }

    private static bool HasCompleteReadableCoverage(BlockMediaImageRepresentation blocks)
    {
        long cursor = 0;
        foreach (var range in blocks.Ranges)
        {
            if (range.Address != cursor || range.Kind == MediaDataRangeKind.Unavailable) return false;
            cursor = checked(cursor + range.Length);
        }
        return cursor == blocks.Capacity;
    }

    private static int Align(int value, int alignment) => checked((value + alignment - 1) / alignment * alignment);
}
