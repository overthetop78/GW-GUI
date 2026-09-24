using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Wav;

/// <summary>Reads bounded integer PCM RIFF/WAVE sources as interleaved sequential sample segments.</summary>
public sealed class WavTapeReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.Wav }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Tape }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> RiffSignatures =
        [System.Text.Encoding.ASCII.GetBytes(WavConstants.RiffIdentifier)];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => WavTapeFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => RiffSignatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (context.Length < WavConstants.RiffHeaderSize) return false;
        var header = new byte[WavConstants.RiffHeaderSize];
        await using var input = new FileStream(context.Source.PrimaryPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            WavConstants.RiffHeaderSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        await input.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
        return IsWaveHeader(header);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        await using var input = new FileStream(context.Source.PrimaryPath, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var header = new byte[WavConstants.RiffHeaderSize];
        await input.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
        if (!IsWaveHeader(header)) throw new InvalidDataException("The RIFF/WAVE signature is missing.");
        var riffLength = checked((long)BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4)) + 8);
        if (riffLength > input.Length) throw new InvalidDataException("The RIFF length exceeds the WAV file.");

        ushort channels = 0;
        uint sampleRate = 0;
        ushort bitsPerSample = 0;
        ushort blockAlign = 0;
        var chunks = new List<Chunk>();
        while (input.Position <= riffLength - WavConstants.ChunkHeaderSize)
        {
            var chunkHeader = new byte[WavConstants.ChunkHeaderSize];
            await input.ReadExactlyAsync(chunkHeader, cancellationToken).ConfigureAwait(false);
            var id = System.Text.Encoding.ASCII.GetString(chunkHeader, 0, 4);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(chunkHeader.AsSpan(4));
            var dataOffset = input.Position;
            if (length > riffLength - dataOffset) throw new InvalidDataException($"WAV chunk '{id}' exceeds the RIFF container.");
            if (id == WavConstants.FormatChunkIdentifier)
            {
                if (length < WavConstants.PcmFormatChunkSize) throw new InvalidDataException("The WAV format chunk is incomplete.");
                var format = new byte[WavConstants.PcmFormatChunkSize];
                await input.ReadExactlyAsync(format, cancellationToken).ConfigureAwait(false);
                if (BinaryPrimitives.ReadUInt16LittleEndian(format) != WavConstants.PcmFormatTag)
                    throw new NotSupportedException("Only integer PCM WAV sources are supported.");
                channels = BinaryPrimitives.ReadUInt16LittleEndian(format.AsSpan(2));
                sampleRate = BinaryPrimitives.ReadUInt32LittleEndian(format.AsSpan(4));
                blockAlign = BinaryPrimitives.ReadUInt16LittleEndian(format.AsSpan(12));
                bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(format.AsSpan(14));
            }
            chunks.Add(new Chunk(id, dataOffset, length));
            input.Position = checked(dataOffset + length + (length & 1));
        }
        if (!WavTapeFormat.IsSupportedPcm(channels, checked((int)sampleRate), bitsPerSample))
            throw new NotSupportedException("The WAV PCM channel count, sample rate, or sample depth is unsupported.");
        var expectedBlockAlign = checked((ushort)(channels * (bitsPerSample / 8)));
        if (blockAlign != expectedBlockAlign) throw new InvalidDataException("The WAV PCM block alignment is inconsistent.");
        var dataChunks = chunks.Where(chunk => chunk.Id == WavConstants.DataChunkIdentifier).ToArray();
        if (dataChunks.Length == 0) throw new InvalidDataException("The WAV data chunk is missing.");

        var segments = new List<SequentialMediaSegment>();
        long totalFrames = 0;
        foreach (var chunk in chunks)
        {
            if (chunk.Id == WavConstants.DataChunkIdentifier)
            {
                if (chunk.Length % blockAlign != 0) throw new InvalidDataException("A WAV data chunk ends inside a PCM sample frame.");
                var frames = chunk.Length / blockAlign;
                var start = TimeSpan.FromSeconds(totalFrames / (double)sampleRate);
                var duration = TimeSpan.FromSeconds(frames / (double)sampleRate);
                for (var channel = 0; channel < channels; channel++)
                    segments.Add(new SequentialMediaSegment(
                        totalFrames,
                        SequentialSegmentKind.Samples,
                        frames,
                        start,
                        duration,
                        channelNumber: channel,
                        direction: SequentialTravelDirection.Forward,
                        dataRange: new MediaDataRange(chunk.Offset, chunk.Length, MediaDataRangeKind.Stored, source, chunk.Offset),
                        metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["channel"] = channel.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            ["interleaved"] = "true",
                            ["blockAlign"] = blockAlign.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        }));
                totalFrames += frames;
            }
            else if (chunk.Id != WavConstants.FormatChunkIdentifier)
            {
                segments.Add(new SequentialMediaSegment(
                    chunk.Offset,
                    SequentialSegmentKind.Unknown,
                    chunk.Length,
                    dataRange: chunk.Length == 0 ? null : new MediaDataRange(chunk.Offset, chunk.Length, MediaDataRangeKind.Stored, source, chunk.Offset),
                    metadata: new Dictionary<string, string>(StringComparer.Ordinal) { ["chunkId"] = chunk.Id }));
            }
        }
        var totalDuration = TimeSpan.FromSeconds(totalFrames / (double)sampleRate);
        var representation = new SequentialMediaImageRepresentation(context.Length, totalDuration, segments);
        return new MediaImageDocument(
            context.Source,
            TapeImageFormatIds.Wav,
            MediaKind.Tape,
            representation,
            [],
            riffLength == input.Length ? [] : ["Bytes following the RIFF container were not interpreted."],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["channels"] = channels.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sampleRate"] = sampleRate.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["bitsPerSample"] = bitsPerSample.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["blockAlign"] = blockAlign.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static bool IsWaveHeader(ReadOnlySpan<byte> header) =>
        header[..4].SequenceEqual(System.Text.Encoding.ASCII.GetBytes(WavConstants.RiffIdentifier))
        && header.Slice(8, 4).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(WavConstants.WaveIdentifier));

    private sealed record Chunk(string Id, long Offset, uint Length);
}
