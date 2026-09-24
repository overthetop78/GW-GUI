using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Encoding;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Commodore;

/// <summary>Encodes validated Commodore bytes into standard ROM pulse frames for TAP or PCM WAV output.</summary>
public sealed class CommodoreTapeEncoder : ISequentialMediaEncoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.CommodoreTap, TapeImageFormatIds.Wav }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Commodore }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => CommodoreTapConstants.EncoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanEncode(SequentialEncodeRequest request) =>
        SupportedFormats.Contains(request.TargetFormatId)
        && SupportedMachines.Contains(request.MachineId)
        && request.Blocks.Count > 0
        && request.Blocks.All(block => block.IntegrityValid == true && block.Data.Length > 0)
        && request.RetainedSegments.Count == 0
        && (!request.TargetFormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase)
            || TryReadSampleRate(request.Parameters, out _));

    public Task<SequentialMediaImageRepresentation> EncodeAsync(
        SequentialEncodeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanEncode(request))
            throw new InvalidDataException("Commodore tape encoding requires validated blocks without retained undecoded segments.");
        cancellationToken.ThrowIfCancellationRequested();
        var cycles = EncodePulseCycles(request.Blocks);
        return Task.FromResult(request.TargetFormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase)
            ? EncodeWave(cycles, request.Parameters)
            : EncodeTap(cycles));
    }

    private static IReadOnlyList<int> EncodePulseCycles(IReadOnlyList<SequentialDecodedBlock> blocks)
    {
        var pulses = new List<int>();
        foreach (var block in blocks)
        {
            foreach (var value in block.Data.Span)
            {
                pulses.Add(CommodoreTapConstants.LongPulseCycles);
                pulses.Add(CommodoreTapConstants.MediumPulseCycles);
                var ones = 0;
                for (var bit = 0; bit < CommodoreTapConstants.DataBitsPerByte; bit++)
                {
                    var set = (value & 1 << bit) != 0;
                    AddBit(pulses, set);
                    if (set) ones++;
                }
                AddBit(pulses, (ones & 1) == 0);
            }
        }
        return pulses;
    }

    private static SequentialMediaImageRepresentation EncodeTap(IReadOnlyList<int> cycles)
    {
        var segments = new SequentialMediaSegment[cycles.Count];
        var elapsed = TimeSpan.Zero;
        for (var index = 0; index < cycles.Count; index++)
        {
            var duration = TimeSpan.FromSeconds(cycles[index] / (double)CommodoreTapConstants.DefaultC64PalClockRate);
            segments[index] = new SequentialMediaSegment(
                index,
                SequentialSegmentKind.Pulse,
                cycles[index],
                elapsed,
                duration,
                direction: SequentialTravelDirection.Forward,
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CommodoreTapConstants.PulseMetadataKey] = bool.TrueString,
                    [CommodoreTapConstants.CycleCountMetadataKey] = cycles[index].ToString(CultureInfo.InvariantCulture),
                    [CommodoreTapConstants.HalfWaveMetadataKey] = bool.FalseString
                });
            elapsed += duration;
        }
        return new SequentialMediaImageRepresentation(cycles.Count, elapsed, segments);
    }

    private static SequentialMediaImageRepresentation EncodeWave(
        IReadOnlyList<int> cycles,
        IReadOnlyDictionary<string, string> parameters)
    {
        TryReadSampleRate(parameters, out var sampleRate);
        var samples = new List<short>();
        var level = CommodoreTapConstants.PcmAmplitude;
        foreach (var pulseCycles in cycles)
        {
            var sampleCount = Math.Max(1, (int)Math.Round(
                pulseCycles * (double)sampleRate / CommodoreTapConstants.DefaultC64PalClockRate));
            for (var sample = 0; sample < sampleCount; sample++) samples.Add(level);
            level = (short)-level;
        }

        var pcm = new byte[samples.Count * sizeof(short)];
        for (var index = 0; index < samples.Count; index++)
            BinaryPrimitives.WriteInt16LittleEndian(pcm.AsSpan(index * sizeof(short)), samples[index]);
        var source = new MemoryRandomAccessData(pcm);
        var duration = TimeSpan.FromSeconds(samples.Count / (double)sampleRate);
        var segment = new SequentialMediaSegment(
            0,
            SequentialSegmentKind.Samples,
            samples.Count,
            TimeSpan.Zero,
            duration,
            channelNumber: 0,
            direction: SequentialTravelDirection.Forward,
            dataRange: new MediaDataRange(0, pcm.Length, MediaDataRangeKind.Stored, source, 0),
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["channel"] = "0",
                ["interleaved"] = bool.TrueString,
                ["blockAlign"] = sizeof(short).ToString(CultureInfo.InvariantCulture),
                ["sampleRate"] = sampleRate.ToString(CultureInfo.InvariantCulture),
                ["bitsPerSample"] = CommodoreTapConstants.PcmBitsPerSample.ToString(CultureInfo.InvariantCulture)
            });
        return new SequentialMediaImageRepresentation(pcm.Length, duration, [segment]);
    }

    private static void AddBit(List<int> pulses, bool value)
    {
        pulses.Add(value ? CommodoreTapConstants.MediumPulseCycles : CommodoreTapConstants.ShortPulseCycles);
        pulses.Add(value ? CommodoreTapConstants.ShortPulseCycles : CommodoreTapConstants.MediumPulseCycles);
    }

    private static bool TryReadSampleRate(IReadOnlyDictionary<string, string> parameters, out int sampleRate)
    {
        sampleRate = 0;
        return parameters.TryGetValue("sampleRate", out var text)
            && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out sampleRate)
            && sampleRate >= 8000;
    }
}
