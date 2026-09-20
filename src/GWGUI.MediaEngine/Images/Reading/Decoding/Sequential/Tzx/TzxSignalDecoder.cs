using System.Buffers.Binary;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Tzx;

/// <summary>Builds a temporal signal timeline from supported TZX blocks without interpreting machine files.</summary>
public sealed class TzxSignalDecoder
{
    public async Task<SequentialSignalDecodeResult> DecodeAsync(
        MediaImageDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!document.FormatId.Equals(TapeImageFormatIds.Tzx, StringComparison.OrdinalIgnoreCase)
            || document.Representation is not SequentialMediaImageRepresentation { Segments: { } sourceSegments })
            throw new InvalidDataException("The document does not contain retained TZX blocks.");

        var timeline = new List<SequentialMediaSegment>();
        var diagnostics = new List<string>();
        var elapsed = TimeSpan.Zero;
        long position = 0;
        foreach (var sourceSegment in sourceSegments.OrderBy(segment => segment.Position))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!TryBlockId(sourceSegment, out var id))
            {
                diagnostics.Add($"Source segment {sourceSegment.Position} has no TZX block identifier.");
                continue;
            }
            var payload = await ReadPayloadAsync(sourceSegment, cancellationToken).ConfigureAwait(false);
            switch (id)
            {
                case TzxConstants.StandardSpeedData:
                    DecodeStandard(payload, sourceSegment, timeline, ref position, ref elapsed);
                    break;
                case TzxConstants.TurboSpeedData:
                    DecodeTurbo(payload, sourceSegment, timeline, ref position, ref elapsed);
                    break;
                case TzxConstants.PureTone:
                    AddRepeatedPulse(payload, sourceSegment, timeline, ref position, ref elapsed);
                    break;
                case TzxConstants.PulseSequence:
                    AddPulseSequence(payload, sourceSegment, timeline, ref position, ref elapsed);
                    break;
                case TzxConstants.PureData:
                    DecodePureData(payload, sourceSegment, timeline, ref position, ref elapsed);
                    break;
                case TzxConstants.DirectRecording:
                    DecodeDirectRecording(payload, sourceSegment, timeline, ref position, ref elapsed);
                    break;
                case TzxConstants.Pause:
                    AddPause(payload, sourceSegment, timeline, ref position, ref elapsed);
                    break;
                default:
                    diagnostics.Add($"TZX block 0x{id:x2} at position {sourceSegment.Position} was retained but has no signal expansion.");
                    break;
            }
        }

        var logicalLength = timeline.Sum(segment => segment.Length ?? 0);
        return new SequentialSignalDecodeResult(
            new SequentialMediaImageRepresentation(logicalLength, elapsed == TimeSpan.Zero ? null : elapsed, timeline),
            sourceSegments,
            diagnostics);
    }

    private static void DecodeStandard(
        byte[] payload,
        SequentialMediaSegment source,
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed)
    {
        Require(payload, 4, TzxConstants.StandardSpeedData);
        var pause = BinaryPrimitives.ReadUInt16LittleEndian(payload);
        var dataLength = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(2));
        Require(payload, 4 + dataLength, TzxConstants.StandardSpeedData);
        var pilotCount = dataLength > 0 && payload[4] < 0x80
            ? TzxConstants.StandardHeaderPilotPulseCount
            : TzxConstants.StandardDataPilotPulseCount;
        AddPulse(output, ref position, ref elapsed, TzxConstants.StandardPilotPulseTStates, pilotCount, source.Position, "pilot");
        AddPulse(output, ref position, ref elapsed, TzxConstants.StandardSyncFirstPulseTStates, 1, source.Position, "sync");
        AddPulse(output, ref position, ref elapsed, TzxConstants.StandardSyncSecondPulseTStates, 1, source.Position, "sync");
        AddData(output, ref position, ref elapsed, source, payload, 4, dataLength, TzxConstants.FullLastByteBitCount,
            TzxConstants.StandardZeroPulseTStates, TzxConstants.StandardOnePulseTStates);
        AddPause(output, ref position, ref elapsed, pause, source.Position);
    }

    private static void DecodeTurbo(
        byte[] payload,
        SequentialMediaSegment source,
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed)
    {
        Require(payload, 18, TzxConstants.TurboSpeedData);
        var pilotPulse = BinaryPrimitives.ReadUInt16LittleEndian(payload);
        var sync1 = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(2));
        var sync2 = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(4));
        var zero = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(6));
        var one = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(8));
        var pilotCount = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(10));
        var usedBits = payload[12];
        var pause = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(13));
        var dataLength = ReadUInt24(payload.AsSpan(15));
        Require(payload, checked(18 + dataLength), TzxConstants.TurboSpeedData);
        AddPulse(output, ref position, ref elapsed, pilotPulse, pilotCount, source.Position, "pilot");
        AddPulse(output, ref position, ref elapsed, sync1, 1, source.Position, "sync");
        AddPulse(output, ref position, ref elapsed, sync2, 1, source.Position, "sync");
        AddData(output, ref position, ref elapsed, source, payload, 18, dataLength, usedBits, zero, one);
        AddPause(output, ref position, ref elapsed, pause, source.Position);
    }

    private static void AddRepeatedPulse(
        byte[] payload,
        SequentialMediaSegment source,
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed)
    {
        Require(payload, 4, TzxConstants.PureTone);
        AddPulse(output, ref position, ref elapsed,
            BinaryPrimitives.ReadUInt16LittleEndian(payload),
            BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(2)),
            source.Position,
            "tone");
    }

    private static void AddPulseSequence(
        byte[] payload,
        SequentialMediaSegment source,
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed)
    {
        Require(payload, 1, TzxConstants.PulseSequence);
        Require(payload, 1 + payload[0] * 2, TzxConstants.PulseSequence);
        for (var index = 0; index < payload[0]; index++)
            AddPulse(output, ref position, ref elapsed,
                BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(1 + index * 2)), 1, source.Position, "sequence");
    }

    private static void DecodePureData(
        byte[] payload,
        SequentialMediaSegment source,
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed)
    {
        Require(payload, 10, TzxConstants.PureData);
        var zero = BinaryPrimitives.ReadUInt16LittleEndian(payload);
        var one = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(2));
        var usedBits = payload[4];
        var pause = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(5));
        var dataLength = ReadUInt24(payload.AsSpan(7));
        Require(payload, checked(10 + dataLength), TzxConstants.PureData);
        AddData(output, ref position, ref elapsed, source, payload, 10, dataLength, usedBits, zero, one);
        AddPause(output, ref position, ref elapsed, pause, source.Position);
    }

    private static void DecodeDirectRecording(
        byte[] payload,
        SequentialMediaSegment source,
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed)
    {
        Require(payload, 8, TzxConstants.DirectRecording);
        var sampleTStates = BinaryPrimitives.ReadUInt16LittleEndian(payload);
        var pause = BinaryPrimitives.ReadUInt16LittleEndian(payload.AsSpan(2));
        var usedBits = payload[4];
        var dataLength = ReadUInt24(payload.AsSpan(5));
        Require(payload, checked(8 + dataLength), TzxConstants.DirectRecording);
        if (dataLength == 0)
        {
            AddPause(output, ref position, ref elapsed, pause, source.Position);
            return;
        }
        if (usedBits is 0 or > TzxConstants.FullLastByteBitCount)
            throw new InvalidDataException("A TZX direct-recording block declares an invalid last-byte bit count.");
        var bitCount = DataBitCount(dataLength, usedBits);
        var duration = TStatesDuration((long)sampleTStates * bitCount);
        output.Add(new SequentialMediaSegment(
            position++, SequentialSegmentKind.Samples, bitCount, elapsed, duration,
            direction: SequentialTravelDirection.Forward,
            dataRange: Slice(source, 8, dataLength),
            metadata: SourceMetadata(source.Position, "direct-recording")));
        elapsed += duration;
        AddPause(output, ref position, ref elapsed, pause, source.Position);
    }

    private static void AddData(
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed,
        SequentialMediaSegment source,
        byte[] payload,
        int dataOffset,
        int dataLength,
        byte usedBits,
        ushort zeroPulse,
        ushort onePulse)
    {
        if (dataLength == 0) return;
        if (usedBits is 0 or > TzxConstants.FullLastByteBitCount)
            throw new InvalidDataException("A TZX data block declares an invalid last-byte bit count.");
        var bitCount = DataBitCount(dataLength, usedBits);
        var data = payload.AsSpan(dataOffset, dataLength);
        long pulseTStates = 0;
        for (var bitIndex = 0; bitIndex < bitCount; bitIndex++)
        {
            var value = (data[bitIndex / 8] & (0x80 >> (bitIndex & 7))) != 0;
            pulseTStates += 2L * (value ? onePulse : zeroPulse);
        }
        var duration = TStatesDuration(pulseTStates);
        output.Add(new SequentialMediaSegment(
            position++, SequentialSegmentKind.DataBlock, dataLength, elapsed, duration,
            direction: SequentialTravelDirection.Forward,
            dataRange: Slice(source, dataOffset, dataLength),
            metadata: SourceMetadata(source.Position, "encoded-bits")));
        elapsed += duration;
    }

    private static void AddPulse(
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed,
        ushort pulseTStates,
        int count,
        long sourcePosition,
        string role)
    {
        if (pulseTStates == 0 || count == 0) return;
        var duration = TStatesDuration((long)pulseTStates * count);
        output.Add(new SequentialMediaSegment(
            position++, SequentialSegmentKind.Pulse, count, elapsed, duration,
            direction: SequentialTravelDirection.Forward,
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceSegmentPosition"] = sourcePosition.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["role"] = role,
                ["pulseTStates"] = pulseTStates.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["pulseCount"] = count.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }));
        elapsed += duration;
    }

    private static void AddPause(
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed,
        ushort milliseconds,
        long sourcePosition)
    {
        if (milliseconds == 0)
        {
            output.Add(new SequentialMediaSegment(position++, SequentialSegmentKind.TapeMark,
                metadata: SourceMetadata(sourcePosition, "stop")));
            return;
        }
        var duration = TimeSpan.FromMilliseconds(milliseconds);
        output.Add(new SequentialMediaSegment(position++, SequentialSegmentKind.Silence, start: elapsed, duration: duration,
            direction: SequentialTravelDirection.Forward, metadata: SourceMetadata(sourcePosition, "pause")));
        elapsed += duration;
    }

    private static void AddPause(
        byte[] payload,
        SequentialMediaSegment source,
        List<SequentialMediaSegment> output,
        ref long position,
        ref TimeSpan elapsed)
    {
        Require(payload, 2, TzxConstants.Pause);
        AddPause(output, ref position, ref elapsed, BinaryPrimitives.ReadUInt16LittleEndian(payload), source.Position);
    }

    private static async Task<byte[]> ReadPayloadAsync(SequentialMediaSegment segment, CancellationToken cancellationToken)
    {
        if (segment.DataRange is null) return [];
        if (segment.DataRange.Source is null || segment.DataRange.Length > int.MaxValue)
            throw new NotSupportedException("A TZX block payload is unavailable or too large to expand.");
        var payload = new byte[checked((int)segment.DataRange.Length)];
        await segment.DataRange.Source.ReadExactlyAsync(segment.DataRange.SourceOffset, payload, cancellationToken).ConfigureAwait(false);
        return payload;
    }

    private static MediaDataRange Slice(SequentialMediaSegment source, int offset, int length)
    {
        var range = source.DataRange ?? throw new InvalidDataException("The TZX source block payload is unavailable.");
        return new MediaDataRange(range.Address + offset, length, range.Kind, range.Source, range.SourceOffset + offset);
    }

    private static IReadOnlyDictionary<string, string> SourceMetadata(long sourcePosition, string role) =>
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["sourceSegmentPosition"] = sourcePosition.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["role"] = role
        };

    private static bool TryBlockId(SequentialMediaSegment segment, out byte id)
    {
        id = 0;
        return segment.Metadata.TryGetValue(TzxConstants.BlockIdMetadataKey, out var text)
            && byte.TryParse(text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out id);
    }

    private static int ReadUInt24(ReadOnlySpan<byte> value) => value[0] | value[1] << 8 | value[2] << 16;
    private static long DataBitCount(int dataLength, byte usedBits) => dataLength == 0 ? 0 : (long)(dataLength - 1) * 8 + usedBits;
    private static TimeSpan TStatesDuration(long tStates) => TimeSpan.FromSeconds(tStates / (double)TzxConstants.TStatesPerSecond);

    private static void Require(byte[] payload, int required, byte id)
    {
        if (payload.Length < required) throw new InvalidDataException($"TZX block 0x{id:x2} is truncated.");
    }
}
