using GWGUI.App.Enums.Rendering.Sequential;

namespace GWGUI.App.Contracts.Rendering.Sequential;

public sealed record SequentialMediaRenderModel(
    long? LogicalLength,
    TimeSpan? Duration,
    IReadOnlyList<SequentialMediaSegment> Segments,
    IReadOnlyList<float>? Waveform = null);

public sealed record SequentialMediaSegment(
    long Position,
    int Lane,
    TimeSpan Start,
    TimeSpan Duration,
    SequentialSegmentKind Kind,
    int? FaceNumber = null,
    int? TrackNumber = null,
    int? ChannelNumber = null,
    string? RecognizedName = null,
    string? DecodeError = null);
