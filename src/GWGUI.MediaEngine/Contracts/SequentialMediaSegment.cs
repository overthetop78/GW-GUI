using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Describes one ordered portion of sequential media using only information supplied by its source.</summary>
public sealed class SequentialMediaSegment
{
    public SequentialMediaSegment(
        long position,
        SequentialSegmentKind kind,
        long? length = null,
        TimeSpan? start = null,
        TimeSpan? duration = null,
        int? faceNumber = null,
        int? trackNumber = null,
        int? channelNumber = null,
        SequentialTravelDirection direction = SequentialTravelDirection.Unknown,
        MediaDataRange? dataRange = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        if (length is <= 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (start is { } segmentStart && segmentStart < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(start));
        if (duration is { } segmentDuration && segmentDuration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duration));
        if (faceNumber is < 0) throw new ArgumentOutOfRangeException(nameof(faceNumber));
        if (trackNumber is < 0) throw new ArgumentOutOfRangeException(nameof(trackNumber));
        if (channelNumber is < 0) throw new ArgumentOutOfRangeException(nameof(channelNumber));

        Position = position;
        Kind = kind;
        Length = length;
        Start = start;
        Duration = duration;
        FaceNumber = faceNumber;
        TrackNumber = trackNumber;
        ChannelNumber = channelNumber;
        Direction = direction;
        DataRange = dataRange;
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal));
    }

    public long Position { get; }
    public SequentialSegmentKind Kind { get; }
    public long? Length { get; }
    public TimeSpan? Start { get; }
    public TimeSpan? Duration { get; }
    public int? FaceNumber { get; }
    public int? TrackNumber { get; }
    public int? ChannelNumber { get; }
    public SequentialTravelDirection Direction { get; }
    public MediaDataRange? DataRange { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
