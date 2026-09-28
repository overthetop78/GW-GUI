using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaFileSystems.Contracts;

namespace GWGUI.MediaEngine.Images.Formats.Optical.WiiU;

/// <summary>Lit les images de disque Nintendo Wii U WUD et WUX.</summary>
public sealed class WiiUReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.NintendoWiiU }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Optical }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => WiiUFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [WiiUFormat.WuxSignature];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!WiiUFormat.Extensions.Contains(context.Extension)
            || context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId))
            return false;

        if (context.Extension.Equals(DiskImageFileExtensions.Wud, StringComparison.OrdinalIgnoreCase))
        {
            if (!WiiUFormat.IsWudLengthCompatible(context.Length))
                return false;
            var header = await context.ReadHeaderAsync(WiiUFormat.WuxHeaderSize, cancellationToken)
                .ConfigureAwait(false);
            return !WiiUFormat.TryReadWuxHeader(header.Span, context.Length, out _);
        }

        if (!WiiUFormat.TryReadWuxHeader(
                (await context.ReadHeaderAsync(WiiUFormat.WuxHeaderSize, cancellationToken).ConfigureAwait(false)).Span,
                context.Length,
                out var wux))
            return false;
        return await TryReadIndexTableAsync(context, wux, cancellationToken).ConfigureAwait(false) is not null;
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!WiiUFormat.Extensions.Contains(context.Extension)
            || context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId))
            throw new InvalidDataException("The selected source is not a Nintendo Wii U image.");

        var isWux = context.Extension.Equals(DiskImageFileExtensions.Wux, StringComparison.OrdinalIgnoreCase);
        var headerBytes = await context.ReadHeaderAsync(WiiUFormat.WuxHeaderSize, cancellationToken)
            .ConfigureAwait(false);
        WiiUWuxHeader wux = default;
        uint[]? indexTable = null;
        if (isWux)
        {
            if (!WiiUFormat.TryReadWuxHeader(headerBytes.Span, context.Length, out wux))
                throw new InvalidDataException("The Nintendo Wii U WUX header is invalid.");
            indexTable = await TryReadIndexTableAsync(context, wux, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidDataException("The Nintendo Wii U WUX index table is invalid.");
        }
        else if (!WiiUFormat.IsWudLengthCompatible(context.Length)
            || WiiUFormat.TryReadWuxHeader(headerBytes.Span, context.Length, out _))
        {
            throw new InvalidDataException("The Nintendo Wii U WUD image is not aligned to optical sectors.");
        }

        var source = isWux
            ? new WiiUWuxRandomAccessData(context.Source.PrimaryPath, wux, indexTable!) as global::GWGUI.MediaFileSystems.Interfaces.IMediaRandomAccessData
            : new FileRandomAccessData(context.Source.PrimaryPath);
        var logicalLength = isWux ? wux.UncompressedSize : context.Length;
        var logicalBlockSize = WiiUFormat.OpticalSectorSize;
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [WiiUMetadataConstants.SystemId] = DiskImageFormatIds.NintendoWiiU,
            [WiiUMetadataConstants.MediaRole] = WiiUMetadataConstants.MediaRole,
            [WiiUMetadataConstants.Variant] = isWux ? DiskImageFileExtensions.Wux : DiskImageFileExtensions.Wud,
            [WiiUMetadataConstants.SectorSize] = logicalBlockSize.ToString(CultureInfo.InvariantCulture),
            [WiiUMetadataConstants.UncompressedSize] = logicalLength.ToString(CultureInfo.InvariantCulture)
        };
        if (isWux)
        {
            metadata[WiiUMetadataConstants.IndexEntryCount] = wux.EntryCount.ToString(CultureInfo.InvariantCulture);
            metadata[WiiUMetadataConstants.StoredSectorCount] = wux.StoredSectorCount.ToString(CultureInfo.InvariantCulture);
        }

        return new MediaImageDocument(
            context.Source,
            DiskImageFormatIds.NintendoWiiU,
            MediaKind.Optical,
            new BlockMediaImageRepresentation(
                logicalLength,
                logicalBlockSize,
                [new MediaDataRange(0, logicalLength, MediaDataRangeKind.Stored, source)]),
            [new MediaVolumeDescriptor(0, logicalLength, MediaVolumeOrigins.DirectVolume, name: WiiUMetadataConstants.VolumeName)],
            [],
            metadata);
    }

    private static async Task<uint[]?> TryReadIndexTableAsync(
        MediaRecognitionContext context,
        WiiUWuxHeader header,
        CancellationToken cancellationToken)
    {
        var bytes = await context.ReadAsync(
                header.IndexOffset,
                checked(header.EntryCount * sizeof(uint)),
                cancellationToken)
            .ConfigureAwait(false);
        var indexes = new uint[header.EntryCount];
        for (var index = 0; index < indexes.Length; index++)
        {
            var storedSector = BinaryPrimitives.ReadUInt32LittleEndian(bytes.Span.Slice(index * sizeof(uint), sizeof(uint)));
            if (storedSector >= header.StoredSectorCount)
                return null;
            indexes[index] = storedSector;
        }
        return indexes;
    }
}
