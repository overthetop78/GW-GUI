using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Encoding;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Encoding.Sequential.Tzx;

/// <summary>Encodes validated data blocks and explicit pulse or pause segments as retained TZX blocks.</summary>
public sealed class TzxSignalEncoder : ISequentialMediaEncoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.Tzx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Spectrum, DiskSystemIds.Amstrad, DiskSystemIds.Msx }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => TzxConstants.EncoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanEncode(SequentialEncodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SupportedFormats.Contains(request.TargetFormatId)
            && SupportedMachines.Contains(request.MachineId)
            && request.Blocks.All(block => block.IntegrityValid == true && block.Data.Length <= ushort.MaxValue)
            && request.RetainedSegments.All(CanRetain);
    }

    public Task<SequentialMediaImageRepresentation> EncodeAsync(
        SequentialEncodeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanEncode(request))
            throw new InvalidDataException("TZX encoding requires validated blocks and retained TZX, pulse, or pause segments.");
        cancellationToken.ThrowIfCancellationRequested();

        var ordered = new List<OrderedItem>();
        ordered.AddRange(request.RetainedSegments.Select(segment => new OrderedItem(segment.Position, segment, null)));
        ordered.AddRange(request.Blocks.Select((block, index) => new OrderedItem(
            block.SourceSegmentPositions.Count == 0 ? long.MaxValue - request.Blocks.Count + index : block.SourceSegmentPositions.Min(),
            null,
            block)));

        var output = new List<SequentialMediaSegment>();
        foreach (var item in ordered.OrderBy(item => item.Position).ThenBy(item => item.Block is null ? 0 : 1))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (item.Block is not null)
            {
                output.Add(CreateBlock(output.Count, TzxConstants.StandardSpeedData, CreateStandardPayload(item.Block.Data.Span)));
                continue;
            }
            var source = item.Segment!;
            if (source.Metadata.ContainsKey(TzxConstants.BlockIdMetadataKey))
            {
                output.Add(Clone(source, output.Count));
                continue;
            }
            if (source.Kind == SequentialSegmentKind.Pulse)
            {
                var pulseTStates = ReadUInt16(source.Metadata, "pulseTStates");
                var pulseCount = ReadUInt16(source.Metadata, "pulseCount", 1);
                output.Add(CreateBlock(output.Count, TzxConstants.PureTone, CreatePureTonePayload(pulseTStates, pulseCount)));
                continue;
            }
            var milliseconds = checked((ushort)Math.Round(source.Duration!.Value.TotalMilliseconds));
            output.Add(CreateBlock(output.Count, TzxConstants.Pause, CreatePausePayload(milliseconds)));
        }

        var logicalLength = output.Sum(segment => segment.DataRange?.Length ?? 0);
        return Task.FromResult(new SequentialMediaImageRepresentation(logicalLength, segments: output));
    }

    private static bool CanRetain(SequentialMediaSegment segment)
    {
        if (segment.Metadata.ContainsKey(TzxConstants.BlockIdMetadataKey))
            return segment.Length is null && segment.DataRange is null
                || segment.DataRange is { Kind: MediaDataRangeKind.Stored, Source: not null } range && range.Length == segment.Length;
        if (segment.Kind == SequentialSegmentKind.Pulse)
            return TryReadUInt16(segment.Metadata, "pulseTStates", out _)
                && (!segment.Metadata.ContainsKey("pulseCount") || TryReadUInt16(segment.Metadata, "pulseCount", out _));
        return segment.Kind == SequentialSegmentKind.Silence
            && segment.Duration is { } duration
            && duration.TotalMilliseconds is >= 1 and <= ushort.MaxValue;
    }

    private static SequentialMediaSegment CreateBlock(long position, byte id, byte[] payload)
    {
        var source = new MemoryRandomAccessData(payload);
        return new SequentialMediaSegment(
            position,
            id switch
            {
                TzxConstants.Pause => SequentialSegmentKind.Silence,
                TzxConstants.PureTone or TzxConstants.PulseSequence => SequentialSegmentKind.Pulse,
                _ => SequentialSegmentKind.DataBlock
            },
            payload.Length,
            dataRange: new MediaDataRange(0, payload.Length, MediaDataRangeKind.Stored, source, 0),
            metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [TzxConstants.BlockIdMetadataKey] = id.ToString("x2", System.Globalization.CultureInfo.InvariantCulture),
                ["payloadLength"] = payload.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["known"] = bool.TrueString
            });
    }

    private static byte[] CreateStandardPayload(ReadOnlySpan<byte> data)
    {
        var payload = new byte[4 + data.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(payload, TzxConstants.StandardPauseMilliseconds);
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(2), checked((ushort)data.Length));
        data.CopyTo(payload.AsSpan(4));
        return payload;
    }

    private static byte[] CreatePureTonePayload(ushort pulseTStates, ushort pulseCount)
    {
        var payload = new byte[4];
        BinaryPrimitives.WriteUInt16LittleEndian(payload, pulseTStates);
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(2), pulseCount);
        return payload;
    }

    private static byte[] CreatePausePayload(ushort milliseconds)
    {
        var payload = new byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(payload, milliseconds);
        return payload;
    }

    private static SequentialMediaSegment Clone(SequentialMediaSegment source, long position) =>
        new(position, source.Kind, source.Length, source.Start, source.Duration, source.FaceNumber, source.TrackNumber,
            source.ChannelNumber, source.Direction, source.DataRange, source.Metadata);

    private static ushort ReadUInt16(IReadOnlyDictionary<string, string> metadata, string key, ushort defaultValue = 0) =>
        metadata.TryGetValue(key, out var text)
        && ushort.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var value)
            ? value
            : defaultValue;

    private static bool TryReadUInt16(IReadOnlyDictionary<string, string> metadata, string key, out ushort value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && ushort.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out value)
            && value > 0;
    }

    private sealed record OrderedItem(long Position, SequentialMediaSegment? Segment, SequentialDecodedBlock? Block);
}
