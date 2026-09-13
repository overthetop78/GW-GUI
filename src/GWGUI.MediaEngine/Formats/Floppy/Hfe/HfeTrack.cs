
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.MediaEngine.Formats.Floppy.Hfe;

/// <summary>Représente une face de piste HFE avec ses cellules et son timing uniforme.</summary>
public sealed record HfeTrack
{
    public HfeTrack(int cylinder, int head, IReadOnlyList<bool> bits, uint bitCellTicks)
        : this(cylinder, head, bits, bitCellTicks,
            [new TrackTimingSegment(0, bits.Count, bitCellTicks * (double)HfeFormat.TickNanoseconds)],
            [], FluxRevolutionFactory.Create(bits, bitCellTicks, checked((uint)(bits.Count * (long)bitCellTicks))))
    {
    }

    internal HfeTrack(int cylinder, int head, IReadOnlyList<bool> bits, uint bitCellTicks,
        IReadOnlyList<TrackTimingSegment> timing, IReadOnlyList<TrackFeature> features, FluxRevolution revolution)
    {
        Cylinder = cylinder;
        Head = head;
        Bits = Array.AsReadOnly(bits.ToArray());
        BitCellTicks = bitCellTicks;
        Timing = Array.AsReadOnly(timing.ToArray());
        Features = Array.AsReadOnly(features.ToArray());
        Revolution = revolution;
    }

    public int Cylinder { get; }
    public int Head { get; }
    public IReadOnlyList<bool> Bits { get; }
    public uint BitCellTicks { get; }
    public IReadOnlyList<TrackTimingSegment> Timing { get; }
    public IReadOnlyList<TrackFeature> Features { get; }
    public FluxRevolution Revolution { get; }
}
