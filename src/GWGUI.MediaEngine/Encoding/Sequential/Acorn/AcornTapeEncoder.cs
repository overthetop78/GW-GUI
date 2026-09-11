using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Encoding;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Encoding.Sequential.Acorn;

/// <summary>Rebuilds validated Acorn cassette blocks as UEF implicit data chunks or standard PCM FSK.</summary>
public sealed class AcornTapeEncoder : ISequentialMediaEncoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.Uef, TapeImageFormatIds.Wav }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.AcornBbc }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => UefConstants.EncoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanEncode(SequentialEncodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SupportedFormats.Contains(request.TargetFormatId)
            && SupportedMachines.Contains(request.MachineId)
            && request.Blocks.Count > 0
            && request.RetainedSegments.Count == 0
            && request.Blocks.All(block => block.IntegrityValid == true && TryBuildBlock(block, out _))
            && (!request.TargetFormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase)
                || TryReadSampleRate(request.Parameters, out _));
    }

    public Task<SequentialMediaImageRepresentation> EncodeAsync(
        SequentialEncodeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanEncode(request))
            throw new InvalidDataException("Acorn tape encoding requires validated CFS blocks with complete cassette metadata.");
        cancellationToken.ThrowIfCancellationRequested();
        var encodedBlocks = request.Blocks.Select(block =>
        {
            TryBuildBlock(block, out var bytes);
            return bytes;
        }).ToArray();
        return Task.FromResult(request.TargetFormatId.Equals(TapeImageFormatIds.Wav, StringComparison.OrdinalIgnoreCase)
            ? EncodeWave(encodedBlocks, request.Parameters)
            : EncodeUef(encodedBlocks));
    }

    private static SequentialMediaImageRepresentation EncodeUef(IReadOnlyList<byte[]> blocks)
    {
        var segments = new List<SequentialMediaSegment>(blocks.Count);
        var elapsed = TimeSpan.Zero;
        for (var index = 0; index < blocks.Count; index++)
        {
            var bytes = blocks[index];
            var duration = TimeSpan.FromSeconds(
                bytes.Length * (1 + UefConstants.AcornDataBitsPerFrame + UefConstants.AcornStopBitsPerFrame)
                / (double)UefConstants.DefaultBaudRate);
            var source = new MemoryRandomAccessData(bytes);
            segments.Add(new SequentialMediaSegment(
                index,
                SequentialSegmentKind.DataBlock,
                bytes.Length,
                elapsed,
                duration,
                direction: SequentialTravelDirection.Forward,
                dataRange: new MediaDataRange(0, bytes.Length, MediaDataRangeKind.Stored, source, 0),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [UefConstants.ChunkIdMetadataKey] = UefConstants.ImplicitData.ToString("x4", CultureInfo.InvariantCulture),
                    [UefConstants.ReconstructionMetadataKey] = bool.TrueString
                }));
            elapsed += duration;
        }
        return new SequentialMediaImageRepresentation(blocks.Sum(block => (long)block.Length), elapsed, segments);
    }

    private static SequentialMediaImageRepresentation EncodeWave(
        IReadOnlyList<byte[]> blocks,
        IReadOnlyDictionary<string, string> parameters)
    {
        TryReadSampleRate(parameters, out var sampleRate);
        var samples = new List<short>();
        var level = UefConstants.PcmAmplitude;
        for (var index = 0; index < blocks.Count; index++)
        {
            if (index > 0) PcmSampleFunctions.AppendSilence(samples, sampleRate, UefConstants.InterBlockGapSeconds);
            FskSerialFunctions.AppendPcmCarrier(
                samples,
                ref level,
                sampleRate,
                UefConstants.DefaultBaudRate,
                UefConstants.AcornZeroFrequency,
                UefConstants.AcornOneFrequency,
                index == 0 ? UefConstants.InitialCarrierSeconds : UefConstants.InterBlockCarrierSeconds);
            foreach (var value in blocks[index])
                FskSerialFunctions.AppendPcmFrame(
                    samples,
                    ref level,
                    sampleRate,
                    UefConstants.DefaultBaudRate,
                    UefConstants.AcornZeroFrequency,
                    UefConstants.AcornOneFrequency,
                    UefConstants.AcornDataBitsPerFrame,
                    UefConstants.AcornStopBitsPerFrame,
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
                ["bitsPerSample"] = UefConstants.PcmBitsPerSample.ToString(CultureInfo.InvariantCulture),
                [UefConstants.ReconstructionMetadataKey] = bool.TrueString
            });
        return new SequentialMediaImageRepresentation(pcm.Length, duration, [segment]);
    }

    private static bool TryBuildBlock(SequentialDecodedBlock block, out byte[] bytes)
    {
        bytes = [];
        if (block.Data.Length > UefConstants.AcornMaximumDataLength
            || !block.Metadata.TryGetValue(UefConstants.FileNameMetadataKey, out var fileName)
            || string.IsNullOrEmpty(fileName)
            || fileName.Length > UefConstants.AcornMaximumFileNameLength
            || fileName.Any(character => character > 0x7f)
            || !TryReadHexUInt32(block.Metadata, UefConstants.LoadAddressMetadataKey, out var loadAddress)
            || !TryReadHexUInt32(block.Metadata, UefConstants.ExecutionAddressMetadataKey, out var executionAddress)
            || !TryReadUInt16(block.Metadata, UefConstants.BlockNumberMetadataKey, out var blockNumber)
            || !TryReadHexByte(block.Metadata, UefConstants.FlagsMetadataKey, out var flags)
            || !TryReadHexUInt32(block.Metadata, UefConstants.NextAddressMetadataKey, out var nextAddress))
            return false;

        var name = System.Text.Encoding.ASCII.GetBytes(fileName);
        var headerLength = name.Length + 1
            + UefConstants.AcornLoadAddressLength
            + UefConstants.AcornExecutionAddressLength
            + UefConstants.AcornBlockNumberLength
            + UefConstants.AcornDataLengthFieldLength
            + UefConstants.AcornFlagsLength
            + UefConstants.AcornNextAddressLength;
        bytes = new byte[1 + headerLength + UefConstants.AcornChecksumLength
            + block.Data.Length + UefConstants.AcornChecksumLength];
        bytes[0] = UefConstants.AcornBlockSync;
        name.CopyTo(bytes, 1);
        var cursor = 1 + name.Length + 1;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(cursor), loadAddress);
        cursor += UefConstants.AcornLoadAddressLength;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(cursor), executionAddress);
        cursor += UefConstants.AcornExecutionAddressLength;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(cursor), blockNumber);
        cursor += UefConstants.AcornBlockNumberLength;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(cursor), checked((ushort)block.Data.Length));
        cursor += UefConstants.AcornDataLengthFieldLength;
        bytes[cursor++] = flags;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(cursor), nextAddress);
        cursor += UefConstants.AcornNextAddressLength;
        var headerChecksum = Crc16Calculator.Compute(
            bytes.AsSpan(1, headerLength).ToArray(),
            UefConstants.AcornChecksumPolynomial,
            UefConstants.AcornChecksumInitialValue);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(cursor), headerChecksum);
        cursor += UefConstants.AcornChecksumLength;
        block.Data.Span.CopyTo(bytes.AsSpan(cursor));
        var dataChecksum = Crc16Calculator.Compute(
            block.Data.ToArray(),
            UefConstants.AcornChecksumPolynomial,
            UefConstants.AcornChecksumInitialValue);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(cursor + block.Data.Length), dataChecksum);
        return true;
    }

    private static bool TryReadSampleRate(IReadOnlyDictionary<string, string> parameters, out int sampleRate)
    {
        sampleRate = 0;
        return parameters.TryGetValue("sampleRate", out var text)
            && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out sampleRate)
            && sampleRate >= 8000;
    }

    private static bool TryReadHexUInt32(IReadOnlyDictionary<string, string> metadata, string key, out uint value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && uint.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadHexByte(IReadOnlyDictionary<string, string> metadata, string key, out byte value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && byte.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryReadUInt16(IReadOnlyDictionary<string, string> metadata, string key, out ushort value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && ushort.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}
