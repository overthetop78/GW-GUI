using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Optical;

namespace GWGUI.MediaEngine.Images.Formats.Optical.Iso;

/// <summary>Reads a headerless optical ISO as one continuous 2048-byte data track.</summary>
public sealed class IsoReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { OpticalImageFormatIds.Iso }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Optical }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => IsoFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId))
            return ValueTask.FromResult(false);
        return ValueTask.FromResult(
            IsoFormat.Extensions.Contains(context.Extension)
            && IsoFormat.IsLengthCompatible(context.Length));
    }

    Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsoFormat.IsLengthCompatible(context.Length))
            throw new InvalidDataException("The ISO image is empty or is not aligned to 2048-byte sectors.");

        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        if (source.Length != context.Length)
            throw new InvalidDataException("The ISO image length changed after recognition.");
        var sectorCount = context.Length / IsoFormat.SectorSize;
        var track = new OpticalTrackDescriptor(
            1,
            1,
            OpticalTrackMode.Mode1Data2048,
            0,
            sectorCount,
            IsoFormat.SectorSize,
            0,
            IsoFormat.SectorSize,
            source,
            0,
            [new OpticalTrackIndex(1, 0)]);
        var representation = new OpticalMediaImageRepresentation(
            context.Length,
            [track],
            associatedFiles: [context.Source.PrimaryPath]);
        return Task.FromResult(new MediaImageDocument(
            context.Source,
            OpticalImageFormatIds.Iso,
            MediaKind.Optical,
            representation,
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sectorSize"] = IsoFormat.SectorSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["trackCount"] = "1"
            }));
    }
}
