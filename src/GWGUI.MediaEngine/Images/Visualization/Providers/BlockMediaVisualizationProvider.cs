using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Images.Models.Blocks;

namespace GWGUI.MediaEngine.Images.Visualization.Providers;

/// <summary>Builds progressive visualization data from the declared 64-bit ranges of block media.</summary>
public sealed class BlockMediaVisualizationProvider : IMediaVisualizationProvider
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedKinds =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedKinds;

    public bool CanProvide(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.Representation is BlockMediaImageRepresentation;
    }

    public MediaVisualizationDescriptor CreateDescriptor(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.Representation is not BlockMediaImageRepresentation blocks)
            throw new NotSupportedException($"Representation '{document.Representation.RepresentationKind}' is not a block representation.");

        var boundaries = new SortedSet<long> { 0, blocks.Capacity };
        foreach (var range in blocks.Ranges)
        {
            boundaries.Add(range.Address);
            boundaries.Add(checked(range.Address + range.Length));
        }
        foreach (var volume in document.Volumes)
        {
            if (volume.Start < blocks.Capacity) boundaries.Add(volume.Start);
            var end = checked(volume.Start + volume.Length);
            if (end <= blocks.Capacity) boundaries.Add(end);
        }
        var points = boundaries.ToArray();
        var elements = new MediaVisualizationElement[points.Length - 1];
        for (var index = 0; index < elements.Length; index++)
            elements[index] = new MediaVisualizationElement(points[index], 0, points[index + 1] - points[index]);
        return new(
            MediaRepresentationKind.Blocks,
            [0],
            MediaVisualizationProgressUnit.BlockRange,
            MediaVisualizationDirection.Ascending,
            elements);
    }

}
