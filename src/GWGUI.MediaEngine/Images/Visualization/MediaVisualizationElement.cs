namespace GWGUI.MediaEngine.Images.Visualization;

/// <summary>Identifies one element in the order used to prepare a media visualization.</summary>
public sealed record MediaVisualizationElement
{
    public MediaVisualizationElement(
        long position,
        int? surface = null,
        long? length = null,
        int? sessionNumber = null,
        int? trackNumber = null,
        int? layerNumber = null,
        int? faceNumber = null,
        int? channelNumber = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        if (surface is < 0) throw new ArgumentOutOfRangeException(nameof(surface));
        if (length is <= 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (sessionNumber is <= 0) throw new ArgumentOutOfRangeException(nameof(sessionNumber));
        if (trackNumber is < 0) throw new ArgumentOutOfRangeException(nameof(trackNumber));
        if (layerNumber is < 0) throw new ArgumentOutOfRangeException(nameof(layerNumber));
        if (faceNumber is < 0) throw new ArgumentOutOfRangeException(nameof(faceNumber));
        if (channelNumber is < 0) throw new ArgumentOutOfRangeException(nameof(channelNumber));

        Position = position;
        Surface = surface;
        Length = length;
        SessionNumber = sessionNumber;
        TrackNumber = trackNumber;
        LayerNumber = layerNumber;
        FaceNumber = faceNumber;
        ChannelNumber = channelNumber;
    }

    public long Position { get; }

    public int? Surface { get; }

    public long? Length { get; }

    public int? SessionNumber { get; }

    public int? TrackNumber { get; }

    public int? LayerNumber { get; }

    public int? FaceNumber { get; }

    public int? ChannelNumber { get; }
}
