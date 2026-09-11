using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Encoding;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;
using GWGUI.MediaEngine.Encoding.Sequential.Tzx;

namespace GWGUI.MediaEngine.Encoding.Sequential.Spectrum;

/// <summary>Encodes validated Spectrum blocks as TAP, standard TZX blocks, or explicit 16-bit mono PCM samples.</summary>
public sealed class SpectrumTapeEncoder : ISequentialMediaEncoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.SpectrumTap, TapeImageFormatIds.Tzx, TapeImageFormatIds.Wav }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Spectrum }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => SpectrumTapConstants.EncoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanEncode(SequentialEncodeRequest request) =>
        SupportedFormats.Contains(request.TargetFormatId)
        && SupportedMachines.Contains(request.MachineId)
        && request.Blocks.Count > 0
        && request.Blocks.All(block => block.IntegrityValid == true && block.Data.Length is >= 2 and <= ushort.MaxValue)
        && request.RetainedSegments.Count == 0
        && (!request.TargetFormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase)
            || TryReadSampleRate(request.Parameters, out _));

    public Task<SequentialMediaImageRepresentation> EncodeAsync(
        SequentialEncodeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanEncode(request))
            throw new InvalidDataException("Spectrum encoding requires checksum-valid blocks and explicit WAV sample rate when WAV is selected.");
        cancellationToken.ThrowIfCancellationRequested();
        if (request.TargetFormatId.Equals(TapeImageFormatIds.Tzx, StringComparison.OrdinalIgnoreCase))
            return new TzxSignalEncoder().EncodeAsync(request, cancellationToken);
        if (request.TargetFormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(EncodeWave(request));

        var segments = request.Blocks.Select((block, index) => CreateTapBlock(index, block.Data)).ToArray();
        return Task.FromResult(new SequentialMediaImageRepresentation(segments.Sum(segment => segment.Length ?? 0), segments: segments));
    }

    private static SequentialMediaImageRepresentation EncodeWave(SequentialEncodeRequest request)
    {
        TryReadSampleRate(request.Parameters, out var sampleRate);
        var samples = new List<short>();
        var level = SpectrumTapConstants.PcmAmplitude;
        foreach (var block in request.Blocks)
        {
            var data = block.Data.Span;
            var pilotCount = data[0] == SpectrumTapConstants.HeaderFlag
                ? SpectrumTapConstants.HeaderPilotPulseCount
                : SpectrumTapConstants.DataPilotPulseCount;
            AddPulses(samples, ref level, SpectrumTapConstants.PilotPulseTStates, pilotCount, sampleRate);
            AddPulses(samples, ref level, SpectrumTapConstants.SyncFirstPulseTStates, 1, sampleRate);
            AddPulses(samples, ref level, SpectrumTapConstants.SyncSecondPulseTStates, 1, sampleRate);
            foreach (var value in data)
            {
                for (var bit = 7; bit >= 0; bit--)
                {
                    var pulse = (value & (1 << bit)) == 0 ? SpectrumTapConstants.ZeroPulseTStates : SpectrumTapConstants.OnePulseTStates;
                    AddPulses(samples, ref level, pulse, 2, sampleRate);
                }
            }
            for (var index = 0; index < sampleRate; index++) samples.Add(0);
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
                ["blockAlign"] = sizeof(short).ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sampleRate"] = sampleRate.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["bitsPerSample"] = SpectrumTapConstants.PcmBitsPerSample.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
        return new SequentialMediaImageRepresentation(pcm.Length, duration, [segment]);
    }

    private static SequentialMediaSegment CreateTapBlock(long position, ReadOnlyMemory<byte> data)
    {
        var bytes = data.ToArray();
        var source = new MemoryRandomAccessData(bytes);
        return new SequentialMediaSegment(
            position,
            SequentialSegmentKind.DataBlock,
            bytes.Length,
            dataRange: new MediaDataRange(0, bytes.Length, MediaDataRangeKind.Stored, source, 0),
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [SpectrumTapConstants.BlockMetadataKey] = bool.TrueString,
                ["flag"] = bytes[0].ToString("x2", System.Globalization.CultureInfo.InvariantCulture),
                ["checksumValid"] = bool.TrueString,
                ["header"] = (bytes.Length == SpectrumTapConstants.HeaderBlockLength && bytes[0] == SpectrumTapConstants.HeaderFlag)
                    .ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static void AddPulses(List<short> samples, ref short level, ushort tStates, int count, int sampleRate)
    {
        var sampleCount = Math.Max(1, (int)Math.Round(tStates * sampleRate / (double)TzxConstants.TStatesPerSecond));
        for (var pulse = 0; pulse < count; pulse++)
        {
            for (var sample = 0; sample < sampleCount; sample++) samples.Add(level);
            level = (short)-level;
        }
    }

    private static bool TryReadSampleRate(IReadOnlyDictionary<string, string> parameters, out int sampleRate)
    {
        sampleRate = 0;
        return parameters.TryGetValue("sampleRate", out var text)
            && int.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out sampleRate)
            && sampleRate >= 8000;
    }
}
