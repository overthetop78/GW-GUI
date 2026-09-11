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
        var surfaces = segments.Select(Lane).Distinct().Order().ToArray();
        if (surfaces.Length == 0) surfaces = [0];
        var elements = segments.Select(segment => new MediaVisualizationElement(
            segment.Start.Ticks,
            Lane(segment),
            segment.Duration.Ticks)).ToArray();
        return new(
            MediaRepresentationKind.Sequential,
            surfaces,
            MediaVisualizationProgressUnit.Segment,
            MediaVisualizationDirection.Ascending,
            elements);
    }

    private static int Lane((int? FaceNumber, int? TrackNumber, int? ChannelNumber, TimeSpan Start, TimeSpan Duration) segment) =>
        segment.ChannelNumber ?? segment.TrackNumber ?? segment.FaceNumber ?? 0;
}
