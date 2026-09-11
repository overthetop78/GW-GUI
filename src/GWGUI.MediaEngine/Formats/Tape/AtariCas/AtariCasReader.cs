using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.AtariCas;

/// <summary>Reads Atari CAS chunks while retaining every payload and the timing explicitly stored by the format.</summary>
public sealed class AtariCasReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.AtariCas }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Tape }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> Signatures =
        [System.Text.Encoding.ASCII.GetBytes(AtariCasConstants.FileMarkerChunk)];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => AtariCasFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => Signatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (context.Length < AtariCasConstants.ChunkHeaderSize) return false;
        var header = new byte[AtariCasConstants.IdentifierLength];
        await using var input = new FileStream(context.Source.PrimaryPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            AtariCasConstants.ChunkHeaderSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await input.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
        return header.AsSpan().SequenceEqual(System.Text.Encoding.ASCII.GetBytes(AtariCasConstants.FileMarkerChunk));
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        await using var input = new FileStream(context.Source.PrimaryPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var segments = new List<SequentialMediaSegment>();
        var diagnostics = new List<string>();
        var currentBaud = AtariCasConstants.DefaultBaudRate;
        var position = 0L;
        var elapsed = TimeSpan.Zero;
        var first = true;
        while (input.Position < input.Length)
        {
            if (input.Length - input.Position < AtariCasConstants.ChunkHeaderSize)
                throw new InvalidDataException("The Atari CAS chunk header is truncated.");
            var header = new byte[AtariCasConstants.ChunkHeaderSize];
            await input.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
            var id = System.Text.Encoding.ASCII.GetString(header, 0, AtariCasConstants.IdentifierLength);
            var length = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(4));
            var auxiliary = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6));
            var dataOffset = input.Position;
            if (length > input.Length - dataOffset) throw new InvalidDataException($"Atari CAS chunk '{id}' exceeds the file.");
            if (first && id != AtariCasConstants.FileMarkerChunk)
                throw new InvalidDataException("The Atari CAS image does not begin with a FUJI marker.");
            first = false;

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [AtariCasConstants.ChunkIdMetadataKey] = id,
                [AtariCasConstants.AuxiliaryMetadataKey] = auxiliary.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            MediaDataRange? range = length == 0
                ? null
                : new MediaDataRange(dataOffset, length, MediaDataRangeKind.Stored, source, dataOffset);
            if (id == AtariCasConstants.FileMarkerChunk)
            {
                var nameBytes = new byte[length];
                if (length > 0) await input.ReadExactlyAsync(nameBytes, cancellationToken).ConfigureAwait(false);
                metadata["name"] = System.Text.Encoding.ASCII.GetString(nameBytes);
                segments.Add(new SequentialMediaSegment(position++, SequentialSegmentKind.TapeMark, length: length == 0 ? null : length,
                    dataRange: range, metadata: metadata));
            }
            else if (id == AtariCasConstants.BaudRateChunk)
            {
                if (length != 0) diagnostics.Add("An Atari CAS baud chunk contains an unexpected payload that was retained.");
                currentBaud = auxiliary;
                metadata[AtariCasConstants.BaudRateMetadataKey] = currentBaud.ToString(System.Globalization.CultureInfo.InvariantCulture);
                segments.Add(new SequentialMediaSegment(position++, SequentialSegmentKind.Carrier, dataRange: range, metadata: metadata));
                input.Position = dataOffset + length;
            }
            else if (id == AtariCasConstants.DataChunk)
            {
                if (auxiliary > 0)
                {
                    var markDuration = TimeSpan.FromMilliseconds(auxiliary);
                    segments.Add(new SequentialMediaSegment(position++, SequentialSegmentKind.Carrier, start: elapsed, duration: markDuration,
                        direction: SequentialTravelDirection.Forward,
                        metadata: new Dictionary<string, string>(StringComparer.Ordinal) { ["signal"] = "mark" }));
                    elapsed += markDuration;
                }
                var duration = currentBaud > 0 ? TimeSpan.FromSeconds(length * 10d / currentBaud) : (TimeSpan?)null;
                metadata[AtariCasConstants.BaudRateMetadataKey] = currentBaud.ToString(System.Globalization.CultureInfo.InvariantCulture);
                segments.Add(new SequentialMediaSegment(position++, SequentialSegmentKind.DataBlock, length, elapsed, duration,
                    direction: SequentialTravelDirection.Forward, dataRange: range, metadata: metadata));
                if (duration is { } dataDuration) elapsed += dataDuration;
                input.Position = dataOffset + length;
            }
            else if (id == AtariCasConstants.FskChunk)
            {
                if ((length & 1) != 0) throw new InvalidDataException("An Atari CAS FSK chunk has an odd payload length.");
                if (auxiliary > 0) elapsed += TimeSpan.FromMilliseconds(auxiliary);
                var pulseBytes = new byte[length];
                if (length > 0) await input.ReadExactlyAsync(pulseBytes, cancellationToken).ConfigureAwait(false);
                for (var offset = 0; offset < pulseBytes.Length; offset += 2)
                {
                    var units = BinaryPrimitives.ReadUInt16LittleEndian(pulseBytes.AsSpan(offset, 2));
                    var duration = TimeSpan.FromMilliseconds(units * AtariCasConstants.FskDurationUnitMilliseconds);
                    segments.Add(new SequentialMediaSegment(position++, SequentialSegmentKind.Pulse, units, elapsed, duration,
                        direction: SequentialTravelDirection.Forward,
                        metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["signal"] = (offset / 2 & 1) == 0 ? "space" : "mark",
                            ["chunkOffset"] = offset.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        }));
                    elapsed += duration;
                }
                segments.Add(new SequentialMediaSegment(position++, SequentialSegmentKind.DataBlock, length, dataRange: range, metadata: metadata));
            }
            else
            {
                segments.Add(new SequentialMediaSegment(position++, SequentialSegmentKind.Unknown, length: length == 0 ? null : length,
                    dataRange: range, metadata: metadata));
                input.Position = dataOffset + length;
                diagnostics.Add($"Atari CAS chunk '{id}' was retained without interpretation.");
            }
        }
        var representation = new SequentialMediaImageRepresentation(context.Length, elapsed == TimeSpan.Zero ? null : elapsed, segments);
        return new MediaImageDocument(
            context.Source,
            TapeImageFormatIds.AtariCas,
            MediaKind.Tape,
            representation,
            [],
            diagnostics,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["defaultBaudRate"] = AtariCasConstants.DefaultBaudRate.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }
}
