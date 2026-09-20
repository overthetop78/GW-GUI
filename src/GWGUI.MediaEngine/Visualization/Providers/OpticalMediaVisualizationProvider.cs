using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Representations.Optical;

namespace GWGUI.MediaEngine.Visualization.Providers;

/// <summary>Builds progressive visualization data from explicitly described optical tracks and faces.</summary>
public sealed class OpticalMediaVisualizationProvider : IMediaVisualizationProvider
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedKinds =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedKinds;

    public bool CanProvide(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.Representation is OpticalMediaImageRepresentation;
    }

    public MediaVisualizationDescriptor CreateDescriptor(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not OpticalMediaImageRepresentation optical)
            throw new NotSupportedException($"Representation '{document.Representation.RepresentationKind}' is not an optical representation.");

        var surfaces = optical.FaceCount is { } faceCount ? Enumerable.Range(0, faceCount).ToArray() : [];
        var layers = optical.LayerCount is { } layerCount ? Enumerable.Range(0, layerCount).ToArray() : null;
        var knownSurface = surfaces.Length == 1 ? 0 : (int?)null;
        var elements = optical.Tracks?.Select(track =>
            new MediaVisualizationElement(
                track.FirstSector,
                knownSurface,
                track.SectorCount,
                track.SessionNumber,
                track.TrackNumber)).ToArray() ?? [];
        return new(
            MediaRepresentationKind.OpticalTracks,
            surfaces,
            MediaVisualizationProgressUnit.OpticalTrack,
            MediaVisualizationDirection.Ascending,
            elements,
            layers);
    }
}
