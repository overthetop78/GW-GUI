using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Blocks;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Vdi;

/// <summary>Reads autonomous fixed and dynamic VDI 1.1 images.</summary>
public sealed class VdiReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Vdi }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.HardDisk }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => VdiFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        try
        {
            await ReadHeaderAsync(context, cancellationToken).ConfigureAwait(false);
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

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var header = await ReadHeaderAsync(context, cancellationToken).ConfigureAwait(false);
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        var mapLength = checked((int)header.BlocksInImage * sizeof(uint));
        var map = await context.ReadAsync(header.BlockMapOffset, mapLength, cancellationToken).ConfigureAwait(false);
        var ranges = new List<MediaDataRange>();
        var seenAllocatedBlocks = new HashSet<uint>();
        for (var logicalBlock = 0; logicalBlock < header.BlocksInImage; logicalBlock++)
        {
            var entry = BinaryPrimitives.ReadUInt32LittleEndian(map.Span.Slice(logicalBlock * 4, 4));
            var address = checked((long)logicalBlock * header.BlockSize);
            var length = Math.Min((long)header.BlockSize, header.DiskSize - address);
            if (entry == HardDiskFormatConstants.VdiUnallocatedBlock)
            {
                AddRange(ranges, address, length, MediaDataRangeKind.Unallocated);
                continue;
            }
            if (entry == HardDiskFormatConstants.VdiDiscardedBlock)
            {
                AddRange(ranges, address, length, MediaDataRangeKind.Zero);
                continue;
            }
            if (entry >= header.BlocksAllocated || !seenAllocatedBlocks.Add(entry))
                throw new InvalidDataException($"VDI block map entry {logicalBlock} is invalid or duplicated.");
            var sourceOffset = checked((long)header.DataOffset + (long)entry * (header.BlockSize + header.BlockExtra) + header.BlockExtra);
            if (sourceOffset > context.Length - length)
                throw new InvalidDataException($"VDI block map entry {logicalBlock} points outside the file.");
            AddRange(ranges, address, length, MediaDataRangeKind.Stored, source, sourceOffset);
        }

        HardDiskGeometry? geometry = null;
        if (header.Cylinders > 0 && header.Heads > 0 && header.Sectors > 0)
        {
            var candidate = new HardDiskGeometry(header.Cylinders, checked((int)header.Heads), checked((int)header.Sectors), header.SectorSize);
            if (candidate.Capacity <= header.DiskSize) geometry = candidate;
        }
        var representation = new BlockMediaImageRepresentation(header.DiskSize, header.SectorSize, ranges, geometry);
        var variant = header.ImageType == HardDiskFormatConstants.VdiFixedImageType
            ? HardDiskImageVariant.Fixed
            : HardDiskImageVariant.Dynamic;
        return new MediaImageDocument(
            context.Source,
            HardDiskImageFormatIds.Vdi,
            MediaKind.HardDisk,
            representation,
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["variant"] = variant.ToString(),
                ["imageUuid"] = header.ImageUuid.ToString("D")
            });
    }

    private static async ValueTask<VdiHeader> ReadHeaderAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.Length < HardDiskFormatConstants.VdiHeaderSize)
            throw new InvalidDataException("The file is too short to contain a VDI header.");
        var memory = await context.ReadAsync(0, HardDiskFormatConstants.VdiHeaderSize, cancellationToken).ConfigureAwait(false);
        var header = memory.Span;
        if (BinaryPrimitives.ReadUInt32LittleEndian(header[0x40..0x44]) != HardDiskFormatConstants.VdiSignature)
            throw new InvalidDataException("The VDI signature is missing.");
        var version = BinaryPrimitives.ReadUInt32LittleEndian(header[0x44..0x48]);
        if (!VdiFormat.Versions.Contains(version)) throw new NotSupportedException($"VDI version 0x{version:X8} is unsupported.");
        var headerSize = BinaryPrimitives.ReadUInt32LittleEndian(header[0x48..0x4C]);
        var imageType = BinaryPrimitives.ReadUInt32LittleEndian(header[0x4C..0x50]);
        if (imageType is not HardDiskFormatConstants.VdiDynamicImageType and not HardDiskFormatConstants.VdiFixedImageType)
            throw new NotSupportedException($"VDI image type {imageType} is unsupported.");
        var blockMapOffset = BinaryPrimitives.ReadUInt32LittleEndian(header[0x154..0x158]);
        var dataOffset = BinaryPrimitives.ReadUInt32LittleEndian(header[0x158..0x15C]);
        var sectorSize = BinaryPrimitives.ReadUInt32LittleEndian(header[0x168..0x16C]);
        var diskSizeUnsigned = BinaryPrimitives.ReadUInt64LittleEndian(header[0x170..0x178]);
        var blockSize = BinaryPrimitives.ReadUInt32LittleEndian(header[0x178..0x17C]);
        var blockExtra = BinaryPrimitives.ReadUInt32LittleEndian(header[0x17C..0x180]);
        var blocksInImage = BinaryPrimitives.ReadUInt32LittleEndian(header[0x180..0x184]);
        var blocksAllocated = BinaryPrimitives.ReadUInt32LittleEndian(header[0x184..0x188]);
        if (headerSize < 0x180 || blockMapOffset < HardDiskFormatConstants.VdiHeaderSize || dataOffset <= blockMapOffset)
            throw new InvalidDataException("The VDI header or data offsets are invalid.");
        if (sectorSize != HardDiskFormatConstants.LegacyLogicalSectorSize)
            throw new NotSupportedException($"VDI logical sector size {sectorSize} is unsupported.");
        if (diskSizeUnsigned == 0 || diskSizeUnsigned > long.MaxValue || diskSizeUnsigned % sectorSize != 0)
            throw new InvalidDataException("The VDI disk size is invalid.");
        if (blockSize == 0 || blockSize % sectorSize != 0 || blocksInImage == 0
            || blocksInImage != (diskSizeUnsigned + blockSize - 1) / blockSize
            || blocksAllocated > blocksInImage)
            throw new InvalidDataException("The VDI block geometry is invalid.");
        var mapLength = checked((long)blocksInImage * sizeof(uint));
        if (blockMapOffset > context.Length - mapLength || dataOffset < blockMapOffset + mapLength)
            throw new InvalidDataException("The VDI block map exceeds the file.");
        var parentUuid = new Guid(header[0x1B8..0x1C8]);
        if (parentUuid != Guid.Empty)
            throw new NotSupportedException("Differencing VDI images require a parent resolver that is not registered.");

        return new VdiHeader(
            imageType,
            blockMapOffset,
            dataOffset,
            BinaryPrimitives.ReadUInt32LittleEndian(header[0x15C..0x160]),
            BinaryPrimitives.ReadUInt32LittleEndian(header[0x160..0x164]),
            BinaryPrimitives.ReadUInt32LittleEndian(header[0x164..0x168]),
            checked((int)sectorSize),
            (long)diskSizeUnsigned,
            checked((int)blockSize),
            checked((int)blockExtra),
            checked((int)blocksInImage),
            blocksAllocated,
            new Guid(header[0x188..0x198]));
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

    private sealed record VdiHeader(
        uint ImageType,
        uint BlockMapOffset,
        uint DataOffset,
        uint Cylinders,
        uint Heads,
        uint Sectors,
        int SectorSize,
        long DiskSize,
        int BlockSize,
        int BlockExtra,
        int BlocksInImage,
        uint BlocksAllocated,
        Guid ImageUuid);
}
