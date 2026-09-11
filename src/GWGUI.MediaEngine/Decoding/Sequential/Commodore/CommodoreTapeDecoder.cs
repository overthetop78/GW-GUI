using System.Collections.Frozen;
using System.Globalization;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Decoding;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Decoding.Sequential.Commodore;

/// <summary>Recognizes standard Commodore ROM byte frames in TAP pulses or a selected PCM WAV channel.</summary>
public sealed class CommodoreTapeDecoder : ISequentialMediaDecoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.CommodoreTap, TapeImageFormatIds.Wav }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Commodore }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => CommodoreTapConstants.DecoderId;
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
            throw new InvalidDataException("The media document is not compatible with the Commodore tape decoder.");
        var representation = (SequentialMediaImageRepresentation)document.Representation;
        if (document.FormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase))
            return await DecodeWaveAsync(document, representation, cancellationToken).ConfigureAwait(false);
        return DecodeTap(document, representation, cancellationToken);
    }

    private static SequentialDecodeResult DecodeTap(
        MediaImageDocument document,
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        var clockRate = TryReadInt(document.Metadata, CommodoreTapConstants.ClockRateMetadataKey, out var storedClock)
            ? storedClock
            : CommodoreTapConstants.DefaultC64PalClockRate;
        var pulses = new List<Pulse>();
        foreach (var segment in (representation.Segments ?? []).OrderBy(segment => segment.Position))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment.Kind != SequentialSegmentKind.Pulse) continue;
            int cycles;
            if (!TryReadInt(segment.Metadata, CommodoreTapConstants.CycleCountMetadataKey, out cycles))
            {
                if (segment.Duration is not { } duration) continue;
                cycles = checked((int)Math.Round(duration.TotalSeconds * clockRate));
            }
            pulses.Add(new Pulse(cycles, segment.Position));
        }
        return CreateResult(representation, pulses, "tap-pulses");
    }

    private static async Task<SequentialDecodeResult> DecodeWaveAsync(
        MediaImageDocument document,
        SequentialMediaImageRepresentation representation,
        CancellationToken cancellationToken)
    {
        if (!PcmSampleFunctions.TryGetProfile(document.Metadata, out var channels, out var sampleRate, out var bitsPerSample, out var blockAlign))
            return new SequentialDecodeResult(
                CommodoreTapConstants.DecoderId,
                0,
                [],
                representation.Segments ?? [],
                ["The WAV PCM profile is incomplete or incompatible."]);

        var pulses = new List<Pulse>();
        foreach (var segment in (representation.Segments ?? [])
                     .Where(segment => segment.Kind == SequentialSegmentKind.Samples && segment.ChannelNumber == 0)
                     .OrderBy(segment => segment.Position))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment.DataRange is not { Source: not null, Length: <= int.MaxValue } range) continue;
            var pcm = new byte[checked((int)range.Length)];
            await range.Source.ReadExactlyAsync(range.SourceOffset, pcm, cancellationToken).ConfigureAwait(false);
            var samples = PcmSampleFunctions.ExtractChannel(pcm, channels, bitsPerSample, blockAlign, 0);
            AddWavePulses(samples, sampleRate, segment.Position, pulses);
        }
        return CreateResult(representation, pulses, "wav-pulses");
    }

    private static SequentialDecodeResult CreateResult(
        SequentialMediaImageRepresentation representation,
        IReadOnlyList<Pulse> pulses,
        string sourceKind)
    {
        var blocks = new List<SequentialDecodedBlock>();
        var consumed = new HashSet<long>();
        for (var scan = 0; scan + 1 < pulses.Count;)
        {
            if (!IsPulse(pulses[scan].Cycles, CommodoreTapConstants.LongPulseCycles)
                || !IsPulse(pulses[scan + 1].Cycles, CommodoreTapConstants.MediumPulseCycles))
            {
                scan++;
                continue;
            }

            var blockStart = scan;
            var bytes = new List<byte>();
            var parityValid = true;
            while (TryDecodeByte(pulses, ref scan, out var value, out var validParity))
            {
                bytes.Add(value);
                parityValid &= validParity;
            }
            if (bytes.Count == 0)
            {
                scan = blockStart + 1;
                continue;
            }

            var sourcePositions = pulses.Skip(blockStart).Take(scan - blockStart)
                .Select(pulse => pulse.SourcePosition).Distinct().ToArray();
            foreach (var position in sourcePositions) consumed.Add(position);
            blocks.Add(new SequentialDecodedBlock(
                blocks.Count,
                bytes.ToArray(),
                sourcePositions,
                parityValid,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CommodoreTapConstants.SourceKindMetadataKey] = sourceKind,
                    [CommodoreTapConstants.ParityValidMetadataKey] = parityValid.ToString(CultureInfo.InvariantCulture),
                    [CommodoreTapConstants.ChecksumPresentMetadataKey] = bool.FalseString
                }));
        }

        var undecoded = (representation.Segments ?? [])
            .Where(segment => !consumed.Contains(segment.Position))
            .ToArray();
        var confidence = blocks.Count == 0 ? 0 : blocks.Count(block => block.IntegrityValid == true) / (double)blocks.Count;
        return new SequentialDecodeResult(
            CommodoreTapConstants.DecoderId,
            confidence,
            blocks,
            undecoded,
            blocks.Count == 0 ? ["No standard Commodore ROM byte frame was recognized."] : []);
    }

    private static bool TryDecodeByte(IReadOnlyList<Pulse> pulses, ref int offset, out byte value, out bool parityValid)
    {
        value = 0;
        parityValid = false;
        var required = 2 + (CommodoreTapConstants.DataBitsPerByte + CommodoreTapConstants.ParityBitsPerByte)
            * CommodoreTapConstants.PulsesPerBit;
        if (offset + required > pulses.Count
            || !IsPulse(pulses[offset].Cycles, CommodoreTapConstants.LongPulseCycles)
            || !IsPulse(pulses[offset + 1].Cycles, CommodoreTapConstants.MediumPulseCycles))
            return false;

        var cursor = offset + 2;
        var ones = 0;
        for (var bit = 0; bit < CommodoreTapConstants.DataBitsPerByte; bit++)
        {
            if (!TryDecodeBit(pulses[cursor].Cycles, pulses[cursor + 1].Cycles, out var set)) return false;
            if (set)
            {
                value |= (byte)(1 << bit);
                ones++;
            }
            cursor += CommodoreTapConstants.PulsesPerBit;
        }
        if (!TryDecodeBit(pulses[cursor].Cycles, pulses[cursor + 1].Cycles, out var parity)) return false;
        parityValid = ((ones + (parity ? 1 : 0)) & 1) == 1;
        offset = cursor + CommodoreTapConstants.PulsesPerBit;
        return true;
    }

    private static bool TryDecodeBit(int firstCycles, int secondCycles, out bool value)
    {
        value = false;
        if (IsPulse(firstCycles, CommodoreTapConstants.ShortPulseCycles)
            && IsPulse(secondCycles, CommodoreTapConstants.MediumPulseCycles))
            return true;
        if (IsPulse(firstCycles, CommodoreTapConstants.MediumPulseCycles)
            && IsPulse(secondCycles, CommodoreTapConstants.ShortPulseCycles))
        {
            value = true;
            return true;
        }
        return false;
    }

    private static void AddWavePulses(IReadOnlyList<int> samples, int sampleRate, long sourcePosition, List<Pulse> pulses)
    {
        if (samples.Count == 0) return;
        var previousSign = samples[0] >= 0;
        var previousCrossing = 0;
        for (var index = 1; index < samples.Count; index++)
        {
            var sign = samples[index] >= 0;
            if (sign == previousSign) continue;
            var cycles = checked((int)Math.Round(
                (index - previousCrossing) * (double)CommodoreTapConstants.DefaultC64PalClockRate / sampleRate));
            if (cycles > 0) pulses.Add(new Pulse(cycles, sourcePosition));
            previousCrossing = index;
            previousSign = sign;
        }
    }

    private static bool IsPulse(int actual, int expected) =>
        Math.Abs(actual - expected) <= expected * CommodoreTapConstants.PulseToleranceRatio;

    private static bool TryReadInt(IReadOnlyDictionary<string, string> metadata, string key, out int value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value)
            && value > 0;
    }

    private readonly record struct Pulse(int Cycles, long SourcePosition);
}
