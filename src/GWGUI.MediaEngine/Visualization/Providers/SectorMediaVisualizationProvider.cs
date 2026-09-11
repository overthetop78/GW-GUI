using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Visualization.Providers;

/// <summary>Builds visualization traversal data directly from sector geometry without synthesizing flux.</summary>
public sealed class SectorMediaVisualizationProvider : IMediaVisualizationProvider
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedKinds =
        new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedKinds;

    public bool CanProvide(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.Representation is SectorMediaImageRepresentation;
    }

    public MediaVisualizationDescriptor CreateDescriptor(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not SectorMediaImageRepresentation sectors)
            throw new NotSupportedException($"Representation '{document.Representation.RepresentationKind}' is not a sector representation.");

        var image = sectors.Image;
        var surfaces = Enumerable.Range(0, image.Heads).ToArray();
        var elements = Enumerable.Range(0, image.Cylinders)
            .SelectMany(cylinder => surfaces.Select(surface => new MediaVisualizationElement(cylinder, surface)))
            .ToArray();
        return new(
            MediaRepresentationKind.Sectors,
            surfaces,
            MediaVisualizationProgressUnit.Track,
            MediaVisualizationDirection.Ascending,
            elements);
    }
}
