using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Decoding;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Decoding.Sequential.Atari;

/// <summary>Decodes retained Atari CAS data chunks and compatible PCM WAV FSK serial frames.</summary>
public sealed class AtariCassetteDecoder : ISequentialMediaDecoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.AtariCas, TapeImageFormatIds.Wav }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Atari8Bit }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => AtariCasConstants.DecoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanDecode(MediaImageDocument document, string? machineId = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.MediaKind == MediaKind.Tape
            && SupportedFormats.Contains(document.FormatId)
            && (machineId is null || SupportedMachines.Contains(machineId))
            && document.Representation is SequentialMediaImageRepresentation;
    }

    public async Task<SequentialDecodeResult> DecodeAsync(
        MediaImageDocument document,
        string? machineId = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanDecode(document, machineId))
            throw new InvalidDataException("The media document is not compatible with the Atari cassette decoder.");
        var representation = (SequentialMediaImageRepresentation)document.Representation;
        return document.FormatId.Equals(TapeImageFormatIds.AtariCas, StringComparison.OrdinalIgnoreCase)
            ? await DecodeCasAsync(representation, cancellationToken).ConfigureAwait(false)
            : await DecodeWaveAsync(document, representation, cancellationToken).ConfigureAwait(false);
    }

    private async Task<SequentialDecodeResult> DecodeCasAsync(
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        var blocks = new List<SequentialDecodedBlock>();
        var consumed = new HashSet<long>();
        foreach (var segment in representation.Segments ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment.Kind != Enums.SequentialSegmentKind.DataBlock
                || !segment.Metadata.TryGetValue(AtariCasConstants.ChunkIdMetadataKey, out var chunkId)
                || chunkId != AtariCasConstants.DataChunk
                || segment.DataRange is not { Source: not null } range
                || range.Length > int.MaxValue)
                continue;

            var data = new byte[checked((int)range.Length)];
            await range.Source.ReadExactlyAsync(range.SourceOffset, data, cancellationToken).ConfigureAwait(false);
            blocks.Add(CreateBlock(blocks.Count, data, [segment.Position], "cas-data"));
            consumed.Add(segment.Position);
        }

        var pulseSegments = (representation.Segments ?? [])
            .Where(segment => segment.Kind == Enums.SequentialSegmentKind.Pulse
                && segment.Duration is not null
                && segment.Metadata.ContainsKey("signal"))
            .OrderBy(segment => segment.Position)
            .ToArray();
        if (pulseSegments.Length > 0)
        {
            var bits = new List<bool>();
            foreach (var pulse in pulseSegments)
            {
                var bitCount = Math.Max(1, (int)Math.Round(pulse.Duration!.Value.TotalSeconds * AtariCasConstants.DefaultBaudRate));
                var mark = pulse.Metadata["signal"].Equals("mark", StringComparison.OrdinalIgnoreCase);
                for (var index = 0; index < bitCount; index++) bits.Add(mark);
            }
            var decoded = FskSerialFunctions.DecodeFrames(
                bits,
                AtariCasConstants.SerialDataBitCount,
                AtariCasConstants.SerialFrameBitCount - AtariCasConstants.SerialDataBitCount - 1);
            if (decoded.Bytes.Count > 0)
            {
                var positions = pulseSegments.Select(segment => segment.Position).ToArray();
                blocks.Add(CreateBlock(blocks.Count, decoded.Bytes.ToArray(), positions, "cas-fsk"));
                foreach (var positionValue in positions) consumed.Add(positionValue);
            }
        }

        var undecoded = (representation.Segments ?? []).Where(segment => !consumed.Contains(segment.Position)).ToArray();
        var confidence = blocks.Count == 0 ? 0 : 1;
        var diagnostics = blocks.Count == 0 ? new[] { "No Atari CAS data chunk could be decoded." } : [];
        return new SequentialDecodeResult(Id, confidence, blocks, undecoded, diagnostics);
    }

    private async Task<SequentialDecodeResult> DecodeWaveAsync(
        MediaImageDocument document,
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        if (!PcmSampleFunctions.TryGetProfile(document.Metadata, out var channels, out var sampleRate, out var bitsPerSample, out var blockAlign))
            return new SequentialDecodeResult(Id, 0, [], representation.Segments ?? [], ["The WAV PCM profile is incomplete or incompatible."]);

        var sampleSegments = (representation.Segments ?? [])
            .Where(segment => segment.Kind == Enums.SequentialSegmentKind.Samples && segment.ChannelNumber == 0 && segment.DataRange?.Source is not null)
            .OrderBy(segment => segment.Position)
            .ToArray();
        if (sampleSegments.Length == 0)
            return new SequentialDecodeResult(Id, 0, [], representation.Segments ?? [], ["The WAV image has no readable PCM sample segment."]);

        var bytes = new List<byte>();
        var sourcePositions = new List<long>();
        var validFrames = 0;
        var testedFrames = 0;
        foreach (var segment in sampleSegments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var range = segment.DataRange!;
            if (range.Length > int.MaxValue) continue;
            var pcm = new byte[checked((int)range.Length)];
            await range.Source!.ReadExactlyAsync(range.SourceOffset, pcm, cancellationToken).ConfigureAwait(false);
            var samples = PcmSampleFunctions.ExtractChannel(pcm, channels, bitsPerSample, blockAlign, 0);
            var decoded = FskSerialFunctions.Decode(
                samples,
                sampleRate,
                AtariCasConstants.DefaultBaudRate,
                AtariCasConstants.SpaceFrequency,
                AtariCasConstants.MarkFrequency,
                AtariCasConstants.SerialDataBitCount,
                AtariCasConstants.SerialFrameBitCount - AtariCasConstants.SerialDataBitCount - 1,
                AtariCasConstants.MinimumSamplesPerBit);
            bytes.AddRange(decoded.Bytes);
            validFrames += decoded.ValidFrames;
            testedFrames += decoded.TestedFrames;
            if (decoded.Bytes.Count > 0) sourcePositions.Add(segment.Position);
        }

        var blocks = bytes.Count == 0
            ? Array.Empty<SequentialDecodedBlock>()
            : [CreateBlock(0, bytes.ToArray(), sourcePositions, "wav-fsk")];
        var consumed = sourcePositions.ToHashSet();
        var undecoded = (representation.Segments ?? []).Where(segment => !consumed.Contains(segment.Position)).ToArray();
        var confidence = testedFrames == 0 ? 0 : validFrames / (double)testedFrames;
        var diagnostics = bytes.Count == 0 ? new[] { "No complete Atari FSK serial frame was found in the selected WAV channel." } : [];
        return new SequentialDecodeResult(Id, confidence, blocks, undecoded, diagnostics);
    }

    private static SequentialDecodedBlock CreateBlock(
        long position,
        byte[] data,
        IReadOnlyList<long> sourcePositions,
        string sourceKind)
    {
        var integrity = data.Length < 2 ? (bool?)null : CalculateSioChecksum(data.AsSpan(0, data.Length - 1)) == data[^1];
        return new SequentialDecodedBlock(
            position,
            data,
            sourcePositions,
            integrity,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceKind"] = sourceKind,
                ["checksumPresent"] = (data.Length >= 2).ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static byte CalculateSioChecksum(ReadOnlySpan<byte> data)
    {
        var checksum = 0;
        foreach (var value in data)
        {
            checksum += value;
            checksum = (checksum & 0xff) + (checksum >> 8);
        }
        return (byte)checksum;
    }

}
