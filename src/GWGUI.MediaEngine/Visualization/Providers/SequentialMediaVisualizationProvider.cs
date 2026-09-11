using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Visualization.Providers;

/// <summary>Builds progressive visualization data from explicitly described sequential segments and lanes.</summary>
public sealed class SequentialMediaVisualizationProvider : IMediaVisualizationProvider
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedKinds;

    public bool CanProvide(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.Representation is SequentialMediaImageRepresentation;
    }

    public MediaVisualizationDescriptor CreateDescriptor(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not SequentialMediaImageRepresentation sequential)
            throw new NotSupportedException($"Representation '{document.Representation.RepresentationKind}' is not a sequential representation.");

        var segments = sequential.Segments ?? [];
        var lanes = segments
            .Select(segment => (segment.FaceNumber, segment.TrackNumber, segment.ChannelNumber))
            .Distinct()
            .OrderBy(lane => lane.FaceNumber ?? int.MaxValue)
            .ThenBy(lane => lane.TrackNumber ?? int.MaxValue)
            .ThenBy(lane => lane.ChannelNumber ?? int.MaxValue)
            .ToArray();
        var laneNumbers = lanes
            .Select((lane, index) => (lane, index))
            .ToDictionary(item => item.lane, item => item.index);
        var surfaces = Enumerable.Range(0, lanes.Length).ToArray();
        var elements = segments.Select(segment => new MediaVisualizationElement(
            segment.Start?.Ticks ?? segment.Position,
            surface: laneNumbers[(segment.FaceNumber, segment.TrackNumber, segment.ChannelNumber)],
            length: segment.Duration?.Ticks ?? segment.Length,
            trackNumber: segment.TrackNumber,
            faceNumber: segment.FaceNumber,
            channelNumber: segment.ChannelNumber)).ToArray();
        var direction = segments.Count == 0 || segments.All(segment => segment.Direction == SequentialTravelDirection.Forward)
            ? MediaVisualizationDirection.Ascending
            : segments.All(segment => segment.Direction == SequentialTravelDirection.Reverse)
                ? MediaVisualizationDirection.Descending
                : MediaVisualizationDirection.SourceDefined;
        return new(
            MediaRepresentationKind.Sequential,
            surfaces,
            MediaVisualizationProgressUnit.Segment,
            direction,
            elements);
    }
}
