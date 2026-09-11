using System.Collections.ObjectModel;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Enums;

namespace GWGUI.MediaEngine.Visualization;

/// <summary>Describes how a recognized media representation is prepared progressively for display.</summary>
public sealed class MediaVisualizationDescriptor
{
    public MediaVisualizationDescriptor(
        MediaRepresentationKind representationKind,
        IReadOnlyList<int> surfaces,
        MediaVisualizationProgressUnit progressUnit,
        MediaVisualizationDirection direction,
        IReadOnlyList<MediaVisualizationElement> elements)
    {
        ArgumentNullException.ThrowIfNull(surfaces);
        ArgumentNullException.ThrowIfNull(elements);
        if (surfaces.Any(surface => surface < 0)) throw new ArgumentOutOfRangeException(nameof(surfaces));
        if (surfaces.Distinct().Count() != surfaces.Count) throw new ArgumentException("Visualization surfaces must be unique.", nameof(surfaces));
        if (elements.Any(element => element is null)) throw new ArgumentException("A visualization element cannot be null.", nameof(elements));

        var surfaceSet = surfaces.ToHashSet();
        if (elements.Any(element => element.Surface is { } surface && !surfaceSet.Contains(surface)))
            throw new ArgumentException("Every element surface must be declared by the descriptor.", nameof(elements));

        RepresentationKind = representationKind;
        Surfaces = new ReadOnlyCollection<int>(surfaces.ToArray());
        ProgressUnit = progressUnit;
        Direction = direction;
        Elements = new ReadOnlyCollection<MediaVisualizationElement>(elements.ToArray());
    }

    public MediaRepresentationKind RepresentationKind { get; }

    public IReadOnlyList<int> Surfaces { get; }

    public MediaVisualizationProgressUnit ProgressUnit { get; }

    public MediaVisualizationDirection Direction { get; }

    public IReadOnlyList<MediaVisualizationElement> Elements { get; }
}
