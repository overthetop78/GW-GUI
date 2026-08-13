using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Decoding;

namespace GWGUI.MediaEngine.TrackImages;

/// <summary>Représente une piste sans réduire ses protections à des secteurs logiques.</summary>
public sealed record ProtectedTrack
{
    public ProtectedTrack(int cylinder, int head, IReadOnlyList<bool>? bits, IReadOnlyList<TrackTimingSegment> timing, IReadOnlyList<FluxStructure> structures, IReadOnlyList<TrackFeature> features, IReadOnlyList<TrackFluxRevolution> revolutions)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cylinder);
        ArgumentOutOfRangeException.ThrowIfNegative(head);
        ArgumentNullException.ThrowIfNull(timing);
        ArgumentNullException.ThrowIfNull(structures);
        ArgumentNullException.ThrowIfNull(features);
        ArgumentNullException.ThrowIfNull(revolutions);
        if ((bits is null || bits.Count == 0) && revolutions.Count == 0) throw new ArgumentException("A protected track requires bit cells or flux revolutions.");
        if (bits is null && (timing.Count != 0 || structures.Count != 0 || features.Count != 0)) throw new ArgumentException("Bit-oriented timing, structures and features require bit cells.");
        if (bits is not null)
        {
            ValidateRanges(timing.Select(segment => (segment.BitOffset, segment.BitLength)), bits.Count, nameof(timing));
            ValidateRanges(structures.Select(structure => (structure.BitOffset, structure.BitLength)), bits.Count, nameof(structures));
            ValidateRanges(features.Select(feature => (feature.BitOffset, feature.BitLength)), bits.Count, nameof(features));
            if (timing.Any(segment => segment.BitCellNanoseconds <= 0 || double.IsNaN(segment.BitCellNanoseconds) || double.IsInfinity(segment.BitCellNanoseconds))) throw new ArgumentOutOfRangeException(nameof(timing));
        }
        Cylinder = cylinder;
        Head = head;
        Bits = bits is null ? null : new ReadOnlyCollection<bool>(bits.ToArray());
        Timing = new ReadOnlyCollection<TrackTimingSegment>(timing.ToArray());
        Structures = new ReadOnlyCollection<FluxStructure>(structures.ToArray());
        Features = new ReadOnlyCollection<TrackFeature>(features.ToArray());
        Revolutions = new ReadOnlyCollection<TrackFluxRevolution>(revolutions.ToArray());
    }

    public int Cylinder { get; }
    public int Head { get; }
    public IReadOnlyList<bool>? Bits { get; }
    public IReadOnlyList<TrackTimingSegment> Timing { get; }
    public IReadOnlyList<FluxStructure> Structures { get; }
    public IReadOnlyList<TrackFeature> Features { get; }
    public IReadOnlyList<TrackFluxRevolution> Revolutions { get; }

    private static void ValidateRanges(IEnumerable<(int Offset, int Length)> ranges, int bitCount, string parameter)
    {
        foreach (var range in ranges) if (range.Offset < 0 || range.Length <= 0 || range.Offset > bitCount - range.Length) throw new ArgumentOutOfRangeException(parameter);
    }
}
