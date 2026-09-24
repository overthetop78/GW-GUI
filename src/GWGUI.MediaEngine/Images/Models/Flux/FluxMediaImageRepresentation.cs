using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Images.Models.Flux;

/// <summary>Exposes captured flux surfaces, tracks, revolutions, and transitions without reducing them to sectors.</summary>
public sealed class FluxMediaImageRepresentation : IMediaImageRepresentation
{
    public FluxMediaImageRepresentation(ProtectedTrackImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        Image = image;
        Surfaces = new ReadOnlyCollection<int>(image.Tracks.Select(track => track.Head).Distinct().Order().ToArray());
    }

    public MediaRepresentationKind RepresentationKind => MediaRepresentationKind.Flux;

    public long? LogicalLength => null;

    public bool SupportsRandomAccess => true;

    public bool SupportsSequentialAccess => true;

    public ProtectedTrackImage Image { get; }

    public IReadOnlyList<int> Surfaces { get; }

    public IReadOnlyList<ProtectedTrack> Tracks => Image.Tracks;

    public int RevolutionCount => Tracks.Sum(track => track.Revolutions.Count);

    public long TransitionCount => Tracks.Sum(track => (long)track.Revolutions.Sum(revolution => revolution.Flux.FluxIntervals.Count));
}
