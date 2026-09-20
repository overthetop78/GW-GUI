using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Representations.Sequential;

/// <summary>Describes sequential faces, tracks, channels, segments, and timing only when supplied by the source.</summary>
public sealed class SequentialMediaImageRepresentation : IMediaImageRepresentation
{
    public SequentialMediaImageRepresentation(
        long? logicalLength,
        TimeSpan? duration = null,
        IReadOnlyList<SequentialMediaSegment>? segments = null)
    {
        if (logicalLength is < 0) throw new ArgumentOutOfRangeException(nameof(logicalLength));
        if (duration is { } declaredDuration && declaredDuration < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duration));

        if (segments is not null)
        {
            foreach (var segment in segments)
            {
                if (segment.FaceNumber is < 0) throw new ArgumentOutOfRangeException(nameof(segments), segment, "A face number cannot be negative.");
                if (segment.TrackNumber is < 0) throw new ArgumentOutOfRangeException(nameof(segments), segment, "A track number cannot be negative.");
                if (segment.ChannelNumber is < 0) throw new ArgumentOutOfRangeException(nameof(segments), segment, "A channel number cannot be negative.");
                if (duration is { } totalDuration && segment.Start is { } start && segment.Duration is { } segmentDuration
                    && start + segmentDuration > totalDuration)
                    throw new ArgumentOutOfRangeException(nameof(segments), segment, "A segment exceeds the declared duration.");
            }

            Segments = new ReadOnlyCollection<SequentialMediaSegment>(segments.ToArray());
            Faces = ValuesOrNull(segments.Select(segment => segment.FaceNumber));
            Tracks = ValuesOrNull(segments.Select(segment => segment.TrackNumber));
            Channels = ValuesOrNull(segments.Select(segment => segment.ChannelNumber));
        }

        LogicalLength = logicalLength;
        Duration = duration;
    }

    public MediaRepresentationKind RepresentationKind => MediaRepresentationKind.Sequential;

    public long? LogicalLength { get; }

    public bool SupportsRandomAccess => false;

    public bool SupportsSequentialAccess => true;

    public TimeSpan? Duration { get; }

    public IReadOnlyList<int>? Faces { get; }

    public IReadOnlyList<int>? Tracks { get; }

    public IReadOnlyList<int>? Channels { get; }

    public IReadOnlyList<SequentialMediaSegment>? Segments { get; }

    private static IReadOnlyList<int>? ValuesOrNull(IEnumerable<int?> values)
    {
        var availableValues = values.Where(value => value.HasValue).Select(value => value!.Value).Distinct().Order().ToArray();
        return availableValues.Length == 0 ? null : new ReadOnlyCollection<int>(availableValues);
    }
}
