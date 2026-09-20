using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Blocks;

namespace GWGUI.MediaEngine.Images.Formats.HardDisk.Raw;

/// <summary>Recognizes a headerless hard disk image only from explicit intent or coherent disk structures.</summary>
public sealed class RawHardDiskReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { HardDiskImageFormatIds.Raw }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.HardDisk }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;

    IReadOnlySet<string> IMediaImageReader.Extensions => RawHardDiskFormat.Extensions;

    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];

    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;

    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;

    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null)
        {
            if (!SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
            var requestedBlockSize = await ResolveLogicalBlockSize(context, cancellationToken).ConfigureAwait(false);
            return RawHardDiskFormat.IsLengthCompatible(context.Length, requestedBlockSize);
        }

        return await TryResolveStructuredBlockSizeAsync(context, cancellationToken).ConfigureAwait(false) is not null;
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var logicalBlockSize = await ResolveLogicalBlockSize(context, cancellationToken).ConfigureAwait(false);
        if (!RawHardDiskFormat.IsLengthCompatible(context.Length, logicalBlockSize))
            throw new InvalidDataException($"The raw image length is not aligned to {logicalBlockSize} byte logical blocks.");

        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        if (source.Length != context.Length)
            throw new InvalidDataException("The raw image length changed after recognition.");

        var representation = new BlockMediaImageRepresentation(
            context.Length,
            logicalBlockSize,
            [new MediaDataRange(0, context.Length, MediaDataRangeKind.Stored, source)]);
        return new MediaImageDocument(
            context.Source,
            HardDiskImageFormatIds.Raw,
            MediaKind.HardDisk,
            representation,
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["logicalBlockSize"] = logicalBlockSize.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static async ValueTask<int> ResolveLogicalBlockSize(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var structuredSize = await TryResolveStructuredBlockSizeAsync(context, cancellationToken).ConfigureAwait(false);
        if (structuredSize is { } resolved) return resolved;
        if (context.RequestedFormatId is not null && SupportedFormatIds.Contains(context.RequestedFormatId))
            return HardDiskFormatConstants.LegacyLogicalSectorSize;
        throw new InvalidDataException("A headerless hard disk image requires an explicit format choice or a coherent MBR/GPT structure.");
    }

    private static async ValueTask<int?> TryResolveStructuredBlockSizeAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (await HasGptHeaderAsync(context, HardDiskFormatConstants.LargeLogicalSectorSize, cancellationToken).ConfigureAwait(false))
            return HardDiskFormatConstants.LargeLogicalSectorSize;
        if (await HasGptHeaderAsync(context, HardDiskFormatConstants.LegacyLogicalSectorSize, cancellationToken).ConfigureAwait(false))
            return HardDiskFormatConstants.LegacyLogicalSectorSize;
        if (context.Length >= HardDiskFormatConstants.LegacyLogicalSectorSize
            && RawHardDiskFormat.IsLengthCompatible(context.Length, HardDiskFormatConstants.LegacyLogicalSectorSize))
        {
            var signature = await context.ReadAsync(510, 2, cancellationToken).ConfigureAwait(false);
            if (signature.Span.SequenceEqual(new byte[] { 0x55, 0xAA }))
                return HardDiskFormatConstants.LegacyLogicalSectorSize;
        }

        return null;
    }

    private static async ValueTask<bool> HasGptHeaderAsync(
        MediaRecognitionContext context,
        int logicalBlockSize,
        CancellationToken cancellationToken)
    {
        if (!RawHardDiskFormat.IsLengthCompatible(context.Length, logicalBlockSize)
            || context.Length < logicalBlockSize + 8L)
            return false;
        var header = await context.ReadAsync(logicalBlockSize, 8, cancellationToken).ConfigureAwait(false);
        return header.Span.SequenceEqual(System.Text.Encoding.ASCII.GetBytes("EFI PART"));
    }
}
