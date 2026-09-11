using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Encoding;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Encoding.Sequential.Msx;

/// <summary>Writes validated MSX byte groups as CAS data or reconstructs their standard FSK waveform.</summary>
public sealed class MsxTapeEncoder : ISequentialMediaEncoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.MsxCas, TapeImageFormatIds.Wav }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Msx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => MsxCasConstants.EncoderId;
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
            throw new InvalidDataException("MSX tape encoding requires validated byte groups without retained undecoded segments.");
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(request.TargetFormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase)
            ? EncodeWave(request.Blocks, request.Parameters)
            : EncodeCas(request.Blocks));
    }

    private static SequentialMediaImageRepresentation EncodeCas(IReadOnlyList<SequentialDecodedBlock> blocks)
    {
        var segments = new List<SequentialMediaSegment>(blocks.Count * 2);
        long position = 0;
        foreach (var block in blocks)
        {
            segments.Add(new SequentialMediaSegment(
                position++,
                SequentialSegmentKind.TapeMark,
                MsxCasConstants.SeparatorLength,
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [MsxCasConstants.SeparatorMetadataKey] = bool.TrueString,
                    [MsxCasConstants.ReconstructionMetadataKey] = bool.TrueString
                }));
            var bytes = block.Data.ToArray();
            var source = new MemoryRandomAccessData(bytes);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [MsxCasConstants.TimingMetadataKey] = "not-stored",
                [MsxCasConstants.ReconstructionMetadataKey] = bool.TrueString
            };
            if (block.Metadata.TryGetValue(MsxCasConstants.FileTypeMetadataKey, out var fileType))
                metadata[MsxCasConstants.FileTypeMetadataKey] = fileType;
            segments.Add(new SequentialMediaSegment(
                position++,
                SequentialSegmentKind.DataBlock,
                bytes.Length,
                direction: SequentialTravelDirection.Forward,
                dataRange: new MediaDataRange(0, bytes.Length, MediaDataRangeKind.Stored, source, 0),
                metadata: metadata));
        }
        var logicalLength = segments.Sum(segment => segment.Length ?? 0);
        return new SequentialMediaImageRepresentation(logicalLength, segments: segments);
    }

    private static SequentialMediaImageRepresentation EncodeWave(
        IReadOnlyList<SequentialDecodedBlock> blocks,
        IReadOnlyDictionary<string, string> parameters)
    {
        TryReadSampleRate(parameters, out var sampleRate);
        var samples = new List<short>();
        var level = MsxCasConstants.PcmAmplitude;
        for (var blockIndex = 0; blockIndex < blocks.Count; blockIndex++)
        {
            var longHeader = blockIndex == 0
                || blocks[blockIndex].Metadata.ContainsKey(MsxCasConstants.FileTypeMetadataKey);
            PcmSampleFunctions.AppendSilence(
                samples,
                sampleRate,
                longHeader ? MsxCasConstants.LongSilenceSeconds : MsxCasConstants.ShortSilenceSeconds);
            FskSerialFunctions.AppendPcmCarrier(
                samples,
                ref level,
                sampleRate,
                MsxCasConstants.StandardBaudRate,
                MsxCasConstants.ZeroFrequency,
                MsxCasConstants.OneFrequency,
                longHeader ? MsxCasConstants.LongCarrierSeconds : MsxCasConstants.ShortCarrierSeconds);
            foreach (var value in blocks[blockIndex].Data.Span)
                FskSerialFunctions.AppendPcmFrame(
                    samples,
                    ref level,
                    sampleRate,
                    MsxCasConstants.StandardBaudRate,
                    MsxCasConstants.ZeroFrequency,
                    MsxCasConstants.OneFrequency,
                    MsxCasConstants.DataBitsPerFrame,
                    MsxCasConstants.StopBitsPerFrame,
                    value);
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
                ["bitsPerSample"] = MsxCasConstants.PcmBitsPerSample.ToString(CultureInfo.InvariantCulture),
                [MsxCasConstants.ReconstructionMetadataKey] = bool.TrueString
            });
        return new SequentialMediaImageRepresentation(pcm.Length, duration, [segment]);
    }

    private static bool TryReadSampleRate(IReadOnlyDictionary<string, string> parameters, out int sampleRate)
    {
        sampleRate = 0;
        return parameters.TryGetValue("sampleRate", out var text)
            && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out sampleRate)
            && sampleRate >= 8000;
    }
}
