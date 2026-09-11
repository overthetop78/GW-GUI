using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.IO.Compression;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.Uef;

/// <summary>Reads bounded direct or gzip UEF containers and retains every chunk payload in source order.</summary>
public sealed class UefReader : IMediaImageReader
{
    private static readonly byte[] Signature = System.Text.Encoding.ASCII.GetBytes(UefConstants.Signature);
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.Uef }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Tape }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => UefFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [Signature];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        try
        {
            var header = await ReadLogicalHeaderAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
            return IsSupportedHeader(header);
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var storedHeader = await context.ReadHeaderAsync(Math.Min(2, checked((int)context.Length)), cancellationToken).ConfigureAwait(false);
        var gzip = IsGzip(storedHeader.Span);
        var bytes = await ReadLogicalBytesAsync(context.Source.PrimaryPath, gzip, cancellationToken).ConfigureAwait(false);
        if (!IsSupportedHeader(bytes)) throw new InvalidDataException("The UEF header or version is unsupported.");

        var source = new MemoryRandomAccessData(bytes);
        var segments = new List<SequentialMediaSegment>();
        var diagnostics = new List<string>();
        var offset = UefConstants.HeaderSize;
        var position = 0L;
        var baseFrequency = UefConstants.DefaultBaseFrequency;
        var baudRate = UefConstants.DefaultBaudRate;
        ushort phase = 180;
        while (offset < bytes.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (bytes.Length - offset < UefConstants.ChunkHeaderSize)
                throw new InvalidDataException("The final UEF chunk header is truncated.");
            var id = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2));
            var length = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset + 2, 4));
            var payloadOffset = offset + UefConstants.ChunkHeaderSize;
            if (length > bytes.Length - payloadOffset)
                throw new InvalidDataException($"UEF chunk 0x{id:x4} exceeds the container.");

            var payload = bytes.AsSpan(payloadOffset, checked((int)length));
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [UefConstants.ChunkIdMetadataKey] = id.ToString("x4", CultureInfo.InvariantCulture),
                ["known"] = UefConstants.UnderstoodTapeChunks.Contains(id).ToString(CultureInfo.InvariantCulture),
                [UefConstants.BaseFrequencyMetadataKey] = baseFrequency.ToString(CultureInfo.InvariantCulture),
                [UefConstants.BaudRateMetadataKey] = baudRate.ToString(CultureInfo.InvariantCulture),
                [UefConstants.PhaseMetadataKey] = phase.ToString(CultureInfo.InvariantCulture)
            };
            TimeSpan? duration = null;
            int? faceNumber = null;
            if (id == UefConstants.BaseFrequency && payload.Length == sizeof(float))
            {
                var value = BinaryPrimitives.ReadSingleLittleEndian(payload);
                if (float.IsFinite(value) && value > 0) baseFrequency = checked((int)Math.Round(value));
                metadata[UefConstants.BaseFrequencyMetadataKey] = baseFrequency.ToString(CultureInfo.InvariantCulture);
            }
            else if (id == UefConstants.BaudRate && payload.Length == sizeof(float))
            {
                var value = BinaryPrimitives.ReadSingleLittleEndian(payload);
                if (float.IsFinite(value) && value > 0) baudRate = checked((int)Math.Round(value));
                metadata[UefConstants.BaudRateMetadataKey] = baudRate.ToString(CultureInfo.InvariantCulture);
            }
            else if (id == UefConstants.PhaseChange && payload.Length == sizeof(ushort))
            {
                phase = BinaryPrimitives.ReadUInt16LittleEndian(payload);
                metadata[UefConstants.PhaseMetadataKey] = phase.ToString(CultureInfo.InvariantCulture);
            }
            else if (id == UefConstants.FloatingGap && payload.Length == sizeof(float))
            {
                var seconds = BinaryPrimitives.ReadSingleLittleEndian(payload);
                if (float.IsFinite(seconds) && seconds > 0) duration = TimeSpan.FromSeconds(seconds);
            }
            else if (id == UefConstants.TapeSide && payload.Length > 0)
            {
                faceNumber = payload[0];
            }

            var range = length == 0
                ? null
                : new MediaDataRange(payloadOffset, length, MediaDataRangeKind.Stored, source, payloadOffset);
            segments.Add(new SequentialMediaSegment(
                position++,
                SegmentKind(id),
                length == 0 ? null : length,
                duration: duration,
                faceNumber: faceNumber,
                direction: SequentialTravelDirection.Forward,
                dataRange: range,
                metadata: metadata));
            if (!UefConstants.UnderstoodTapeChunks.Contains(id))
                diagnostics.Add($"UEF chunk 0x{id:x4} was retained without interpretation.");
            offset = checked(payloadOffset + (int)length);
        }

        return new MediaImageDocument(
            context.Source,
            TapeImageFormatIds.Uef,
            MediaKind.Tape,
            new SequentialMediaImageRepresentation(bytes.Length, segments: segments),
            [],
            diagnostics,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [UefConstants.GzipMetadataKey] = gzip.ToString(CultureInfo.InvariantCulture),
                [UefConstants.MinorVersionMetadataKey] = bytes[10].ToString(CultureInfo.InvariantCulture),
                [UefConstants.MajorVersionMetadataKey] = bytes[11].ToString(CultureInfo.InvariantCulture)
            });
    }

    private static SequentialSegmentKind SegmentKind(ushort id) => id switch
    {
        UefConstants.ImplicitData or UefConstants.MultiplexedData or UefConstants.ExplicitData or UefConstants.DefinedData =>
            SequentialSegmentKind.DataBlock,
        UefConstants.CarrierTone or UefConstants.CarrierToneWithDummy => SequentialSegmentKind.Carrier,
        UefConstants.IntegerGap or UefConstants.FloatingGap => SequentialSegmentKind.Silence,
        UefConstants.SecurityCycles => SequentialSegmentKind.Pulse,
        UefConstants.PhaseChange or UefConstants.BaseFrequency or UefConstants.BaudRate or
            UefConstants.PositionMarker or UefConstants.TapeSetInformation or UefConstants.TapeSide => SequentialSegmentKind.TapeMark,
        _ => SequentialSegmentKind.Unknown
    };

    private static async Task<byte[]> ReadLogicalHeaderAsync(string path, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            UefConstants.HeaderSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var first = new byte[2];
        if (await input.ReadAsync(first, cancellationToken).ConfigureAwait(false) != first.Length) return [];
        input.Position = 0;
        Stream logical = IsGzip(first) ? new GZipStream(input, CompressionMode.Decompress, leaveOpen: false) : input;
        await using (logical.ConfigureAwait(false))
        {
            var header = new byte[UefConstants.HeaderSize];
            try
            {
                await logical.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
                return header;
            }
            catch (EndOfStreamException)
            {
                return [];
            }
        }
    }

    private static async Task<byte[]> ReadLogicalBytesAsync(string path, bool gzip, CancellationToken cancellationToken)
    {
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        if (!gzip)
        {
            if (input.Length > UefConstants.MaximumDecompressedLength)
                throw new InvalidDataException("The UEF container exceeds the configured reading limit.");
            var direct = new byte[checked((int)input.Length)];
            await input.ReadExactlyAsync(direct, cancellationToken).ConfigureAwait(false);
            return direct;
        }

        await using var compressed = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        var buffer = new byte[64 * 1024];
        int count;
        while ((count = await compressed.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (output.Length + count > UefConstants.MaximumDecompressedLength)
                throw new InvalidDataException("The gzip UEF container exceeds the configured decompression limit.");
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
        }
        return output.ToArray();
    }

    private static bool IsSupportedHeader(ReadOnlySpan<byte> header) =>
        header.Length >= UefConstants.HeaderSize
        && header[..UefConstants.SignatureLength].SequenceEqual(Signature)
        && header[11] == UefConstants.SupportedMajorVersion
        && header[10] <= UefConstants.SupportedMinorVersion;

    private static bool IsGzip(ReadOnlySpan<byte> header) =>
        header.Length >= 2
        && header[0] == UefConstants.GzipIdentificationFirst
        && header[1] == UefConstants.GzipIdentificationSecond;
}
