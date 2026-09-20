using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Decoding;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Decoding.Sequential.Spectrum;

/// <summary>Validates Spectrum blocks retained in TAP or TZX and detects standard Spectrum pulses in PCM WAV.</summary>
public sealed class SpectrumTapeDecoder : ISequentialMediaDecoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.SpectrumTap, TapeImageFormatIds.Tzx, TapeImageFormatIds.Wav }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Spectrum }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => SpectrumTapConstants.DecoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanDecode(MediaImageDocument document, string? machineId = null) =>
        document.MediaKind == MediaKind.Tape
        && SupportedFormats.Contains(document.FormatId)
        && (machineId is null || SupportedMachines.Contains(machineId))
        && document.Representation is SequentialMediaImageRepresentation;

    public async Task<SequentialDecodeResult> DecodeAsync(
        MediaImageDocument document,
        string? machineId = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanDecode(document, machineId))
            throw new InvalidDataException("The media document is not compatible with the Spectrum tape decoder.");
        var representation = (SequentialMediaImageRepresentation)document.Representation;
        return document.FormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase)
            ? await DecodeWaveAsync(document, representation, cancellationToken).ConfigureAwait(false)
            : await DecodeStructuredAsync(document.FormatId, representation, cancellationToken).ConfigureAwait(false);
    }

    private async Task<SequentialDecodeResult> DecodeStructuredAsync(
        string formatId,
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        var blocks = new List<SequentialDecodedBlock>();
        var consumed = new HashSet<long>();
        foreach (var segment in representation.Segments ?? [])
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[]? data = null;
            if (formatId.Equals(TapeImageFormatIds.SpectrumTap, StringComparison.OrdinalIgnoreCase)
                && segment.Metadata.ContainsKey(SpectrumTapConstants.BlockMetadataKey))
                data = await ReadRangeAsync(segment.DataRange, 0, segment.DataRange?.Length ?? 0, cancellationToken).ConfigureAwait(false);
            else if (formatId.Equals(TapeImageFormatIds.Tzx, StringComparison.OrdinalIgnoreCase)
                && segment.DataRange is { Length: <= int.MaxValue })
            {
                var payload = await ReadRangeAsync(segment.DataRange, 0, segment.DataRange.Length, cancellationToken).ConfigureAwait(false);
                if (payload is not null && TryTzxData(segment, payload, out var dataOffset, out var dataLength))
                    data = payload.AsSpan(dataOffset, dataLength).ToArray();
            }
            if (data is null || data.Length < 2) continue;
            blocks.Add(CreateBlock(blocks.Count, data, [segment.Position], formatId));
            consumed.Add(segment.Position);
        }
        var validCount = blocks.Count(block => block.IntegrityValid == true);
        var confidence = blocks.Count == 0 ? 0 : validCount / (double)blocks.Count;
        return new SequentialDecodeResult(
            Id,
            confidence,
            blocks,
            (representation.Segments ?? []).Where(segment => !consumed.Contains(segment.Position)).ToArray(),
            blocks.Count == 0 ? ["No complete Spectrum data block was found."] : []);
    }

    private async Task<SequentialDecodeResult> DecodeWaveAsync(
        MediaImageDocument document,
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        if (!PcmSampleFunctions.TryGetProfile(document.Metadata, out var channels, out var sampleRate, out var bitsPerSample, out var blockAlign))
            return new SequentialDecodeResult(Id, 0, [], representation.Segments ?? [], ["The WAV PCM profile is incomplete or incompatible."]);
        var blocks = new List<SequentialDecodedBlock>();
        var consumed = new HashSet<long>();
        foreach (var segment in (representation.Segments ?? []).Where(segment => segment.Kind == SequentialSegmentKind.Samples && segment.ChannelNumber == 0))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pcm = await ReadRangeAsync(segment.DataRange, 0, segment.DataRange?.Length ?? 0, cancellationToken).ConfigureAwait(false);
            if (pcm is null) continue;
            var samples = PcmSampleFunctions.ExtractChannel(pcm, channels, bitsPerSample, blockAlign, 0);
            var detected = DetectWaveBlocks(samples, sampleRate);
            foreach (var data in detected) blocks.Add(CreateBlock(blocks.Count, data, [segment.Position], "wav"));
            if (detected.Count > 0) consumed.Add(segment.Position);
        }
        var validCount = blocks.Count(block => block.IntegrityValid == true);
        var confidence = blocks.Count == 0 ? 0 : validCount / (double)blocks.Count;
        return new SequentialDecodeResult(
            Id,
            confidence,
            blocks,
            (representation.Segments ?? []).Where(segment => !consumed.Contains(segment.Position)).ToArray(),
            blocks.Count == 0 ? ["No standard Spectrum pulse block was detected in the selected WAV channel."] : []);
    }

    private static IReadOnlyList<byte[]> DetectWaveBlocks(IReadOnlyList<int> samples, int sampleRate)
    {
        var pulses = new List<double>();
        var previousSign = samples.Count > 0 && samples[0] >= 0;
        var previousCrossing = 0;
        for (var index = 1; index < samples.Count; index++)
        {
            var sign = samples[index] >= 0;
            if (sign == previousSign) continue;
            pulses.Add((index - previousCrossing) * (double)TzxConstants.TStatesPerSecond / sampleRate);
            previousCrossing = index;
            previousSign = sign;
        }

        var blocks = new List<byte[]>();
        for (var index = 0; index < pulses.Count;)
        {
            var pilotStart = index;
            while (index < pulses.Count && Close(pulses[index], SpectrumTapConstants.PilotPulseTStates)) index++;
            if (index - pilotStart < SpectrumTapConstants.MinimumDetectedPilotPulseCount
                || index + 1 >= pulses.Count
                || !Close(pulses[index], SpectrumTapConstants.SyncFirstPulseTStates)
                || !Close(pulses[index + 1], SpectrumTapConstants.SyncSecondPulseTStates))
            {
                index = pilotStart + 1;
                continue;
            }
            index += 2;
            var bits = new List<bool>();
            while (index + 1 < pulses.Count)
            {
                var zero = Close(pulses[index], SpectrumTapConstants.ZeroPulseTStates)
                    && Close(pulses[index + 1], SpectrumTapConstants.ZeroPulseTStates);
                var one = Close(pulses[index], SpectrumTapConstants.OnePulseTStates)
                    && Close(pulses[index + 1], SpectrumTapConstants.OnePulseTStates);
                if (!zero && !one) break;
                bits.Add(one);
                index += 2;
            }
            var byteCount = bits.Count / 8;
            if (byteCount < 2) continue;
            var data = new byte[byteCount];
            for (var byteIndex = 0; byteIndex < byteCount; byteIndex++)
                for (var bit = 0; bit < 8; bit++)
                    if (bits[byteIndex * 8 + bit]) data[byteIndex] |= (byte)(0x80 >> bit);
            blocks.Add(data);
        }
        return blocks;
    }

    private static bool TryTzxData(SequentialMediaSegment segment, byte[] payload, out int offset, out int length)
    {
        offset = length = 0;
        if (!segment.Metadata.TryGetValue(TzxConstants.BlockIdMetadataKey, out var idText)
            || !byte.TryParse(idText, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var id))
            return false;
        var prefixLength = id switch
        {
            TzxConstants.StandardSpeedData => 4,
            TzxConstants.TurboSpeedData => 18,
            TzxConstants.PureData => 10,
            _ => -1
        };
        if (prefixLength < 0 || payload.Length < prefixLength) return false;
        length = id switch
        {
            TzxConstants.StandardSpeedData => BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(2)),
            TzxConstants.TurboSpeedData => ReadUInt24(payload.AsSpan(15)),
            TzxConstants.PureData => ReadUInt24(payload.AsSpan(7)),
            _ => 0
        };
        offset = prefixLength;
        return length >= 2 && offset + (long)length <= payload.Length;
    }

    private static async Task<byte[]?> ReadRangeAsync(MediaDataRange? range, long relativeOffset, long length, CancellationToken cancellationToken)
    {
        if (range is not { Source: not null } || length <= 0 || length > int.MaxValue || relativeOffset < 0 || relativeOffset + length > range.Length)
            return null;
        var data = new byte[checked((int)length)];
        await range.Source.ReadExactlyAsync(range.SourceOffset + relativeOffset, data, cancellationToken).ConfigureAwait(false);
        return data;
    }

    private static SequentialDecodedBlock CreateBlock(long position, byte[] data, IReadOnlyList<long> sourcePositions, string sourceFormat) =>
        new(
            position,
            data,
            sourcePositions,
            Xor(data) == 0,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceFormat"] = sourceFormat,
                ["flag"] = data[0].ToString("x2", System.Globalization.CultureInfo.InvariantCulture),
                ["header"] = (data.Length == SpectrumTapConstants.HeaderBlockLength && data[0] == SpectrumTapConstants.HeaderFlag)
                    .ToString(System.Globalization.CultureInfo.InvariantCulture)
            });

    private static bool Close(double actual, double expected) =>
        Math.Abs(actual - expected) <= expected * SpectrumTapConstants.PulseToleranceRatio;

    private static byte Xor(ReadOnlySpan<byte> data)
    {
        byte result = 0;
        foreach (var value in data) result ^= value;
        return result;
    }

    private static int ReadUInt24(ReadOnlySpan<byte> value) => value[0] | value[1] << 8 | value[2] << 16;
}
