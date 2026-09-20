using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.Security.Cryptography;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Reading.Blocks;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.MediaEngine.Formats.HardDisk.Chd;

/// <summary>Writes autonomous, uncompressed CHD V5 hard disk images with explicit GDDD geometry.</summary>
public sealed class ChdHardDiskWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Chd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public ChdHardDiskWriter(IAtomicImageFileWriter? files = null)
    {
        this.files = files ?? new AtomicImageFileWriter();
    }

    public string Id => MediaImageWriterIds.HardDiskChd;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => ChdHardDiskFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && ChdHardDiskFormat.Extensions.Contains(targetExtension)
            && document.MediaKind == MediaKind.HardDisk
            && document.Representation is BlockMediaImageRepresentation blocks
            && blocks.Geometry is not null
            && blocks.Geometry.Capacity == blocks.Capacity
            && blocks.Geometry.SectorSize == blocks.LogicalBlockSize
            && blocks.LogicalBlockSize > 0
            && ChdConstants.DefaultHunkSize % blocks.LogicalBlockSize == 0
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
            || document.Representation is not BlockMediaImageRepresentation blocks
            || blocks.Geometry is null)
            throw new InvalidDataException("The media document cannot be written as an autonomous uncompressed CHD hard disk image with explicit geometry.");

        await files.WriteAsync(
            outputPath,
            (output, token) => WriteContainerAsync(blocks, blocks.Geometry, output, token),
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static async Task WriteContainerAsync(
        BlockMediaImageRepresentation blocks,
        HardDiskGeometry geometry,
        Stream output,
        CancellationToken cancellationToken)
    {
        var hunkBytes = ChdConstants.DefaultHunkSize;
        var hunkCount = checked((blocks.Capacity + hunkBytes - 1) / hunkBytes);
        var mapLength = checked(hunkCount * ChdConstants.Version5MapEntrySize);
        if (mapLength > int.MaxValue) throw new InvalidDataException("The CHD hunk map is too large.");
        var mapOffset = ChdConstants.Version5HeaderSize;
        var dataOffset = Align(checked(mapOffset + mapLength), hunkBytes);
        var lastDataOffset = checked(dataOffset + checked((hunkCount - 1) * hunkBytes));
        if (lastDataOffset / hunkBytes > uint.MaxValue)
            throw new InvalidDataException("The CHD file offsets exceed the uncompressed V5 map capacity.");

        output.SetLength(dataOffset);
        var map = new byte[(int)mapLength];
        var hunk = new byte[hunkBytes];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        for (long hunkIndex = 0; hunkIndex < hunkCount; hunkIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            hunk.AsSpan().Clear();
            var logicalOffset = checked(hunkIndex * hunkBytes);
            var logicalLength = (int)Math.Min(hunkBytes, blocks.Capacity - logicalOffset);
            await BlockMediaDataReader.ReadExactlyAsync(
                blocks,
                logicalOffset,
                hunk.AsMemory(0, logicalLength),
                cancellationToken).ConfigureAwait(false);
            hash.AppendData(hunk, 0, logicalLength);
            var fileOffset = checked(dataOffset + hunkIndex * hunkBytes);
            BinaryPrimitives.WriteUInt32BigEndian(
                map.AsSpan(checked((int)hunkIndex * 4), 4),
                checked((uint)(fileOffset / hunkBytes)));
            output.Position = fileOffset;
            await output.WriteAsync(hunk, cancellationToken).ConfigureAwait(false);
        }

        var metadata = System.Text.Encoding.ASCII.GetBytes(string.Format(
            CultureInfo.InvariantCulture,
            ChdConstants.HardDiskMetadataFormat,
            geometry.Cylinders,
            geometry.Heads,
            geometry.SectorsPerTrack,
            geometry.SectorSize));
        if (metadata.Length > ChdConstants.MetadataLengthMask)
            throw new InvalidDataException("The CHD hard disk metadata is too large.");
        var metadataOffset = checked(dataOffset + hunkCount * hunkBytes);
        var metadataHeader = new byte[ChdConstants.MetadataHeaderSize];
        BinaryPrimitives.WriteUInt32BigEndian(metadataHeader.AsSpan(0, 4), ChdConstants.HardDiskMetadataTag);
        metadataHeader[4] = 0;
        metadataHeader[5] = checked((byte)(metadata.Length >> 16));
        metadataHeader[6] = checked((byte)(metadata.Length >> 8));
        metadataHeader[7] = checked((byte)metadata.Length);
        output.Position = metadataOffset;
        await output.WriteAsync(metadataHeader, cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(metadata, cancellationToken).ConfigureAwait(false);

        output.Position = mapOffset;
        await output.WriteAsync(map, cancellationToken).ConfigureAwait(false);
        var header = CreateHeader(
            blocks.Capacity,
            mapOffset,
            metadataOffset,
            hunkBytes,
            blocks.LogicalBlockSize,
            hash.GetHashAndReset());
        output.Position = 0;
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
    }

    private static byte[] CreateHeader(
        long logicalBytes,
        long mapOffset,
        long metadataOffset,
        int hunkBytes,
        int unitBytes,
        ReadOnlySpan<byte> rawSha1)
    {
        var header = new byte[ChdConstants.Version5HeaderSize];
        ChdHardDiskFormat.Signature.Span.CopyTo(header);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(8, 4), ChdConstants.Version5HeaderSize);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(12, 4), ChdConstants.Version5);
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(32, 8), checked((ulong)logicalBytes));
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(40, 8), checked((ulong)mapOffset));
        BinaryPrimitives.WriteUInt64BigEndian(header.AsSpan(48, 8), checked((ulong)metadataOffset));
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(56, 4), checked((uint)hunkBytes));
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(60, 4), checked((uint)unitBytes));
        rawSha1.CopyTo(header.AsSpan(64, ChdConstants.Sha1Length));
        return header;
    }

    private static long Align(long value, int alignment)
        => checked((value + alignment - 1) / alignment * alignment);

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
}
