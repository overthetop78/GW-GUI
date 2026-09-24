using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Images.Models.Sequential;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Keeps a decoded signal timeline separate from the retained container blocks used for lossless writing.</summary>
public sealed class SequentialSignalDecodeResult
{
    public SequentialSignalDecodeResult(
        SequentialMediaImageRepresentation timeline,
        IReadOnlyList<SequentialMediaSegment> sourceSegments,
        IReadOnlyList<string>? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(sourceSegments);
        Timeline = timeline;
        SourceSegments = new ReadOnlyCollection<SequentialMediaSegment>(sourceSegments.ToArray());
        Diagnostics = new ReadOnlyCollection<string>((diagnostics ?? []).ToArray());
    }

    public SequentialMediaImageRepresentation Timeline { get; }
    public IReadOnlyList<SequentialMediaSegment> SourceSegments { get; }
    public IReadOnlyList<string> Diagnostics { get; }
}
