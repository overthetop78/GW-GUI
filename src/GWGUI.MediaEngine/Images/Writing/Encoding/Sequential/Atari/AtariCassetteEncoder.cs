using System.Collections.Frozen;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Encoding;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Atari;

/// <summary>Encodes validated Atari serial blocks as CAS data chunks while retaining unrelated source chunks.</summary>
public sealed class AtariCassetteEncoder : ISequentialMediaEncoder
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.AtariCas }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedMachines =
        new[] { DiskSystemIds.Atari8Bit }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public string Id => AtariCasConstants.EncoderId;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<string> MachineIds => SupportedMachines;

    public bool CanEncode(SequentialEncodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SupportedFormats.Contains(request.TargetFormatId)
            && SupportedMachines.Contains(request.MachineId)
            && request.Blocks.Count > 0
            && request.Blocks.All(block => block.IntegrityValid == true && block.Data.Length <= ushort.MaxValue);
    }

    public Task<SequentialMediaImageRepresentation> EncodeAsync(
        SequentialEncodeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanEncode(request))
            throw new InvalidDataException("Atari CAS encoding requires checksum-valid blocks no larger than 65535 bytes.");
        cancellationToken.ThrowIfCancellationRequested();

        var initialMark = ReadDuration(request.Parameters, "initialMarkMilliseconds", AtariCasConstants.InitialMarkDurationMilliseconds);
        var subsequentMark = ReadDuration(request.Parameters, "subsequentMarkMilliseconds", AtariCasConstants.SubsequentMarkDurationMilliseconds);
        var output = new List<SequentialMediaSegment>();
        var retained = request.RetainedSegments.OrderBy(segment => segment.Position).ToList();
        var marker = retained.FirstOrDefault(IsFileMarker);
        if (marker is not null)
        {
            output.Add(Clone(marker, output.Count));
            retained.Remove(marker);
        }
        else
        {
            output.Add(new SequentialMediaSegment(
                output.Count,
                SequentialSegmentKind.TapeMark,
                metadata: ChunkMetadata(AtariCasConstants.FileMarkerChunk, 0)));
        }

        var pending = new List<OrderedItem>();
        pending.AddRange(retained.Select(segment => new OrderedItem(segment.Position, segment, null)));
        pending.AddRange(request.Blocks.Select((block, index) =>
            new OrderedItem(block.SourceSegmentPositions.Count == 0 ? long.MaxValue - request.Blocks.Count + index : block.SourceSegmentPositions.Min(), null, block)));
        var elapsed = TimeSpan.Zero;
        var blockIndex = 0;
        foreach (var item in pending.OrderBy(item => item.Position).ThenBy(item => item.Block is null ? 0 : 1))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (item.Segment is not null)
            {
                output.Add(Clone(item.Segment, output.Count));
                continue;
            }

            var block = item.Block!;
            output.Add(new SequentialMediaSegment(
                output.Count,
                SequentialSegmentKind.Carrier,
                metadata: ChunkMetadata(AtariCasConstants.BaudRateChunk, AtariCasConstants.DefaultBaudRate)));
            var markMilliseconds = blockIndex++ == 0 ? initialMark : subsequentMark;
            var duration = TimeSpan.FromSeconds(block.Data.Length * AtariCasConstants.SerialFrameBitCount / (double)AtariCasConstants.DefaultBaudRate);
            var data = block.Data.ToArray();
            var source = new MemoryRandomAccessData(data);
            output.Add(new SequentialMediaSegment(
                output.Count,
                SequentialSegmentKind.DataBlock,
                data.Length,
                elapsed,
                duration,
                direction: SequentialTravelDirection.Forward,
                dataRange: new MediaDataRange(0, data.Length, MediaDataRangeKind.Stored, source, 0),
                metadata: ChunkMetadata(AtariCasConstants.DataChunk, markMilliseconds, AtariCasConstants.DefaultBaudRate)));
            elapsed += TimeSpan.FromMilliseconds(markMilliseconds) + duration;
        }

        var logicalLength = output.Sum(segment => segment.DataRange?.Length ?? 0);
        return Task.FromResult(new SequentialMediaImageRepresentation(logicalLength, elapsed == TimeSpan.Zero ? null : elapsed, output));
    }

    private static bool IsFileMarker(SequentialMediaSegment segment) =>
        segment.Metadata.TryGetValue(AtariCasConstants.ChunkIdMetadataKey, out var chunkId)
        && chunkId == AtariCasConstants.FileMarkerChunk;

    private static SequentialMediaSegment Clone(SequentialMediaSegment source, long position) =>
        new(
            position,
            source.Kind,
            source.Length,
            source.Start,
            source.Duration,
            source.FaceNumber,
            source.TrackNumber,
            source.ChannelNumber,
            source.Direction,
            source.DataRange,
            source.Metadata);

    private static IReadOnlyDictionary<string, string> ChunkMetadata(string chunkId, int auxiliary, int? baudRate = null)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [AtariCasConstants.ChunkIdMetadataKey] = chunkId,
            [AtariCasConstants.AuxiliaryMetadataKey] = auxiliary.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        if (baudRate is not null)
            metadata[AtariCasConstants.BaudRateMetadataKey] = baudRate.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return metadata;
    }

    private static int ReadDuration(IReadOnlyDictionary<string, string> parameters, string key, int defaultValue)
    {
        if (!parameters.TryGetValue(key, out var text)) return defaultValue;
        if (!int.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var value)
            || value is < 0 or > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(key, "An Atari CAS mark duration must fit the 16-bit auxiliary field.");
        return value;
    }

    private sealed record OrderedItem(long Position, SequentialMediaSegment? Segment, SequentialDecodedBlock? Block);
}
