using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Visualization;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.MediaEngine.Visualization.Providers;

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

        var elements = blocks.Ranges
            .Select(range => new MediaVisualizationElement(range.Address, 0, range.Length))
            .ToArray();
        return new(
            MediaRepresentationKind.Blocks,
            [0],
            MediaVisualizationProgressUnit.BlockRange,
            MediaVisualizationDirection.Ascending,
            elements);
    }
}
