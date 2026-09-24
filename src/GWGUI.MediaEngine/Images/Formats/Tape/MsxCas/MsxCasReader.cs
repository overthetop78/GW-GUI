using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Images.Formats.Tape.MsxCas;

/// <summary>Reads byte groups delimited by verified MSX CAS separators without reconstructing absent timing.</summary>
public sealed class MsxCasReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.MsxCas }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Tape }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> Signatures = [MsxCasConstants.Separator.ToArray()];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => MsxCasFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => Signatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        var separators = await FindSeparatorsAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
        if (separators.Count == 0) return false;
        if (context.RequestedFormatId is not null) return true;
        return separators[0] == 0
            && await HasKnownFileTypeMarkerAsync(context.Source.PrimaryPath, MsxCasConstants.SeparatorLength, cancellationToken)
                .ConfigureAwait(false);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var separators = await FindSeparatorsAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
        if (separators.Count == 0) throw new InvalidDataException("The image contains no complete MSX CAS separator.");
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        var segments = new List<SequentialMediaSegment>();
        var diagnostics = new List<string>();
        var position = 0L;

        if (separators[0] > 0)
        {
            segments.Add(new SequentialMediaSegment(
                position++,
                SequentialSegmentKind.Unknown,
                separators[0],
                dataRange: new MediaDataRange(0, separators[0], MediaDataRangeKind.Stored, source, 0)));
            diagnostics.Add("Bytes before the first MSX CAS separator were retained without interpretation.");
        }

        for (var index = 0; index < separators.Count; index++)
        {
            var separatorOffset = separators[index];
            segments.Add(new SequentialMediaSegment(
                position++,
                SequentialSegmentKind.TapeMark,
                MsxCasConstants.SeparatorLength,
                dataRange: new MediaDataRange(
                    separatorOffset,
                    MsxCasConstants.SeparatorLength,
                    MediaDataRangeKind.Stored,
                    source,
                    separatorOffset),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [MsxCasConstants.SeparatorMetadataKey] = bool.TrueString
                }));

            var dataOffset = separatorOffset + MsxCasConstants.SeparatorLength;
            var nextOffset = index + 1 < separators.Count ? separators[index + 1] : context.Length;
            var length = nextOffset - dataOffset;
            if (length <= 0)
            {
                diagnostics.Add($"MSX CAS separator at offset {separatorOffset.ToString(CultureInfo.InvariantCulture)} has no following data.");
                continue;
            }
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [MsxCasConstants.TimingMetadataKey] = "not-stored"
            };
            var marker = await ReadFileTypeMarkerAsync(context.Source.PrimaryPath, dataOffset, length, cancellationToken).ConfigureAwait(false);
            if (marker is not null) metadata[MsxCasConstants.FileTypeMetadataKey] = marker;
            segments.Add(new SequentialMediaSegment(
                position++,
                SequentialSegmentKind.DataBlock,
                length,
                direction: SequentialTravelDirection.Forward,
                dataRange: new MediaDataRange(dataOffset, length, MediaDataRangeKind.Stored, source, dataOffset),
                metadata: metadata));
        }

        return new MediaImageDocument(
            context.Source,
            TapeImageFormatIds.MsxCas,
            MediaKind.Tape,
            new SequentialMediaImageRepresentation(context.Length, segments: segments),
            [],
            diagnostics,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [MsxCasConstants.TimingMetadataKey] = "not-stored"
            });
    }

    private static async Task<IReadOnlyList<long>> FindSeparatorsAsync(string path, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var separator = MsxCasConstants.Separator.ToArray();
        var result = new List<long>();
        var buffer = new byte[64 * 1024];
        var matched = 0;
        var absolute = 0L;
        int count;
        while ((count = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            for (var index = 0; index < count; index++, absolute++)
            {
                if (buffer[index] == separator[matched])
                {
                    matched++;
                    if (matched != separator.Length) continue;
                    result.Add(absolute - separator.Length + 1);
                    matched = 0;
                }
                else
                {
                    matched = buffer[index] == separator[0] ? 1 : 0;
                }
            }
        }
        return result;
    }

    private static async Task<bool> HasKnownFileTypeMarkerAsync(
        string path,
        long offset,
        CancellationToken cancellationToken) =>
        await ReadFileTypeMarkerAsync(path, offset, MsxCasConstants.FileTypeMarkerLength, cancellationToken).ConfigureAwait(false) is not null;

    private static async Task<string?> ReadFileTypeMarkerAsync(
        string path,
        long offset,
        long available,
        CancellationToken cancellationToken)
    {
        if (available < MsxCasConstants.FileTypeMarkerLength) return null;
        var marker = new byte[MsxCasConstants.FileTypeMarkerLength];
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            MsxCasConstants.FileTypeMarkerLength, FileOptions.Asynchronous | FileOptions.RandomAccess);
        input.Position = offset;
        await input.ReadExactlyAsync(marker, cancellationToken).ConfigureAwait(false);
        if (marker.All(value => value == MsxCasConstants.BinaryFileMarker)) return "binary";
        if (marker.All(value => value == MsxCasConstants.TokenizedBasicFileMarker)) return "basic";
        if (marker.All(value => value == MsxCasConstants.AsciiFileMarker)) return "ascii";
        return null;
    }
}
