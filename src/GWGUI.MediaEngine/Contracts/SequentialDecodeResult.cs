using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Returns decoded protocol blocks while retaining every source segment that was not decoded.</summary>
public sealed class SequentialDecodeResult
{
    public SequentialDecodeResult(
        string decoderId,
        double confidence,
        IReadOnlyList<SequentialDecodedBlock> blocks,
        IReadOnlyList<SequentialMediaSegment> undecodedSegments,
        IReadOnlyList<string>? diagnostics = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(decoderId);
        if (double.IsNaN(confidence) || confidence is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(confidence));
        ArgumentNullException.ThrowIfNull(blocks);
        ArgumentNullException.ThrowIfNull(undecodedSegments);

        DecoderId = decoderId;
        Confidence = confidence;
        Blocks = new ReadOnlyCollection<SequentialDecodedBlock>(blocks.ToArray());
        UndecodedSegments = new ReadOnlyCollection<SequentialMediaSegment>(undecodedSegments.ToArray());
        Diagnostics = new ReadOnlyCollection<string>((diagnostics ?? []).ToArray());
    }

    public string DecoderId { get; }
    public double Confidence { get; }
    public IReadOnlyList<SequentialDecodedBlock> Blocks { get; }
    public IReadOnlyList<SequentialMediaSegment> UndecodedSegments { get; }
    public IReadOnlyList<string> Diagnostics { get; }
}
