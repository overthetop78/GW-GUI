using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Simh;

/// <summary>Validates complete SIMH tape object framing before exposing records and markers.</summary>
public sealed class SimhTapeReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.SimhTap }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Tape }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SimhTapeFormat.Extensions;
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
            await ParseAsync(context.Source.PrimaryPath, createSegments: false, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var parsed = await ParseAsync(context.Source.PrimaryPath, createSegments: true, cancellationToken).ConfigureAwait(false);
        return new MediaImageDocument(
            context.Source,
            TapeImageFormatIds.SimhTap,
            MediaKind.Tape,
            new SequentialMediaImageRepresentation(context.Length, segments: parsed.Segments),
            [],
            parsed.Diagnostics,
            new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private static async Task<ParseResult> ParseAsync(
        string path,
        bool createSegments,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (stream.Length < SimhTapeConstants.WordSize)
            throw new InvalidDataException("A SIMH tape image must contain at least one object word.");
        var source = createSegments ? new FileRandomAccessData(path) : null;
        var segments = new List<SequentialMediaSegment>();
        var diagnostics = new List<string>();
        var wordBuffer = new byte[SimhTapeConstants.WordSize];
        var position = 0L;
        var objectCount = 0;
        while (stream.Position < stream.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (stream.Length - stream.Position < SimhTapeConstants.WordSize)
                throw new InvalidDataException("The final SIMH tape object word is truncated.");
            await stream.ReadExactlyAsync(wordBuffer, cancellationToken).ConfigureAwait(false);
            var word = BinaryPrimitives.ReadUInt32LittleEndian(wordBuffer);
            objectCount++;

            if (word == SimhTapeConstants.TapeMark)
            {
                if (createSegments) segments.Add(Marker(position++, SequentialSegmentKind.TapeMark, word, SimhTapeConstants.TapeMarkKind));
                continue;
            }
            if (word == SimhTapeConstants.EraseGap)
            {
                if (createSegments) segments.Add(Marker(position++, SequentialSegmentKind.Silence, word, SimhTapeConstants.EraseGapKind));
                continue;
            }
            if (word == SimhTapeConstants.EndOfMedium)
            {
                if (createSegments) segments.Add(Marker(position++, SequentialSegmentKind.TapeMark, word, SimhTapeConstants.EndOfMediumKind));
                if (stream.Position < stream.Length)
                    diagnostics.Add($"{stream.Length - stream.Position} physical byte(s) follow the logical SIMH end-of-medium marker.");
                break;
            }
            if ((word & SimhTapeConstants.ReservedLengthBits) != 0)
                throw new InvalidDataException($"SIMH tape word 0x{word:x8} uses reserved record-length bits.");

            var length = word & SimhTapeConstants.RecordLengthMask;
            if (length == 0) throw new InvalidDataException("A SIMH data record cannot have zero length.");
            var paddedLength = (length + SimhTapeConstants.RecordAlignment - 1)
                & ~(uint)(SimhTapeConstants.RecordAlignment - 1);
            if (stream.Length - stream.Position < paddedLength + SimhTapeConstants.WordSize)
                throw new InvalidDataException("A SIMH data record exceeds the image boundary.");
            var dataOffset = stream.Position;
            stream.Position += paddedLength;
            await stream.ReadExactlyAsync(wordBuffer, cancellationToken).ConfigureAwait(false);
            var trailingWord = BinaryPrimitives.ReadUInt32LittleEndian(wordBuffer);
            if (trailingWord != word)
                throw new InvalidDataException("The leading and trailing SIMH record-length words differ.");

            if (!createSegments) continue;
            var hasError = (word & SimhTapeConstants.ErrorFlag) != 0;
            segments.Add(new SequentialMediaSegment(
                position++,
                SequentialSegmentKind.DataBlock,
                length,
                direction: SequentialTravelDirection.Forward,
                dataRange: new MediaDataRange(dataOffset, length, MediaDataRangeKind.Stored, source, dataOffset),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [SimhTapeConstants.ObjectKindMetadataKey] = SimhTapeConstants.DataRecordKind,
                    [SimhTapeConstants.RecordErrorMetadataKey] = hasError.ToString(CultureInfo.InvariantCulture)
                }));
            if (hasError) diagnostics.Add($"SIMH record {position - 1} is marked as containing an error.");
        }

        if (objectCount == 0) throw new InvalidDataException("The SIMH tape image contains no object.");
        return new ParseResult(segments, diagnostics);
    }

    private static SequentialMediaSegment Marker(
        long position,
        SequentialSegmentKind kind,
        uint value,
        string objectKind) =>
        new(
            position,
            kind,
            direction: SequentialTravelDirection.Forward,
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [SimhTapeConstants.ObjectKindMetadataKey] = objectKind,
                [SimhTapeConstants.MarkerValueMetadataKey] = value.ToString("x8", CultureInfo.InvariantCulture)
            });

    private sealed record ParseResult(
        IReadOnlyList<SequentialMediaSegment> Segments,
        IReadOnlyList<string> Diagnostics);
}
