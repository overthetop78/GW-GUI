using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Images.Models.Flux;

namespace GWGUI.MediaEngine.Images.Visualization.Providers;

/// <summary>Builds visualization traversal data from preserved flux tracks without decoding sectors.</summary>
public sealed class FluxMediaVisualizationProvider : IMediaVisualizationProvider
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedKinds =
        new[] { MediaRepresentationKind.Flux }.ToFrozenSet();

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedKinds;

    public bool CanProvide(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.Representation is FluxMediaImageRepresentation;
    }

    public MediaVisualizationDescriptor CreateDescriptor(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not FluxMediaImageRepresentation flux)
            throw new NotSupportedException($"Representation '{document.Representation.RepresentationKind}' is not a flux representation.");

        var elements = flux.Tracks
            .Select(track => new MediaVisualizationElement(track.Cylinder, track.Head))
            .ToArray();
        return new(
            MediaRepresentationKind.Flux,
            flux.Surfaces,
            MediaVisualizationProgressUnit.Track,
            MediaVisualizationDirection.SourceDefined,
            elements);
    }
}
