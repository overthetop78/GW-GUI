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

namespace GWGUI.MediaEngine.Images.Formats.Floppy.FamicomFds;

/// <summary>Lit les faces de disquette Famicom Disk System avec ou sans en-tête FDS.</summary>
public sealed class FamicomFdsReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.NintendoFamicomDisk }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Fds }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [FamicomFdsConstants.Signature];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!SupportedExtensions.Contains(context.Extension)
            || context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId))
            return false;

        if (context.Length >= FamicomFdsConstants.HeaderLength)
        {
            var header = await context.ReadHeaderAsync(FamicomFdsConstants.HeaderLength, cancellationToken)
                .ConfigureAwait(false);
            if (header.Span[..FamicomFdsConstants.Signature.Length].SequenceEqual(FamicomFdsConstants.Signature))
                return IsHeaderedLength(context.Length, header.Span);
        }

        return IsHeaderlessLength(context.Length);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!SupportedExtensions.Contains(context.Extension)
            || context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId))
            throw new InvalidDataException("The selected source is not a Famicom Disk System image.");

        var header = context.Length >= FamicomFdsConstants.HeaderLength
            ? (await context.ReadHeaderAsync(FamicomFdsConstants.HeaderLength, cancellationToken).ConfigureAwait(false)).ToArray()
            : [];
        var headered = header.Length == FamicomFdsConstants.HeaderLength
            && header.AsSpan(0, FamicomFdsConstants.Signature.Length).SequenceEqual(FamicomFdsConstants.Signature);
        var headerLength = headered ? FamicomFdsConstants.HeaderLength : 0;
        var dataLength = checked(context.Length - headerLength);
        if (dataLength <= 0 || dataLength % FamicomFdsConstants.SideLength != 0)
            throw new InvalidDataException("The Famicom Disk System image does not contain complete sides.");

        var sideCount = checked((int)(dataLength / FamicomFdsConstants.SideLength));
        if (sideCount < FamicomFdsConstants.MinimumSideCount)
            throw new InvalidDataException("The Famicom Disk System image contains no side.");
        var declaredSideCount = headered ? header[FamicomFdsConstants.SideCountOffset] : 0;
        if (headered && declaredSideCount != 0 && declaredSideCount != sideCount)
            throw new InvalidDataException("The Famicom Disk System side count does not match its header.");

        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        var ranges = new List<MediaDataRange>(sideCount);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [FamicomFdsConstants.SystemIdMetadata] = FamicomFdsConstants.SystemId,
            [FamicomFdsConstants.MediaRoleMetadata] = FamicomFdsConstants.MediaRole,
            [FamicomFdsConstants.HeaderedMetadata] = headered ? bool.TrueString : bool.FalseString,
            [FamicomFdsConstants.HeaderLengthMetadata] = headerLength.ToString(CultureInfo.InvariantCulture),
            [FamicomFdsConstants.SideCountMetadata] = sideCount.ToString(CultureInfo.InvariantCulture),
            [FamicomFdsConstants.DeclaredSideCountMetadata] = declaredSideCount.ToString(CultureInfo.InvariantCulture),
            [FamicomFdsConstants.SideLengthMetadata] = FamicomFdsConstants.SideLength.ToString(CultureInfo.InvariantCulture)
        };
        for (var side = 0; side < sideCount; side++)
        {
            var address = checked(side * (long)FamicomFdsConstants.SideLength);
            ranges.Add(new MediaDataRange(
                address,
                FamicomFdsConstants.SideLength,
                MediaDataRangeKind.Stored,
                source,
                checked(headerLength + address)));
            metadata[$"{FamicomFdsConstants.SideNamePrefix}{side}{FamicomFdsConstants.SideNameSuffix}"] =
                string.Format(CultureInfo.InvariantCulture, FamicomFdsConstants.SideNameFormat, side);
        }

        var capacity = checked((long)sideCount * FamicomFdsConstants.SideLength);
        var diagnostics = headered && declaredSideCount == 0
            ? ["The FDS header does not declare a side count; the count was taken from the stored data."]
            : Array.Empty<string>();
        return new MediaImageDocument(
            context.Source,
            DiskImageFormatIds.NintendoFamicomDisk,
            MediaKind.Floppy,
            new BlockMediaImageRepresentation(capacity, FamicomFdsConstants.SideLength, ranges),
            [new MediaVolumeDescriptor(0, capacity, MediaVolumeOrigins.DirectVolume, name: FamicomFdsConstants.VolumeName)],
            diagnostics,
            metadata);
    }

    private static bool IsHeaderlessLength(long length)
        => length >= FamicomFdsConstants.SideLength
            && length % FamicomFdsConstants.SideLength == 0;

    private static bool IsHeaderedLength(long length, ReadOnlySpan<byte> header)
    {
        if (length <= FamicomFdsConstants.HeaderLength)
            return false;
        var dataLength = length - FamicomFdsConstants.HeaderLength;
        if (dataLength % FamicomFdsConstants.SideLength != 0)
            return false;
        var sideCount = dataLength / FamicomFdsConstants.SideLength;
        var declaredSideCount = header[FamicomFdsConstants.SideCountOffset];
        return sideCount >= FamicomFdsConstants.MinimumSideCount
            && (declaredSideCount == 0 || declaredSideCount == sideCount);
    }
}
