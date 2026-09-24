using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains one validated CHD optical track declaration decoded from CHTR or CHT2 metadata.</summary>
internal sealed class ChdOpticalTrackMetadata
{
    public ChdOpticalTrackMetadata(
        int trackNumber,
        OpticalTrackMode mode,
        int dataSize,
        int subchannelSize,
        long frameCount,
        long pregapFrames,
        long postgapFrames,
        bool pregapStored)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(trackNumber);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dataSize);
        ArgumentOutOfRangeException.ThrowIfNegative(subchannelSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameCount);
        ArgumentOutOfRangeException.ThrowIfNegative(pregapFrames);
        ArgumentOutOfRangeException.ThrowIfNegative(postgapFrames);
        if (pregapFrames >= frameCount)
            throw new ArgumentException("The CHD pregap must be shorter than the track.", nameof(pregapFrames));

        TrackNumber = trackNumber;
        Mode = mode;
        DataSize = dataSize;
        SubchannelSize = subchannelSize;
        FrameCount = frameCount;
        PregapFrames = pregapFrames;
        PostgapFrames = postgapFrames;
        PregapStored = pregapStored;
    }

    public int TrackNumber { get; }
    public OpticalTrackMode Mode { get; }
    public int DataSize { get; }
    public int SubchannelSize { get; }
    public long FrameCount { get; }
    public long PregapFrames { get; }
    public long PostgapFrames { get; }
    public bool PregapStored { get; }
}
